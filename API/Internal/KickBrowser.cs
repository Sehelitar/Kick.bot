/*
    Copyright (C) 2023-2025 Sehelitar

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kick.Bot;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Kick.API.Internal
{
    internal sealed class KickBrowser : Form
    {
        private const string KickBaseUri = "https://kick.com";
        //private const string KickBrowserStart = "https://dashboard.kick.com/?auth=sign-in";
        private const string KickBrowserStart = "https://kick.com/";
        public string Profile { get; }
        
        private bool _closable;
        
        private AsyncJsBridge Bridge { get; set; }
        private WebView2 WebController { get; set; }

        public event EventHandler<EventArgs> OnReady;
        public event EventHandler<EventArgs> OnAuthenticated;
        
        public bool IsAuthenticated { get; private set; }

        internal KickBrowser(string profile = null) : base()
        {
            Profile = profile;
            
            FormBorderStyle = FormBorderStyle.Sizable;
            Icon = Properties.Resources.Kick;
            MaximizeBox = false;
            MinimizeBox = false;
            ControlBox = true;
            Size = new Size(700, 900);
            MinimumSize = new Size(400, 500);
            MaximumSize = new Size(1000, 1300);
            StartPosition = FormStartPosition.CenterScreen;
            Text = @"Kick - Login";

            FormClosing += (sender, e) =>
            {
                if (_closable) return;
                e.Cancel = true;
                Hide();
            };
            Disposed += (sender, e) =>
            {
                Controls.Clear();
            };

            var done = false;
            var env = CoreWebView2Environment.CreateAsync().GetAwaiter().GetResult();
            var options = env.CreateCoreWebView2ControllerOptions();
            options.ProfileName = "Kick" + (Profile ?? "Broadcaster");
            options.IsInPrivateModeEnabled = false;

            WebController = new WebView2()
            {
                Bounds = new Rectangle(Point.Empty, ClientSize),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            Controls.Add(WebController);
            
            WebController.EnsureCoreWebView2Async(env, options);
            WebController.CoreWebView2InitializationCompleted += (sender, args) =>
            {
                WebController.CoreWebView2.NavigationCompleted += delegate
                {
                    if (done)
                        return;
                    done = true;

                    try
                    {
                        WebController.CoreWebView2.IsMuted = true;
                        WebController.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
                        WebController.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

                        Bridge = new AsyncJsBridge();
                        WebController.CoreWebView2.AddHostObjectToScript("bridge", Bridge);
                        BotClient.CPH.LogDebug($"[Kick] Bridge registered for {options.ProfileName}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
#if DEBUG
                    WebController.CoreWebView2.OpenDevToolsWindow();
#endif
                    OnReady?.Invoke(this, EventArgs.Empty);
                    
                    ExecuteAsyncFetch(
                        null,
                        @"return new Promise((r,j) => {
const loop = () => {
    cookieStore
		.get('session_token')
		.then(cookie => {
            if(cookie) {
                clearInterval(loopInterval);
				r('OK');
			}
		});
};
const loopInterval = setInterval(loop, 500);
});"
                    ).ContinueWith((Task response) =>
                    {
                        if(response.IsFaulted)
                            BotClient.CPH.LogError($"[Kick] An exception occured while looking for authentication status : {response.Exception}");
                        else if (response.IsCompleted)
                        {
                            IsAuthenticated = true;
                            BotClient.CPH.LogInfo($"[Kick] Authenticated with Kick!");
                            OnAuthenticated?.Invoke(this, EventArgs.Empty);
                        }
                    });
                };
                WebController.CoreWebView2.Navigate(KickBrowserStart);
            };
        }
        
        public void BeginAuthentication()
        {
            BotClient.CPH.LogInfo($"[Kick] Attempting to authenticate with Kick... IsAlreadyAuth={IsAuthenticated}");
            if (IsAuthenticated) return;

            // Automatically open login form
            ExecuteScriptAsync(@"(function() { document.evaluate(""/html/body/div/nav/div/button[3]"", document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue.click(); })();");

            // Wait for UI to respond
            Thread.Sleep(200);

            // Hide unsupported UI elements (close button, account creation, SSO login methods...)
            ExecuteScriptAsync(@"(function() {
                let expressions = [
                    ""/html/body/div[3]/div[1]/div/button"", // Close Button
                    ""/html/body/div[3]/div[2]"", // Account Creation
                    ""/html/body/div[3]/div[3]/div/div[1]"", // -OR-
                    ""/html/body/div[3]/div[3]/div/div[2]"", // Ext Auth
                    ""/html/body/div[3]/div[3]/div/form/div[2]/div/div[2]"" // Password Reset
                ];
                let xpath = new XPathEvaluator();
                expressions.forEach(expr => {
                    try {
                        let xnodes = xpath.evaluate(expr, document);
                        let xchild, remChilds = [];
                        while(xchild = xnodes.iterateNext())
                            remChilds.push(xchild);
                        // remChilds.forEach(child => child.parentNode.removeChild(child));
                        remChilds.forEach(child => child.style.display = ""none"");
                    } catch (e) {
                        console.log(`Failed to hide elements at path {expr}`);
                    }
                });
            })();");
            
            // Display window
            Show(BotClient.GlobalPluginUi.ConfigWindow);
        }
        
        public void ForceClose()
        {
            _closable = true;
            Close();
        }

        public void ExecuteScriptAsync(string script)
        {
            if (WebController.InvokeRequired)
                WebController.Invoke((Action)(() => ExecuteScriptAsync(script)));
            else
                WebController.CoreWebView2.ExecuteScriptAsync(script);
        }

        public string ExecuteFetch(string target, string jsPayload = null)
        {
            try
            {
                // Génération du GUID unique
                var actionGuid = Guid.NewGuid().ToString();

                // Préparation du payload Javascript
                if (jsPayload == null)
                {
                    jsPayload = "fetch(\"" + target +
                                "\").then(resp => resp.text()).then(text => chrome.webview.hostObjects.bridge.Resolve(\"" +
                                actionGuid + "\", text));";
                }
                else
                {
                    jsPayload = "(new Promise((r,j) => {\r\ntry { r((() => {\r\n" + jsPayload +
                                "\r\n})()); } catch(e) { j(e.getMessage()); }\r\n})).then(text => chrome.webview.hostObjects.bridge.Resolve(\"" +
                                actionGuid + "\", text));";
                }

                // Préparation de la tâche d'attente
                var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

                // Enregistrement du callback
                string data = null;
                Action<string> callback = delegate(string output)
                {
                    data = output;
                    Bridge.UnregisterCallback(actionGuid);
                    waitHandle.Set();
                };
                Bridge.RegisterCallback(actionGuid, callback);

                // Execution du javascript
                ExecuteScriptAsync(jsPayload);

                // En attente de la réponse
                waitHandle.WaitOne();

                if (string.IsNullOrEmpty(data))
                    throw new Exception("No data received.");

                return data;
            }
            catch (ThreadAbortException)
            {
                return string.Empty;
            }
            catch (Exception ex)
            {
                BotClient.CPH.LogError($"[Kick] An exception occured while executing a Javascript payload : {ex}");
                throw new Exception("An error occured while executing a Javascript payload.", ex);
            }
        }

        public async Task<string> ExecuteAsyncFetch(string target, string jsPayload = null)
        {
            return await Task.Run(() => ExecuteFetch(target, jsPayload));
        }

        public async Task<T> ExecuteAsyncFetch<T>(string target, string jsPayload = null)
        {
            return await Task.Run(() => JsonConvert.DeserializeObject<T>(ExecuteFetch(target, jsPayload)));
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class AsyncJsBridge
    {
        private Dictionary<string, Action<string>> _callbacks = new Dictionary<string, Action<string>>();

        [ComVisible(false)]
        public void RegisterCallback(string id, Action<string> callback)
        {
            _callbacks.Add(id, callback);
            BotClient.CPH.LogDebug($"[Kick] Bridge callback registered (ID: {id})");
        }

        public void Resolve(string id, string data)
        {
            if (!_callbacks.ContainsKey(id)) return;
            BotClient.CPH.LogDebug($"[Kick] Response received for callback ID {id}");
            _callbacks[id].Invoke(data);
        }

        [ComVisible(false)]
        public void UnregisterCallback(string id)
        {
            _callbacks.Remove(id);
        }
    }
}
