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
        private Thread UiThread { get; set; }
        private SynchronizationContext UiContext { get; set; }
        private ApplicationContext AppContext { get; set; }
        internal PluginConfig ConfigWindow { get; set; }

        internal KickClient BroadcasterKClient { get; private set; }
        internal KickClient BotKClient { get; private set; }

        internal PluginUi()
        {
            var awaitLock = new ManualResetEvent(false);
            UiThread = new Thread(() =>
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                UiContext = SynchronizationContext.Current;
                
                var isNotFirstTime = BotClient.CPH.GetGlobalVar<bool>("KickNotFirstLaunch", true);
                if (!isNotFirstTime)
                {
                    BotClient.CPH.SetGlobalVar("KickNotFirstLaunch", true, true);
                }
                
                ConfigWindow = new PluginConfig();
                ConfigWindow.Load += (sender, args) =>
                {
                    try
                    {
                        BotClient.CPH.LogDebug("[Kick] Initializing web clients...");
                        var browserClient = new KickBrowser();
                        var browserBot = new KickBrowser("BotAcc");
                        BroadcasterKClient = new KickClient(browserClient);
                        BotKClient = new KickClient(browserBot);
                        RefreshUi();
                        BotClient.CPH.LogDebug("[Kick] Web clients loaded, status updated.");

                        browserClient.OnAuthenticated += (o, eventArgs) => RefreshUi();
                        browserBot.OnAuthenticated    += (o, eventArgs) => RefreshUi();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // ignored
                    }
                    finally
                    {
                        awaitLock.Set();
                        if (isNotFirstTime)
                            Task.Run(() => { ConfigWindow.Invoke((Action)(() => { ConfigWindow.Hide(); })); });
                    }
                };
                ConfigWindow.OnLoginRequested += async (isBot) =>
                {
                    BotClient.CPH.LogDebug("[Kick] Starting authentication for " + (isBot ? "bot" : "broadcaster") + " account...");
                    if (isBot)
                    {
                        if (BotKClient.IsAuthenticated)
                        {
                            await BotKClient.Browser.BeginLogout();
                            BotKClient = new KickClient(BotKClient.Browser); // Reset Client
                            RefreshUi();
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
                            BroadcasterKClient = new KickClient(BroadcasterKClient.Browser); // Reset Client
                            RefreshUi();
                        }
                        else
                        {
                            BroadcasterKClient.Browser.BeginAuthentication();
                        }
                    }
                };
                ConfigWindow.OnBroadcastSocketChangeRequested += async () =>
                {
                    if (BroadcasterKClient.GetEventListener().IsConnected)
                    {
                        await BroadcasterKClient.GetEventListener().DisconnectAsync();
                        BotClient.CPH.LogDebug("[Kick] Broadcast socket disconnected.");
                        RefreshUi();
                    }
                    else
                    {
                        await BroadcasterKClient.GetEventListener().ConnectAsync();
                        BotClient.CPH.LogDebug("[Kick] Broadcast socket connected.");
                        RefreshUi();
                    }
                };

                AppContext = new ApplicationContext();
                AppContext.MainForm = ConfigWindow;
                Application.Run(AppContext);
                
                AppContext = null;
                UiThread = null;
            });
            UiThread.SetApartmentState(ApartmentState.STA);
            UiThread.Start();
            awaitLock.WaitOne();
        }

        ~PluginUi()
        {
            AppContext?.ExitThread();
        }

        public void Invoke(MethodInvoker method)
        {
            ConfigWindow.Invoke(method);
        }

        public void OpenConfig()
        {
            if (ConfigWindow == null)
                return;

            if (ConfigWindow.InvokeRequired)
            {
                ConfigWindow.Invoke((Action)OpenConfig);
                return;
            }

            RefreshUi();
            ConfigWindow.Visible = true;
        }

        private void RefreshUi()
        {
            if (ConfigWindow == null)
                return;
            
            BotClient.CPH.LogDebug("[Kick] Refreshing UI...");

            Task.Run(async () =>
            {
                BotClient.CPH.LogDebug($"[Kick] Status : Broadcaster {BroadcasterKClient.IsAuthenticated} / Bot {BotKClient.IsAuthenticated}");
                
                var broadcasterName = "<Déconnecté>";
                var broadcasterStatus = "-";
                Bitmap broadcasterPictureBitmap = null;
                if (BroadcasterKClient.IsAuthenticated)
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
                            else
                            {
                                broadcasterPictureBitmap = Resources.KickLogo;
                            }
                        }
                    }

                    BotClient.CPH.LogDebug($"[Kick] Broadcaster status : {currentUserInfos.Username} (A:{channelInfos.IsAffiliate} / V:{channelInfos.IsVerified})");
                }

                var botName = "<Déconnecté>";
                var botStatus = "-";
                Bitmap botPictureBitmap = null;
                if (BotKClient.IsAuthenticated)
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
                            else
                            {
                                botPictureBitmap = Resources.KickLogo;
                            }
                        }
                    }
                    
                    BotClient.CPH.LogDebug($"[Kick] Bot status : {currentUserInfos.Username} (A:{channelInfos.IsAffiliate} / V:{channelInfos.IsVerified})");
                }

                ConfigWindow.Invoke((Action)(() =>
                {
                    var matches = ConfigWindow.Controls.Find("broadcasterLoginBtn", true);
                    if (matches.Any())
                        matches.First().Text = BroadcasterKClient.IsAuthenticated ? "Logout" : "Login";
            
                    matches = ConfigWindow.Controls.Find("botLoginBtn", true);
                    if (matches.Any())
                        matches.First().Text = BotKClient.IsAuthenticated ? "Logout" : "Login";
            
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
                    matches = ConfigWindow.Controls.Find("broadcasterPusherDisconnect", true);
                    if (matches.Any())
                        matches.First().Enabled = false; //BroadcasterKClient.IsAuthenticated && BroadcasterKClient.GetEventListener().IsConnected;

                    matches = ConfigWindow.Controls.Find("botName", true);
                    if (matches.Any())
                        matches.First().Text = botName;
                    matches = ConfigWindow.Controls.Find("botStatus", true);
                    if (matches.Any())
                        matches.First().Text = botStatus;
                    if (botPictureBitmap != null)
                    {
                        matches = ConfigWindow.Controls.Find("botPicture", true);
                        if (matches.Any())
                        {
                            if(matches.First() is PictureBox firstMatch)
                                firstMatch.Image = botPictureBitmap;
                        }
                    }
                }));
                
                RefreshEventListenerStatus();
            });
        }

        public void RefreshEventListenerStatus()
        {
            if (ConfigWindow == null)
                return;
            
            BotClient.CPH.LogDebug("[Kick] Refreshing EventListener status UI...");
            ConfigWindow.Invoke((Action)(() =>
            {
                var matches = ConfigWindow.Controls.Find("broadcasterSocketStatus", true);
                if (matches.Any())
                    matches.First().BackColor = BroadcasterKClient.IsAuthenticated && BroadcasterKClient.GetEventListener().IsConnected ? Color.SpringGreen : Color.Red;
            }));
            BotClient.CPH.LogDebug("[Kick] UI refreshed.");
        }
    }
}