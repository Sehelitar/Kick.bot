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

using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kick.API;
using Kick.API.Internal;
using Kick.Properties;

namespace Kick.Bot
{
    public class PluginUi
    {
        internal PluginConfig ConfigWindow { get; set; }

        internal KickClient BroadcasterKClient { get; private set; }
        internal KickClient BotKClient { get; private set; }

        internal PluginUi()
        {
            var awaitLock = new ManualResetEvent(false);
            var uiThread = new Thread(() =>
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                InitUi();
                
                awaitLock.Set();
                try
                {
                    Application.Run();
                }
                catch (ThreadAbortException)
                {
                    // ignored
                }
                catch(ThreadInterruptedException) {
                    //ignored
                }
            });
            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();
            awaitLock.WaitOne();
        }

        private void InitUi()
        {
            BotClient.CPH.LogDebug("[Kick] Initializing web clients...");
            var browserClient = new KickBrowser();
            var browserBot = new KickBrowser("BotAcc");
            BroadcasterKClient = new KickClient(browserClient);
            BotKClient = new KickClient(browserBot);
            
            browserClient.OnAuthenticated += (o, eventArgs) => RefreshUi().Wait();
            browserBot.OnAuthenticated += (o, eventArgs) => RefreshUi().Wait();

            browserClient.OnReady += (o, eventArgs) =>
            {
                var matches = ConfigWindow.Controls.Find("broadcasterLoginBtn", true);
                if (matches.Any())
                    matches.First().Enabled = true;
            };
            browserBot.OnReady += (o, eventArgs) =>
            {
                var matches = ConfigWindow.Controls.Find("botLoginBtn", true);
                if (matches.Any())
                    matches.First().Enabled = true;
            };
            
            ConfigWindow = new PluginConfig();
            ConfigWindow.OnLoginRequested += async (isBot) =>
            {
                BotClient.CPH.LogDebug("[Kick] Starting authentication for " + (isBot ? "bot" : "broadcaster") + " account...");
                if (isBot)
                {
                    if (BotKClient.IsAuthenticated)
                    {
                        await BotKClient.Browser.BeginLogout();
                        Invoke(() =>
                        {
                            BotKClient = new KickClient(BotKClient.Browser); // Reset Client                            
                        });
                        await RefreshUi();
                    }
                    else
                    {
                        BotKClient.Browser.BeginAuthentication();
                    }
                }
                else
                {
                    if (BroadcasterKClient.IsAuthenticated)
                    {
                        await BroadcasterKClient.Browser.BeginLogout();
                        Invoke(() =>
                        {
                            BroadcasterKClient = new KickClient(BroadcasterKClient.Browser); // Reset Client
                        });
                        await RefreshUi();
                    }
                    else
                    {
                        BroadcasterKClient.Browser.BeginAuthentication();
                    }
                }
            };
            ConfigWindow.Show();
            
            var isFirstLaunch = !BotClient.CPH.GetGlobalVar<bool>("KickNotFirstLaunch");
            if (!isFirstLaunch)
                ConfigWindow.Hide();
        }

        private void Invoke(MethodInvoker method)
        {
            if(ConfigWindow.InvokeRequired)
                ConfigWindow.Invoke(method);
            else
                method();
        }

        public void OpenConfig()
        {
            BotClient.CPH.LogDebug("[Kick] Trying to refresh UI...");
            _ = RefreshUi();
            BotClient.CPH.LogDebug("[Kick] Opening configuration window...");
            Invoke(() => ConfigWindow.Show());
            BotClient.CPH.LogDebug("[Kick] Done!");
        }

