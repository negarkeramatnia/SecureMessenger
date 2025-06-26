using SecureMessenger.Core.Models;
using SecureMessenger.Core.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
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
            if (string.IsNullOrEmpty(_selectedChatUser))
            {
                return;
            }

            try
            {
                var conversation = _messageService.GetConversation(_loggedInUsername, _selectedChatUser);

                var newItems = new List<ChatMessageDisplay>();
                foreach (var message in conversation)
                {
                    string decryptedText = _messageService.DecryptMessage(message, _loggedInUsername, _authService);
                    string prefix = message.SenderUsername == _loggedInUsername ? "You" : message.SenderUsername;
                    string displayText = $"[{message.Timestamp:G}] {prefix}: {decryptedText}";

                    if (message.IsEdited)
                    {
                        displayText += " (edited)";
                    }

                    newItems.Add(new ChatMessageDisplay(message, displayText));
                }

                // This logic prevents the list from flickering if no new messages have arrived.
                if (lstChatHistory.Items.Count != newItems.Count || !lstChatHistory.Items.Cast<ChatMessageDisplay>().Select(i => i.OriginalMessage.Id).SequenceEqual(newItems.Select(i => i.OriginalMessage.Id)))
                {
                    int selectedIndex = lstChatHistory.SelectedIndex;
                    lstChatHistory.Items.Clear();
                    foreach (var item in newItems)
                    {
                        lstChatHistory.Items.Add(item);
                    }
                    if (selectedIndex != -1 && selectedIndex < lstChatHistory.Items.Count)
                    {
                        lstChatHistory.SelectedIndex = selectedIndex;
                    }
                }
            }
            catch (Exception)
            {
                // Fail silently during a timer refresh to avoid constant popups.
            }
        }

        private void lstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                _selectedChatUser = lstUsers.SelectedItem.ToString();
                lstChatHistory.Items.Clear();
                lstChatHistory.Items.Add("Loading conversation...");
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

        private void lstChatHistory_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var item = lstChatHistory.Items[e.Index] as ChatMessageDisplay;
            if (item == null) return;

            e.DrawBackground();
            TextRenderer.DrawText(e.Graphics, item.DisplayText, e.Font, e.Bounds, Color.Black, TextFormatFlags.Left | TextFormatFlags.WordBreak);
            e.DrawFocusRectangle();
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (lstChatHistory.SelectedItem == null)
            {
                e.Cancel = true; return;
            }
            var selectedMessageDisplay = lstChatHistory.SelectedItem as ChatMessageDisplay;
            if (selectedMessageDisplay == null || selectedMessageDisplay.OriginalMessage == null) // Check for null
            {
                e.Cancel = true; return;
            }
            bool isMyMessage = selectedMessageDisplay.OriginalMessage.SenderUsername == _loggedInUsername;
            contextMenuStrip1.Items[0].Enabled = isMyMessage; // Edit
            contextMenuStrip1.Items[1].Enabled = isMyMessage; // Delete
        }

        // Replace the old editMessageToolStripMenuItem_Click method with this one
        private void editMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedMessageDisplay = lstChatHistory.SelectedItem as ChatMessageDisplay;
            if (selectedMessageDisplay == null) return;

            string originalText = _messageService.DecryptMessage(selectedMessageDisplay.OriginalMessage, _loggedInUsername, _authService);

            using (var editForm = new EditMessageForm(originalText))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    string newText = editForm.EditedMessageText;
                    bool success = _messageService.EditMessage(selectedMessageDisplay.OriginalMessage.Id, newText, _loggedInUsername, _authService);

                    if (success)
                    {
                        LoadConversation();
                    }
                    else
                    {
                        MessageBox.Show("Failed to edit the message.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void deleteMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedMessageDisplay = lstChatHistory.SelectedItem as ChatMessageDisplay;
            if (selectedMessageDisplay == null || selectedMessageDisplay.OriginalMessage == null) return;
            var result = MessageBox.Show("Are you sure you want to permanently delete this message?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                bool success = _messageService.DeleteMessage(selectedMessageDisplay.OriginalMessage.Id, _loggedInUsername);
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

        //private void lstChatHistory_MouseDown(object sender, MouseEventArgs e)
        //{
        //    // We only care about right-clicks
        //    if (e.Button == MouseButtons.Right)
        //    {
        //        // Use GetItemAt for ListView to find the item under the cursor
        //        var item = lstChatHistory.GetItemAt(e.X, e.Y);

        //        // If the cursor is over a valid item, select it.
        //        if (item != null)
        //        {
        //            item.Selected = true;
        //        }
        //    }
        //}

        //private void btnDeleteMessage_Click(object sender, EventArgs e)
        //{
        //    // Checks if any item is selected in the ListView
        //    if (lstChatHistory.SelectedItems.Count == 0)
        //    {
        //        MessageBox.Show("Please select a message to delete.", "No Message Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return;
        //    }

        //    // Gets the first selected item from the .SelectedItems collection
        //    var selectedItem = lstChatHistory.SelectedItems[0];

        //    // Uses the full name to avoid ambiguity with System.Windows.Forms.Message
        //    var messageToDelete = selectedItem.Tag as SecureMessenger.Core.Models.Message;

        //    if (messageToDelete == null) return;

        //    if (messageToDelete.SenderUsername != _loggedInUsername)
        //    {
        //        MessageBox.Show("You can only delete messages that you have sent.", "Delete Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        return;
        //    }

        //    var confirmResult = MessageBox.Show("Are you sure you want to delete this message?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        //    if (confirmResult == DialogResult.Yes)
        //    {
        //        // Calls the service layer with the correct parameters
        //        bool success = _messageService.DeleteMessage(messageToDelete.Id, _loggedInUsername);

        //        if (success)
        //        {
        //            LoadConversation(); // Refresh the chat
        //        }
        //        else
        //        {
        //            MessageBox.Show("Failed to delete the message.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //    }
        //}
    }
}