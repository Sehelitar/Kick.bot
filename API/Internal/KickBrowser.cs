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
using Microsoft.Web.WebView2.Core;

namespace Kick.API.Internal
{
    internal sealed class KickBrowser : Form
    {
        private const string KickBaseUri = "https://kick.com";
        public string Profile { get; }
        
        private const int NoCloseBtn = 0x200;
        private bool _closable;
        
        private AsyncJsBridge Bridge { get; set; }
        private CoreWebView2Controller WebController { get; set; }

        public event EventHandler<EventArgs> Ready;
        public event EventHandler<EventArgs> Authenticated;

        internal KickBrowser(string profile = null)
        {
            Profile = profile;
            
            FormBorderStyle = FormBorderStyle.Sizable;
            Icon = Properties.Resources.Kick;
            MaximizeBox = false;
            MinimizeBox = false;
            ControlBox = false;
            Size = new Size(700, 900);
            StartPosition = FormStartPosition.CenterScreen;
            Text = @"Kick - Login";

            FormClosing += (sender, e) => {
                if (e.CloseReason == CloseReason.UserClosing && !_closable)
                    e.Cancel = true;
                else
                    Hide();
            };
            Disposed += (sender, e) =>
            {
                Controls.Clear();
            };

            Task.Run(async () =>
            {
                var done = false;
                var env = await CoreWebView2Environment.CreateAsync();
                var options = env.CreateCoreWebView2ControllerOptions();
                options.ProfileName = "Kick" + (Profile ?? "Broadcaster");
                options.IsInPrivateModeEnabled = false;
    
                WebController = await env.CreateCoreWebView2ControllerAsync(Handle, options);
                WebController.Bounds = new Rectangle(Point.Empty, Size);
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
                    }
                    catch
                    {
                        // ignored
                    }
#if DEBUG
                    WebController.CoreWebView2.OpenDevToolsWindow();
#endif
                    Ready?.Invoke(this, EventArgs.Empty);
                    
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
                        TriggerAuthenticated();
                    });
                };
                WebController.CoreWebView2.Navigate(KickBaseUri);
            });
        }

        private void TriggerAuthenticated()
        {
            if(InvokeRequired)
                Invoke((Action)TriggerAuthenticated);
            else
                Authenticated?.Invoke(this, EventArgs.Empty);
        }
        
        public void ForceClose()
        {
            _closable = true;
            Close();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var myCp = base.CreateParams;
                myCp.ClassStyle |= NoCloseBtn;
                return myCp;
            }
        }

        public new void Show()
        {
            if(InvokeRequired)
                Invoke((Action)base.Show);
            else
                base.Show();
        }

        public new void Hide()
        {
            if(InvokeRequired)
                Invoke((Action)base.Hide);
            else
                base.Hide();
        }

        public void ExecuteScriptAsync(string script)
        {
            if (InvokeRequired)
                Invoke((Action)(() => ExecuteScriptAsync(script)));
            else
                WebController.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task<string> ExecuteAsyncFetch(string target, string jsPayload = null)
        {
            return await Task.Run(() => {
                 // Génération du GUID unique
                var actionGuid = Guid.NewGuid().ToString();

                // Préparation du payload Javascript
                if (jsPayload == null)
                {
                    jsPayload = "fetch(\"" + target + "\").then(resp => resp.text()).then(text => chrome.webview.hostObjects.bridge.Resolve(\"" + actionGuid + "\", text));";
                }
                else
                {
                    jsPayload = "(new Promise((r,j) => {\r\ntry { r((() => {\r\n" + jsPayload + "\r\n})()); } catch(e) { j(e.getMessage()); }\r\n})).then(text => chrome.webview.hostObjects.bridge.Resolve(\"" + actionGuid + "\", text));";
                }

                // Préparation de la tâche d'attente
                var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

                // Enregistrement du callback
                string data = null;
                Action<string> callback = delegate (string output)
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
            });
        }

        public async Task<T> ExecuteAsyncFetch<T>(string target, string jsPayload = null)
        {
            return JsonConvert.DeserializeObject<T>(await ExecuteAsyncFetch(target, jsPayload));
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
        }

        public void Resolve(string id, string data)
        {
            if (_callbacks.ContainsKey(id))
            {
                _callbacks[id].Invoke(data);
            }
        }

        [ComVisible(false)]
        public void UnregisterCallback(string id)
        {
            _callbacks.Remove(id);
        }
    }
}
