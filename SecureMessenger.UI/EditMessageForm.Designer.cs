namespace SecureMessenger.UI
{
    partial class EditMessageForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            txtEditMessage = new TextBox();
            btnSaveChanges = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // txtEditMessage
            // 
            txtEditMessage.Dock = DockStyle.Fill;
            txtEditMessage.Location = new Point(0, 0);
            txtEditMessage.Multiline = true;
            txtEditMessage.Name = "txtEditMessage";
            txtEditMessage.ScrollBars = ScrollBars.Vertical;
            txtEditMessage.Size = new Size(800, 450);
            txtEditMessage.TabIndex = 0;
            // 
            // btnSaveChanges
            // 
            btnSaveChanges.Dock = DockStyle.Bottom;
            btnSaveChanges.Location = new Point(0, 421);
            btnSaveChanges.Name = "btnSaveChanges";
            btnSaveChanges.Size = new Size(800, 29);
            btnSaveChanges.TabIndex = 1;
            btnSaveChanges.Text = "Save Changes";
            btnSaveChanges.UseVisualStyleBackColor = true;
            btnSaveChanges.Click += btnSaveChanges_Click;
            // 
            // btnCancel
            // 
            btnCancel.Dock = DockStyle.Bottom;
            btnCancel.Location = new Point(0, 392);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(800, 29);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // EditMessageForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnCancel);
            Controls.Add(btnSaveChanges);
            Controls.Add(txtEditMessage);
            Name = "EditMessageForm";
            Text = "EditMessageForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtEditMessage;
        private Button btnSaveChanges;
        private Button btnCancel;
    }
}