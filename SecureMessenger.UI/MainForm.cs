using SecureMessenger.Core.Models;
using SecureMessenger.Core.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
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
            _authService = authService;
            _loggedInUsername = loggedInUsername;

            _messageService = new MessageService();
            _userDataService = new UserDataService();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Text = $"Secure Messenger - {_loggedInUsername}";
            lblStatus.Text = $"Logged in as: {_loggedInUsername}";
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

        private void SetupRefreshTimer()
        {
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 5000; // Refresh every 5 seconds
            _refreshTimer.Tick += (sender, e) => LoadConversation();
            _refreshTimer.Start();
        }

        private void LoadConversation()
        {
            if (string.IsNullOrEmpty(_selectedChatUser)) return;

            try
            {
                lstChatHistory.Items.Clear(); // Clear the list
                var conversation = _messageService.GetConversation(_loggedInUsername, _selectedChatUser);

                foreach (var message in conversation)
                {
                    string decryptedText = _messageService.DecryptMessage(message, _loggedInUsername, _authService);
                    string prefix = message.SenderUsername == _loggedInUsername ? "You" : message.SenderUsername;

                    // Create a new ListViewItem
                    var item = new ListViewItem(message.Timestamp.ToString("G"));
                    item.SubItems.Add(prefix);
                    item.SubItems.Add(decryptedText);
                    item.Tag = message; // IMPORTANT: Store the message object here

                    lstChatHistory.Items.Add(item);
                }
                // Auto-scroll to the bottom
                if (lstChatHistory.Items.Count > 0)
                {
                    lstChatHistory.EnsureVisible(lstChatHistory.Items.Count - 1);
                }
            }
            catch (Exception ex)
            {
                // Handle error display for the ListView
                lstChatHistory.Items.Clear();
                var errorItem = new ListViewItem("Error");
                errorItem.SubItems.Add("");
                errorItem.SubItems.Add($"Error loading conversation: {ex.Message}");
                lstChatHistory.Items.Add(errorItem);
            }
        }

        private void lstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                _selectedChatUser = lstUsers.SelectedItem.ToString();
                lstChatHistory.Items.Clear();
                lstChatHistory.Items.Add("Loading conversation...");

                LoadConversation(); // Load the new conversation
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

        private void Logout()
        {
            _authService.Logout();
            _refreshTimer?.Stop();
            this.DialogResult = DialogResult.Retry;
            this.Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.Retry)
            {
                Logout();
                Application.ExitThread();
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Use SelectedItems.Count for ListView
            if (lstChatHistory.SelectedItems.Count == 0)
            {
                e.Cancel = true; // Stop the menu from appearing
                return;
            }

            // Get the item from the SelectedItems collection and get the object from its Tag
            var selectedItem = lstChatHistory.SelectedItems[0];
            var messageToDelete = selectedItem.Tag as SecureMessenger.Core.Models.Message;

            if (messageToDelete == null)
            {
                e.Cancel = true;
                return;
            }

            // Only enable "Delete" if the message was sent by the logged-in user
            bool isMyMessage = messageToDelete.SenderUsername == _loggedInUsername;

            // Index 0 is "Edit", Index 1 is "Delete"
            contextMenuStrip1.Items[0].Enabled = false; // Edit is disabled for now
            contextMenuStrip1.Items[1].Enabled = isMyMessage;
        }

        private void editMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Editing is not yet implemented.", "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void deleteMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstChatHistory.SelectedItems.Count == 0) return;

            // Get the selected item and its associated message object from the Tag
            var selectedItem = lstChatHistory.SelectedItems[0];
            var messageToDelete = selectedItem.Tag as SecureMessenger.Core.Models.Message;

            if (messageToDelete == null) return;

            var result = MessageBox.Show("Are you sure you want to permanently delete this message?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                bool success = _messageService.DeleteMessage(messageToDelete.Id, _loggedInUsername);
                if (success)
                {
                    LoadConversation();
                }
                else
                {
                    MessageBox.Show("Failed to delete the message.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void lstChatHistory_MouseDown(object sender, MouseEventArgs e)
        {
            // We only care about right-clicks
            if (e.Button == MouseButtons.Right)
            {
                // Use GetItemAt for ListView to find the item under the cursor
                var item = lstChatHistory.GetItemAt(e.X, e.Y);

                // If the cursor is over a valid item, select it.
                if (item != null)
                {
                    item.Selected = true;
                }
            }
        }

        private void btnDeleteMessage_Click(object sender, EventArgs e)
        {
            // Checks if any item is selected in the ListView
            if (lstChatHistory.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a message to delete.", "No Message Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Gets the first selected item from the .SelectedItems collection
            var selectedItem = lstChatHistory.SelectedItems[0];

            // Uses the full name to avoid ambiguity with System.Windows.Forms.Message
            var messageToDelete = selectedItem.Tag as SecureMessenger.Core.Models.Message;

            if (messageToDelete == null) return;

            if (messageToDelete.SenderUsername != _loggedInUsername)
            {
                MessageBox.Show("You can only delete messages that you have sent.", "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var confirmResult = MessageBox.Show("Are you sure you want to delete this message?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                // Calls the service layer with the correct parameters
                bool success = _messageService.DeleteMessage(messageToDelete.Id, _loggedInUsername);

                if (success)
                {
                    LoadConversation(); // Refresh the chat
                }
                else
                {
                    MessageBox.Show("Failed to delete the message.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}