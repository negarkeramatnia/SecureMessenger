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
        private readonly UserDataService _userDataService;
        private readonly MessageService _messageService;
        private readonly AuthService _authService; // We need this to get keys

        // --- State Management ---
        private readonly string _loggedInUsername;
        private User _loggedInUser;
        private byte[] _loggedInUserPrivateKey;
        private string _selectedChatUser;
        private System.Windows.Forms.Timer _refreshTimer;

        public MainForm(string loggedInUsername, byte[] decryptedPrivateKey)
        {
            InitializeComponent();
            _loggedInUsername = loggedInUsername;
            _loggedInUserPrivateKey = decryptedPrivateKey;

            // Initialize services
            _userDataService = new UserDataService();
            _messageService = new MessageService();
            _authService = new AuthService(new CryptoService());
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Text = $"Secure Messenger - {_loggedInUsername}";
            lblStatus.Text = $"Logged in as: {_loggedInUsername}";

            _loggedInUser = _userDataService.GetUserByUsername(_loggedInUsername);
            if (_loggedInUser == null)
            {
                MessageBox.Show("Could not load your user profile. Logging out.", "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logout();
                return;
            }

            PopulateUserList();
            SetupRefreshTimer();
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
                    if (username != _loggedInUsername)
                    {
                        lstUsers.Items.Add(username);
                    }
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

        private void SetupRefreshTimer()
        {
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 5000; // Refresh every 5 seconds
            _refreshTimer.Tick += (sender, e) => LoadConversation(); // On each tick, call LoadConversation
            _refreshTimer.Start();
        }

        private void lstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                _selectedChatUser = lstUsers.SelectedItem.ToString();
                LoadConversation();
            }
        }

        private void LoadConversation()
        {
            if (string.IsNullOrEmpty(_selectedChatUser))
            {
                txtChatHistory.Text = "Select a user to view the conversation.";
                return;
            }

            try
            {
                var conversation = _messageService.GetConversation(_loggedInUsername, _selectedChatUser);
                var chatHistory = new System.Text.StringBuilder();

                foreach (var message in conversation)
                {
                    string decryptedText = _messageService.DecryptMessage(message, _loggedInUsername, _loggedInUserPrivateKey);
                    string prefix = message.SenderUsername == _loggedInUsername ? "You" : message.SenderUsername;
                    chatHistory.AppendLine($"[{message.Timestamp:G}] {prefix}: {decryptedText}");
                }

                // Only update the textbox if the content has changed to prevent flickering
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

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedChatUser))
            {
                MessageBox.Show("Please select a user to send a message to.", "No Recipient Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string messageText = txtMessageInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(messageText)) return;

            bool success = _messageService.SendMessage(_loggedInUsername, _selectedChatUser, messageText, _loggedInUserPrivateKey, _loggedInUser);

            if (success)
            {
                txtMessageInput.Clear();
                LoadConversation(); // Immediately refresh the chat after sending
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
                Logout(); // Ensure keys are cleared if user closes with 'X'
                Application.Exit();
            }
        }

        private void Logout()
        {
            // Clear the sensitive private key from memory
            if (_loggedInUserPrivateKey != null)
            {
                Array.Clear(_loggedInUserPrivateKey, 0, _loggedInUserPrivateKey.Length);
            }
            _refreshTimer?.Stop();
            this.DialogResult = DialogResult.Retry; // Signal to Program.cs to show login form
            this.Close();
        }
    }
}