        private async Task RefreshUi()
        {
            if (ConfigWindow == null)
                return;
            
            BotClient.CPH.LogDebug("[Kick] Refreshing UI...");
            BotClient.CPH.LogDebug($"[Kick] Status : Broadcaster {BroadcasterKClient.IsAuthenticated} / Bot {BotKClient.IsAuthenticated}");
            
            var broadcasterName = "<Disconnected>";
            var broadcasterStatus = "-";
            var broadcasterPictureBitmap = Resources.KickLogo;
            if (BroadcasterKClient.IsReady && BroadcasterKClient.IsAuthenticated)
            {
                var currentUserInfos = await BroadcasterKClient.GetCurrentUserInfos();
                broadcasterName = currentUserInfos.Username;
                var channelInfos = await BroadcasterKClient.GetChannelInfos(currentUserInfos.StreamerChannel.Slug);
                broadcasterStatus = channelInfos.IsAffiliate
                    ? "Affiliate"
                    : (channelInfos.IsVerified ? "Verified" : "User");
                var broadcasterPicture = currentUserInfos.ProfilePic ?? currentUserInfos.ProfilePicAlt;

                if (broadcasterPicture != null)
                {
                    var decoder = new Imazen.WebP.SimpleDecoder();
                    using (var stream = System.Net.WebRequest
                               .CreateHttp(broadcasterPicture)
                               .GetResponse().GetResponseStream())
                    using (var memoryStream = new System.IO.MemoryStream())
                    {
                        if (stream != null) {
                            await stream.CopyToAsync(memoryStream);
                            var pictureBuffer = memoryStream.ToArray();
                            broadcasterPictureBitmap = decoder.DecodeFromBytes(pictureBuffer, pictureBuffer.Length);
                        }
                    }
                }

                BotClient.CPH.LogDebug($"[Kick] Broadcaster status : {currentUserInfos.Username} (A:{channelInfos.IsAffiliate} / V:{channelInfos.IsVerified})");
            }

            var botName = "<Disconnected>";
            var botStatus = "-";
            var botPictureBitmap = Resources.KickLogo;
            if (BotKClient.IsReady && BotKClient.IsAuthenticated)
            {
                var currentUserInfos = await BotKClient.GetCurrentUserInfos();
                botName = currentUserInfos.Username;
                var channelInfos = await BotKClient.GetChannelInfos(currentUserInfos.StreamerChannel.Slug);
                botStatus = channelInfos.IsAffiliate
                    ? "Affiliate"
                    : (channelInfos.IsVerified ? "Verified" : "User");
                var botPicture = currentUserInfos.ProfilePic ?? currentUserInfos.ProfilePicAlt;
                
                if (botPicture != null)
                {
                    var decoder = new Imazen.WebP.SimpleDecoder();
                    using (var stream = System.Net.WebRequest
                               .CreateHttp(botPicture)
                               .GetResponse().GetResponseStream())
                    using (var memoryStream = new System.IO.MemoryStream())
                    {
                        if (stream != null) {
                            await stream.CopyToAsync(memoryStream);
                            var pictureBuffer = memoryStream.ToArray();
                            botPictureBitmap = decoder.DecodeFromBytes(pictureBuffer, pictureBuffer.Length);
                        }
                    }
                }
                
                BotClient.CPH.LogDebug($"[Kick] Bot status : {currentUserInfos.Username} (A:{channelInfos.IsAffiliate} / V:{channelInfos.IsVerified})");
            }

            BotClient.CPH.LogDebug($"[Kick] Syncing UI");
            Invoke(() =>
            {
                var matches = ConfigWindow.Controls.Find("broadcasterLoginBtn", true);
                if (matches.Any())
                {
                    matches.First().Enabled = BroadcasterKClient.IsReady;
                    matches.First().Text = BroadcasterKClient.IsAuthenticated ? "Logout" : "Login";
                }

                matches = ConfigWindow.Controls.Find("botLoginBtn", true);
                if (matches.Any())
                {
                    matches.First().Enabled = BotKClient.IsReady;
                    matches.First().Text = BotKClient.IsAuthenticated ? "Logout" : "Login";
                }

                matches = ConfigWindow.Controls.Find("broadcasterName", true);
                if (matches.Any())
                    matches.First().Text = broadcasterName;
                matches = ConfigWindow.Controls.Find("broadcasterStatus", true);
                if (matches.Any())
                    matches.First().Text = broadcasterStatus;
                if (broadcasterPictureBitmap != null)
                {
                    matches = ConfigWindow.Controls.Find("broadcasterPicture", true);
                    if (matches.Any())
                    {
                        if(matches.First() is PictureBox firstMatch)
                            firstMatch.Image = broadcasterPictureBitmap;
                    }
                }

                matches = ConfigWindow.Controls.Find("botName", true);
                if (matches.Any())
                    matches.First().Text = botName;
                matches = ConfigWindow.Controls.Find("botStatus", true);
                if (matches.Any())
                    matches.First().Text = botStatus;
                if (botPictureBitmap != null)
                {
                    matches = ConfigWindow.Controls.Find("botPicture", true);
                    if(matches.Any() && matches.First() is PictureBox firstMatch)
                        firstMatch.Image = botPictureBitmap;
                }
            });
            
            RefreshEventListenerStatus();
        }

        public void RefreshEventListenerStatus()
        {
            if (ConfigWindow == null)
                return;
            
            BotClient.CPH.LogDebug("[Kick] Refreshing EventListener status UI...");
            Invoke(() =>
            {
                var matches = ConfigWindow.Controls.Find("broadcasterSocketStatus", true);
                if (!matches.Any()) return;

                var control = matches.First();
                var isConnected = BroadcasterKClient.IsAuthenticated &&
                                  BroadcasterKClient.GetEventListener().IsConnected;
                
                ConfigWindow.ToolTip.SetToolTip(control, isConnected ? "Connected" : "Disconnected");
                control.BackColor = isConnected ? Color.SpringGreen : Color.Red;
            });
            BotClient.CPH.LogDebug("[Kick] UI refreshed.");
        }
    }
}