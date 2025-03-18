using System;
using System.Windows.Forms;

namespace Kick.Bot
{
    public partial class PluginConfig : Form
    {
        public PluginConfig()
        {
            InitializeComponent();
            Icon = Properties.Resources.Kick;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Hide();
        }
    }
}