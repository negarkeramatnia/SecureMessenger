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
            components = new System.ComponentModel.Container();
            menuStrip1 = new MenuStrip();
            logoutToolStripMenuItem = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            lstUsers = new ListBox();
            txtMessageInput = new TextBox();
            btnSendMessage = new Button();
            contextMenuStrip1 = new ContextMenuStrip(components);
            editMessageToolStripMenuItem = new ToolStripMenuItem();
            deleteMessageToolStripMenuItem = new ToolStripMenuItem();
            lstChatHistory = new ListView();
            Timestamp = new ColumnHeader();
            Sender = new ColumnHeader();
            Message = new ColumnHeader();
            btnDeleteMessage = new Button();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { logoutToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 28);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "(Add items below)";
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
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(97, 20);
            lblStatus.Text = "Logged in as:";
            // 
            // lstUsers
            // 
            lstUsers.FormattingEnabled = true;
            lstUsers.Location = new Point(12, 29);
            lstUsers.Name = "lstUsers";
            lstUsers.Size = new Size(139, 344);
            lstUsers.TabIndex = 2;
            lstUsers.SelectedIndexChanged += lstUsers_SelectedIndexChanged;
            // 
            // txtMessageInput
            // 
            txtMessageInput.Location = new Point(12, 393);
            txtMessageInput.Name = "txtMessageInput";
            txtMessageInput.Size = new Size(631, 27);
            txtMessageInput.TabIndex = 5;
            // 
            // btnSendMessage
            // 
            btnSendMessage.Location = new Point(721, 391);
            btnSendMessage.Name = "btnSendMessage";
            btnSendMessage.Size = new Size(67, 29);
            btnSendMessage.TabIndex = 6;
            btnSendMessage.Text = "Send";
            btnSendMessage.UseVisualStyleBackColor = true;
            btnSendMessage.Click += btnSendMessage_Click;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(20, 20);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { editMessageToolStripMenuItem, deleteMessageToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(185, 52);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // editMessageToolStripMenuItem
            // 
            editMessageToolStripMenuItem.Name = "editMessageToolStripMenuItem";
            editMessageToolStripMenuItem.Size = new Size(184, 24);
            editMessageToolStripMenuItem.Text = "Edit Message";
            editMessageToolStripMenuItem.Click += editMessageToolStripMenuItem_Click;
            // 
            // deleteMessageToolStripMenuItem
            // 
            deleteMessageToolStripMenuItem.Name = "deleteMessageToolStripMenuItem";
            deleteMessageToolStripMenuItem.Size = new Size(184, 24);
            deleteMessageToolStripMenuItem.Text = "Delete Message";
            deleteMessageToolStripMenuItem.Click += deleteMessageToolStripMenuItem_Click;
            // 
            // lstChatHistory
            // 
            lstChatHistory.Columns.AddRange(new ColumnHeader[] { Timestamp, Sender, Message });
            lstChatHistory.Location = new Point(157, 31);
            lstChatHistory.Name = "lstChatHistory";
            lstChatHistory.Size = new Size(631, 342);
            lstChatHistory.TabIndex = 7;
            lstChatHistory.UseCompatibleStateImageBehavior = false;
            lstChatHistory.View = View.Details;
            // 
            // Timestamp
            // 
            Timestamp.Width = 100;
            // 
            // Sender
            // 
            Sender.Width = 100;
            // 
            // Message
            // 
            Message.Width = 800;
            // 
            // btnDeleteMessage
            // 
            btnDeleteMessage.Location = new Point(649, 392);
            btnDeleteMessage.Name = "btnDeleteMessage";
            btnDeleteMessage.Size = new Size(67, 29);
            btnDeleteMessage.TabIndex = 8;
            btnDeleteMessage.Text = "Delete";
            btnDeleteMessage.UseVisualStyleBackColor = true;
            btnDeleteMessage.Click += btnDeleteMessage_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnDeleteMessage);
            Controls.Add(lstChatHistory);
            Controls.Add(btnSendMessage);
            Controls.Add(txtMessageInput);
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
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem logoutToolStripMenuItem;
        private StatusStrip statusStrip1;
        private ListBox lstUsers;
        private ToolStripStatusLabel lblStatus;
        private TextBox txtMessageInput;
        private Button btnSendMessage;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem editMessageToolStripMenuItem;
        private ToolStripMenuItem deleteMessageToolStripMenuItem;
        private ListView lstChatHistory;
        private ColumnHeader Timestamp;
        private ColumnHeader Sender;
        private ColumnHeader Message;
        private Button btnDeleteMessage;
    }
}
