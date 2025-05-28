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
using System.Windows.Forms;

namespace Kick.Bot
{
    public partial class PluginConfig : Form
    {
        internal ToolTip ToolTip { get; } = new ToolTip();
        
        public delegate void OnLoginRequestedHandler(bool isBot);
        public event OnLoginRequestedHandler OnLoginRequested;
        
        public PluginConfig()
        {
            InitializeComponent();
            Icon = Properties.Resources.Kick;
            
            ToolTip.AutoPopDelay = 5000;
            ToolTip.InitialDelay = 500;
            ToolTip.ReshowDelay = 500;
            ToolTip.ShowAlways = false;
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