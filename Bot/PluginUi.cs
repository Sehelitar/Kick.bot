using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Kick.API;

namespace Kick.Bot
{
    public class PluginUi
    {
        private Thread UiThread { get; set; }
        private SynchronizationContext UiContext { get; set; }
        private ApplicationContext AppContext { get; set; }
        private PluginConfig ConfigWindow { get; set; }

        internal KickClient BroadcasterClient { get; private set; }
        internal KickClient BotClient { get; private set; }

        internal PluginUi()
        {
            UiThread = new Thread(() =>
            {
                UiContext = SynchronizationContext.Current;
                
                AppContext = new ApplicationContext();
                ConfigWindow = new PluginConfig();
                AppContext.MainForm = ConfigWindow;
                
                BroadcasterClient = new KickClient();
                BotClient = new KickClient("KickBotProfile");
                
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
                ConfigWindow.Invoke(new MethodInvoker(OpenConfig));
                return;
            }

            var matches = ConfigWindow.Controls.Find("broadcasterLoginBtn", true);
            if (matches.Any())
                matches.First().Text = BroadcasterClient.IsAuthenticated ? "Logout" : "Login";
            
            matches = ConfigWindow.Controls.Find("botLoginBtn", true);
            if (matches.Any())
                matches.First().Text = BotClient.IsAuthenticated ? "Logout" : "Login";
            
            matches = ConfigWindow.Controls.Find("broadcasterSocketStatus", true);
            if (matches.Any())
                matches.First().BackColor = BroadcasterClient.GetEventListener().IsConnected ? Color.Green : Color.Red;

            var infos = BroadcasterClient.GetCurrentUserInfos().Result;
            matches = ConfigWindow.Controls.Find("broadcasterName", true);
            if (matches.Any())
                matches.First().Text = infos.Username;
            var channelInfos = BroadcasterClient.GetChannelInfos(infos.StreamerChannel.Slug).Result;
            matches = ConfigWindow.Controls.Find("broadcasterStatus", true);
            if (matches.Any())
                matches.First().Text = channelInfos.IsAffiliate ? "Affiliate" : (channelInfos.IsVerified ? "Verified" : "User");
            
            infos = BotClient.GetCurrentUserInfos().Result;
            matches = ConfigWindow.Controls.Find("botName", true);
            if (matches.Any())
                matches.First().Text = infos.Username;
            channelInfos = BotClient.GetChannelInfos(infos.StreamerChannel.Slug).Result;
            matches = ConfigWindow.Controls.Find("botStatus", true);
            if (matches.Any())
                matches.First().Text = channelInfos.IsAffiliate ? "Affiliate" : (channelInfos.IsVerified ? "Verified" : "User");

            ConfigWindow?.Show();
        }
    }
}