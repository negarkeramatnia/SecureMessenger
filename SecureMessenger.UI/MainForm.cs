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
            if (string.IsNullOrEmpty(_selectedChatUser))
            {
                // Do not clear the chat history if no user is selected, just stop.
                return;
            }

            try
            {
                var conversation = _messageService.GetConversation(_loggedInUsername, _selectedChatUser);

                // --- This section is important to prevent flickering ---
                // We compare the new list with the old list before updating.
                var newItems = new List<ChatMessageDisplay>();
                foreach (var message in conversation)
                {
                    string decryptedText = _messageService.DecryptMessage(message, _loggedInUsername, _authService);
                    string prefix = message.SenderUsername == _loggedInUsername ? "You" : message.SenderUsername;
                    string displayText = $"[{message.Timestamp:G}] {prefix}: {decryptedText}";
                    if (message.IsEdited) displayText += " (edited)";
                    newItems.Add(new ChatMessageDisplay(message, displayText));
                }

                // Check if the conversation has actually changed before redrawing everything.
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
            catch (Exception ex)
            {
                // Do nothing in the timer tick to avoid annoying popups
            }
        }

        private void lstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                _selectedChatUser = lstUsers.SelectedItem.ToString();

                // --- THIS IS THE CORRECTED PART ---
                // We now clear the ListBox and show a loading message in it.
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

        // --- NEW AND CORRECTED METHODS FOR EDIT/DELETE ---
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
            // If nothing is selected in the chat history, don't show the menu.
            if (lstChatHistory.SelectedItem == null)
            {
                e.Cancel = true; // This line stops the menu from appearing.
                return;
            }

            var selectedMessageDisplay = lstChatHistory.SelectedItem as ChatMessageDisplay;
            if (selectedMessageDisplay == null)
            {
                e.Cancel = true;
                return;
            }

            // Only enable the "Delete Message" option if the message was sent by the logged-in user.
            bool isMyMessage = selectedMessageDisplay.OriginalMessage.SenderUsername == _loggedInUsername;

            // The first item (index 0) is "Edit Message", the second (index 1) is "Delete Message".
            contextMenuStrip1.Items[0].Enabled = false; // "Edit Message" is always disabled for now.
            contextMenuStrip1.Items[1].Enabled = isMyMessage; // "Delete Message" is only enabled if it's your message.
        }

        private void editMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Editing is not yet implemented.", "Coming Soon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void deleteMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedMessageDisplay = lstChatHistory.SelectedItem as ChatMessageDisplay;
            if (selectedMessageDisplay == null) return;
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

        private void lstChatHistory_MouseDown(object sender, MouseEventArgs e)
        {
            // We only care about right-clicks
            if (e.Button == MouseButtons.Right)
            {
                // Figure out which item in the list is under the mouse cursor
                int index = this.lstChatHistory.IndexFromPoint(e.Location);

                // If the cursor is over a valid item, select it.
                if (index != ListBox.NoMatches)
                {
                    this.lstChatHistory.SelectedIndex = index;
                }
            }
        }
    }
}