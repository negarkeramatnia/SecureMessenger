using SecureMessenger.Core.Models;
using SecureMessenger.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SecureMessenger.UI
{
    public partial class MainForm : Form
    {
        // --- Services ---
        private readonly AuthService _authService;
        private readonly MessageService _messageService;
        private readonly UserDataService _userDataService;

        // --- State Management ---
        private readonly string _loggedInUsername;
        private string _selectedChatUser;
        private System.Windows.Forms.Timer _refreshTimer;

        public MainForm(string loggedInUsername, AuthService authService)
        {
            InitializeComponent();
            _loggedInUsername = loggedInUsername;
            _authService = authService;

            // Initialize our services
            _messageService = new MessageService();
            _userDataService = new UserDataService();
        }

        // --- THIS METHOD WAS MISSING ---
        // It runs automatically when the form first opens.
        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Text = $"Secure Messenger - {_loggedInUsername}";
            lblStatus.Text = $"Logged in as: {_loggedInUsername}"; // Fixes the status label

            PopulateUserList(); // Populates the user list on startup
            SetupRefreshTimer(); // Starts the auto-refresh timer
        }

        private void PopulateUserList()
        {
            string previouslySelected = _selectedChatUser;
            lstUsers.Items.Clear();
            try
            {
                List<string> allUsernames = _userDataService.GetAllUsernames();
                foreach (string username in allUsernames)
                {
                    // This adds all users to the list
                    lstUsers.Items.Add(username);
                }

                if (!string.IsNullOrEmpty(previouslySelected) && lstUsers.Items.Contains(previouslySelected))
                {
                    lstUsers.SelectedItem = previouslySelected;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load user list: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- THIS METHOD WAS MISSING ---
        private void SetupRefreshTimer()
        {
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 5000; // Refresh every 5 seconds
            _refreshTimer.Tick += (sender, e) => LoadConversation();
            _refreshTimer.Start();
        }

        private void LoadConversation()
        {
            if (string.IsNullOrEmpty(_selectedChatUser))
            {
                // This line prevents trying to load a conversation when no one is selected
                return;
            }
            try
            {
                var conversation = _messageService.GetConversation(_loggedInUsername, _selectedChatUser);
                var chatHistory = new System.Text.StringBuilder();

                foreach (var message in conversation)
                {
                    string decryptedText = _messageService.DecryptMessage(message, _loggedInUsername, _authService);
                    string prefix = message.SenderUsername == _loggedInUsername ? "You" : message.SenderUsername;
                    chatHistory.AppendLine($"[{message.Timestamp:G}] {prefix}: {decryptedText}");
                }

                if (txtChatHistory.Text != chatHistory.ToString())
                {
                    txtChatHistory.Text = chatHistory.ToString();
                    txtChatHistory.SelectionStart = txtChatHistory.Text.Length;
                    txtChatHistory.ScrollToCaret();
                }
            }
            catch (Exception ex)
            {
                txtChatHistory.Text = $"Error loading conversation: {ex.Message}";
            }
        }

        // This is the single, correct event handler for the user list
        private void lstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                _selectedChatUser = lstUsers.SelectedItem.ToString();
                txtChatHistory.Text = $"Loading conversation with {_selectedChatUser}..."; // Give instant feedback
                LoadConversation();
            }
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedChatUser))
            {
                MessageBox.Show("Please select a user to send a message to.", "No Recipient Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string messageText = txtMessageInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(messageText)) return;

            bool success = _messageService.SendMessage(_loggedInUsername, _selectedChatUser, messageText, _authService);

            if (success)
            {
                txtMessageInput.Clear();
                LoadConversation();
            }
            else
            {
                MessageBox.Show("Failed to send the message.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logout();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.Retry)
            {
                Logout();
                Application.ExitThread();
            }
        }

        private void Logout()
        {
            _authService.Logout();
            _refreshTimer?.Stop();
            this.DialogResult = DialogResult.Retry;
            this.Close();
        }
    }
}