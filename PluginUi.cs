using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Kick.Internal;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Kick.Bot
{
    public class PluginUi
    {
        private Thread UiThread { get; set; }
        private SynchronizationContext UiContext { get; set; }
        private ApplicationContext AppContext { get; set; }
        private PluginConfig ConfigWindow { get; set; }
        
        private readonly KickClient _broadcasterClient;
        private readonly KickClient _botClient;

        internal PluginUi(KickClient broadcasterClient, KickClient botClient)
        {
            _broadcasterClient = broadcasterClient;
            _botClient = botClient;
            Initialize();
        }

        public void Dispose() {}

        private void Initialize()
        {
            if (UiThread != null)
                return;

            UiThread = new Thread(() =>
            {
                UiContext = SynchronizationContext.Current;
                
                AppContext = new ApplicationContext();
                ConfigWindow = new PluginConfig();
                AppContext.MainForm = ConfigWindow;
                
                Application.EnableVisualStyles();
                Application.Run(AppContext);
                
                AppContext = null;
                UiThread = null;
            });
            UiThread.SetApartmentState(ApartmentState.STA);
            UiThread.Start();
        }

        ~PluginUi()
        {
            AppContext?.ExitThread();
        }

        public void OpenConfig()
        {
            if (ConfigWindow == null)
                return;

            var matches = ConfigWindow.Controls.Find("broadcasterLoginBtn", true);
            if (matches.Any())
                matches.First().Text = _broadcasterClient.IsAuthenticated ? "Logout" : "Login";
            
            matches = ConfigWindow.Controls.Find("botLoginBtn", true);
            if (matches.Any())
                matches.First().Text = _botClient.IsAuthenticated ? "Logout" : "Login";
            
            matches = ConfigWindow.Controls.Find("broadcasterSocketStatus", true);
            if (matches.Any())
                matches.First().BackColor = _broadcasterClient.GetEventListener().IsConnected ? Color.Green : Color.Red;

            var infos = _broadcasterClient.GetCurrentUserInfos().Result;
            matches = ConfigWindow.Controls.Find("broadcasterName", true);
            if (matches.Any())
                matches.First().Text = infos.Username;
            var channelInfos = _broadcasterClient.GetChannelInfos(infos.StreamerChannel.Slug).Result;
            matches = ConfigWindow.Controls.Find("broadcasterStatus", true);
            if (matches.Any())
                matches.First().Text = channelInfos.IsAffiliate ? "Affiliate" : (channelInfos.IsVerified ? "Verified" : "User");
            
            infos = _botClient.GetCurrentUserInfos().Result;
            matches = ConfigWindow.Controls.Find("botName", true);
            if (matches.Any())
                matches.First().Text = infos.Username;
            channelInfos = _botClient.GetChannelInfos(infos.StreamerChannel.Slug).Result;
            matches = ConfigWindow.Controls.Find("botStatus", true);
            if (matches.Any())
                matches.First().Text = channelInfos.IsAffiliate ? "Affiliate" : (channelInfos.IsVerified ? "Verified" : "User");

            ConfigWindow?.Show();
        }
    }
}