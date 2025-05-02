using System;
using System.Windows.Forms;

namespace Kick.Bot
{
    public partial class PluginConfig : Form
    {
        public delegate void OnLoginRequestedHandler(bool isBot);
        public event OnLoginRequestedHandler OnLoginRequested;
        
        public PluginConfig()
        {
            InitializeComponent();
            Icon = Properties.Resources.Kick;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void broadcasterLoginBtn_Click(object sender, EventArgs e)
        {
            OnLoginRequested?.Invoke(false);
        }

        private void botLoginBtn_Click(object sender, EventArgs e)
        {
            OnLoginRequested?.Invoke(true);
        }
    }
}