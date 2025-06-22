using SecureMessenger.Core.Services;
using System;
using System.Windows.Forms;

namespace SecureMessenger.UI
{
    public partial class MainForm : Form
    {
        private string _loggedInUsername;

        private readonly UserDataService _userDataService;

        public MainForm(string loggedInUsername)
        {
            InitializeComponent();
            _loggedInUsername = loggedInUsername;
            // Initialize the service here
            _userDataService = new UserDataService();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Text = $"Secure Messenger - {_loggedInUsername}";
            lblStatus.Text = $"Logged in as: {_loggedInUsername}";

            // --- NEW LOGIC TO POPULATE THE USER LIST ---
            PopulateUserList();
        }

        private void PopulateUserList()
        {
            // Clear any existing items from the list box
            lstUsers.Items.Clear();

            try
            {
                // Call our new method to get all usernames from the database
                List<string> allUsernames = _userDataService.GetAllUsernames();

                // Loop through the list of usernames
                foreach (string username in allUsernames)
                {
                    // We don't want to show the logged-in user in the list of people to chat with
                    if (username != _loggedInUsername)
                    {
                        lstUsers.Items.Add(username);
                    }
                }
            }
            catch (Exception ex)
            {
                // If there's an error connecting to the DB or fetching users, show an error message
                MessageBox.Show($"Failed to load user list: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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