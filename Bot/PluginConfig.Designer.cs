using System.ComponentModel;

namespace Kick.Bot
{
    partial class PluginConfig
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.broadcasterServices = new System.Windows.Forms.GroupBox();
            this.broadcasterSocketStatus = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.broadcasterLoginBtn = new System.Windows.Forms.Button();
            this.broadcasterStatus = new System.Windows.Forms.Label();
            this.broadcasterName = new System.Windows.Forms.Label();
            this.broadcasterPicture = new System.Windows.Forms.PictureBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.botLoginBtn = new System.Windows.Forms.Button();
            this.botStatus = new System.Windows.Forms.Label();
            this.botName = new System.Windows.Forms.Label();
            this.botPicture = new System.Windows.Forms.PictureBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.broadcasterServices.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.broadcasterSocketStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.broadcasterPicture)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.botPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.broadcasterServices);
            this.groupBox1.Controls.Add(this.broadcasterLoginBtn);
            this.groupBox1.Controls.Add(this.broadcasterStatus);
            this.groupBox1.Controls.Add(this.broadcasterName);
            this.groupBox1.Controls.Add(this.broadcasterPicture);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(386, 300);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Broadcaster Account";
            // 
            // broadcasterServices
            // 
            this.broadcasterServices.Controls.Add(this.broadcasterSocketStatus);
            this.broadcasterServices.Controls.Add(this.label1);
            this.broadcasterServices.Location = new System.Drawing.Point(20, 209);
            this.broadcasterServices.Name = "broadcasterServices";
            this.broadcasterServices.Size = new System.Drawing.Size(350, 74);
            this.broadcasterServices.TabIndex = 4;
            this.broadcasterServices.TabStop = false;
            this.broadcasterServices.Text = "Services";
            // 
            // broadcasterSocketStatus
            // 
            this.broadcasterSocketStatus.BackColor = System.Drawing.Color.Red;
            this.broadcasterSocketStatus.Location = new System.Drawing.Point(315, 33);
            this.broadcasterSocketStatus.Name = "broadcasterSocketStatus";
            this.broadcasterSocketStatus.Size = new System.Drawing.Size(16, 16);
            this.broadcasterSocketStatus.TabIndex = 2;
            this.broadcasterSocketStatus.TabStop = false;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(165, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Pusher WebSocket";
            // 
            // broadcasterLoginBtn
            // 
            this.broadcasterLoginBtn.Location = new System.Drawing.Point(20, 146);
            this.broadcasterLoginBtn.Name = "broadcasterLoginBtn";
            this.broadcasterLoginBtn.Size = new System.Drawing.Size(350, 57);
            this.broadcasterLoginBtn.TabIndex = 3;
            this.broadcasterLoginBtn.Text = "Login";
            this.broadcasterLoginBtn.UseVisualStyleBackColor = true;
            this.broadcasterLoginBtn.Click += new System.EventHandler(this.broadcasterLoginBtn_Click);
            // 
            // broadcasterStatus
            // 
            this.broadcasterStatus.Location = new System.Drawing.Point(122, 81);
            this.broadcasterStatus.Name = "broadcasterStatus";
            this.broadcasterStatus.Size = new System.Drawing.Size(258, 31);
            this.broadcasterStatus.TabIndex = 2;
            this.broadcasterStatus.Text = "Affiliate";
            // 
            // broadcasterName
            // 
            this.broadcasterName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.broadcasterName.Location = new System.Drawing.Point(122, 43);
            this.broadcasterName.Name = "broadcasterName";
            this.broadcasterName.Size = new System.Drawing.Size(258, 30);
            this.broadcasterName.TabIndex = 1;
            this.broadcasterName.Text = "BroadcasterName";
            // 
            // broadcasterPicture
            // 
            this.broadcasterPicture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.broadcasterPicture.Image = global::Kick.Properties.Resources.KickLogo;
            this.broadcasterPicture.InitialImage = global::Kick.Properties.Resources.KickLogo;
            this.broadcasterPicture.Location = new System.Drawing.Point(20, 30);
            this.broadcasterPicture.Name = "broadcasterPicture";
            this.broadcasterPicture.Size = new System.Drawing.Size(96, 96);
            this.broadcasterPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.broadcasterPicture.TabIndex = 0;
            this.broadcasterPicture.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.botLoginBtn);
            this.groupBox2.Controls.Add(this.botStatus);
            this.groupBox2.Controls.Add(this.botName);
            this.groupBox2.Controls.Add(this.botPicture);
            this.groupBox2.Location = new System.Drawing.Point(413, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(382, 223);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Bot Account";
            // 
            // botLoginBtn
            // 
            this.botLoginBtn.Location = new System.Drawing.Point(20, 146);
            this.botLoginBtn.Name = "botLoginBtn";
            this.botLoginBtn.Size = new System.Drawing.Size(350, 57);
            this.botLoginBtn.TabIndex = 5;
            this.botLoginBtn.Text = "Login";
            this.botLoginBtn.UseVisualStyleBackColor = true;
            this.botLoginBtn.Click += new System.EventHandler(this.botLoginBtn_Click);
            // 
            // botStatus
            // 
            this.botStatus.Location = new System.Drawing.Point(122, 81);
            this.botStatus.Name = "botStatus";
            this.botStatus.Size = new System.Drawing.Size(258, 31);
            this.botStatus.TabIndex = 4;
            this.botStatus.Text = "User";
            // 
            // botName
            // 
            this.botName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.botName.Location = new System.Drawing.Point(122, 43);
            this.botName.Name = "botName";
            this.botName.Size = new System.Drawing.Size(258, 30);
            this.botName.TabIndex = 3;
            this.botName.Text = "BotName";
            // 
            // botPicture
            // 
            this.botPicture.Image = global::Kick.Properties.Resources.KickLogo;
            this.botPicture.Location = new System.Drawing.Point(20, 30);
            this.botPicture.Name = "botPicture";
            this.botPicture.Size = new System.Drawing.Size(96, 96);
            this.botPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.botPicture.TabIndex = 0;
            this.botPicture.TabStop = false;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(630, 262);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(165, 50);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // PluginConfig
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(807, 324);
            this.ControlBox = false;
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(829, 380);
            this.Name = "PluginConfig";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Kick.bot Configuration";
            this.TopMost = true;
            this.groupBox1.ResumeLayout(false);
            this.broadcasterServices.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.broadcasterSocketStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.broadcasterPicture)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.botPicture)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button btnClose;

        private System.Windows.Forms.PictureBox broadcasterSocketStatus;

        private System.Windows.Forms.Label label1;

        private System.Windows.Forms.Button botLoginBtn;
        private System.Windows.Forms.GroupBox broadcasterServices;

        private System.Windows.Forms.Button broadcasterLoginBtn;

        private System.Windows.Forms.Label botStatus;
        private System.Windows.Forms.Label botName;

        private System.Windows.Forms.Label broadcasterStatus;

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.PictureBox botPicture;

        private System.Windows.Forms.Label broadcasterName;

        private System.Windows.Forms.PictureBox broadcasterPicture;

        private System.Windows.Forms.GroupBox groupBox1;

        #endregion
    }
}