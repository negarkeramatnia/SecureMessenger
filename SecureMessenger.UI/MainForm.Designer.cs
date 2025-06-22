namespace SecureMessenger.UI
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            logoutToolStripMenuItem = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            lstUsers = new ListBox();
            txtChatHistory = new TextBox();
            lblStatus = new ToolStripStatusLabel();
            txtMessageInput = new TextBox();
            btnSendMessage = new Button();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, logoutToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 28);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "(Add items below)";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(46, 24);
            fileToolStripMenuItem.Text = "File";
            // 
            // logoutToolStripMenuItem
            // 
            logoutToolStripMenuItem.Name = "logoutToolStripMenuItem";
            logoutToolStripMenuItem.Size = new Size(70, 24);
            logoutToolStripMenuItem.Text = "Logout";
            logoutToolStripMenuItem.Click += logoutToolStripMenuItem_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(20, 20);
            statusStrip1.Items.AddRange(new ToolStripItem[] { lblStatus });
            statusStrip1.Location = new Point(0, 424);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(800, 26);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // lstUsers
            // 
            lstUsers.FormattingEnabled = true;
            lstUsers.Location = new Point(12, 31);
            lstUsers.Name = "lstUsers";
            lstUsers.Size = new Size(150, 344);
            lstUsers.TabIndex = 2;
            //lstUsers.SelectedIndexChanged += lstUsers_SelectedIndexChanged;
            // 
            // txtChatHistory
            // 
            txtChatHistory.Location = new Point(177, 31);
            txtChatHistory.Multiline = true;
            txtChatHistory.Name = "txtChatHistory";
            txtChatHistory.ReadOnly = true;
            txtChatHistory.Size = new Size(611, 344);
            txtChatHistory.TabIndex = 4;
            //txtChatHistory.TextChanged += txtChatHistory_TextChanged;
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(97, 20);
            lblStatus.Text = "Logged in as:";
            // 
            // txtMessageInput
            // 
            txtMessageInput.Location = new Point(12, 394);
            txtMessageInput.Name = "txtMessageInput";
            txtMessageInput.Size = new Size(676, 27);
            txtMessageInput.TabIndex = 5;
            // 
            // btnSendMessage
            // 
            btnSendMessage.Location = new Point(706, 392);
            btnSendMessage.Name = "btnSendMessage";
            btnSendMessage.Size = new Size(94, 29);
            btnSendMessage.TabIndex = 6;
            btnSendMessage.Text = "Send";
            btnSendMessage.UseVisualStyleBackColor = true;
            btnSendMessage.Click += btnSendMessage_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnSendMessage);
            Controls.Add(txtMessageInput);
            Controls.Add(txtChatHistory);
            Controls.Add(lstUsers);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "MainForm";
            Text = "Form1";
            Load += MainForm_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem logoutToolStripMenuItem;
        private StatusStrip statusStrip1;
        private ListBox lstUsers;
        private TextBox txtChatHistory;
        private ToolStripStatusLabel lblStatus;
        private TextBox txtMessageInput;
        private Button btnSendMessage;
    }
}
