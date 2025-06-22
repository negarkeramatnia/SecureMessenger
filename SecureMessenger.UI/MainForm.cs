using System;
using System.Windows.Forms;

namespace SecureMessenger.UI
{
    public partial class MainForm : Form
    {
        private string _loggedInUsername;

        public MainForm(string loggedInUsername)
        {
            InitializeComponent();
            _loggedInUsername = loggedInUsername;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // This event runs when the form is first loaded
            this.Text = $"Secure Messenger - {_loggedInUsername}";
            lblStatus.Text = $"Logged in as: {_loggedInUsername}";

            // For now, let's just add the current user to the list.
            // Later, this would be populated with all registered users.
            lstUsers.Items.Add(_loggedInUsername);
            foreach (var user in UserStore.Users)
            {
                if (user.Username != _loggedInUsername && !lstUsers.Items.Contains(user.Username))
                {
                    lstUsers.Items.Add(user.Username);
                }
            }
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            string message = txtMessageInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            // In the real app, this would be encrypted and sent to the selected user.
            // For now, we just display it locally.
            string formattedMessage = $"[{DateTime.Now:HH:mm:ss}] You: {message}{Environment.NewLine}";
            txtChatHistory.AppendText(formattedMessage);

            txtMessageInput.Clear();
            txtMessageInput.Focus();

        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Set a flag that we are logging out, so the application can restart
            this.DialogResult = DialogResult.Retry;
            this.Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If we are not logging out, closing the main form should exit the app.
            if (this.DialogResult != DialogResult.Retry)
            {
                Application.Exit();
            }
        }
    }
}