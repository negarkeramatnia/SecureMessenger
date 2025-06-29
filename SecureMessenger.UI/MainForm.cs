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
        private readonly AuthService _authService;
        private readonly MessageService _messageService;
        private readonly UserDataService _userDataService;
        private readonly string _loggedInUsername;
        private string _selectedChatUser;
        private System.Windows.Forms.Timer _refreshTimer;
        private bool _isLoadingConversation = false;

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
            _refreshTimer.Interval = 5000;
            _refreshTimer.Tick += (s, e) => LoadConversation();
            _refreshTimer.Start();
        }

        private void LoadConversation()
        {
            if (_isLoadingConversation) return;
            if (string.IsNullOrEmpty(_selectedChatUser)) return;

            try
            {
                _isLoadingConversation = true;
                var fullConversation = _messageService.GetConversation(_loggedInUsername, _selectedChatUser);

                var newItems = new List<ChatMessageDisplay>();
                foreach (var message in fullConversation)
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

                var currentIds = lstChatHistory.Items.Cast<ChatMessageDisplay>().Where(i => i.OriginalMessage != null).Select(item => item.OriginalMessage.Id);
                var newIds = newItems.Select(item => item.OriginalMessage.Id);

                if (!currentIds.SequenceEqual(newIds))
                {
                    int selectedIndex = lstChatHistory.SelectedIndex;
                    lstChatHistory.Items.Clear();
                    foreach (var item in newItems) lstChatHistory.Items.Add(item);
                    if (selectedIndex > -1 && selectedIndex < lstChatHistory.Items.Count)
                    {
                        lstChatHistory.SelectedIndex = selectedIndex;
                    }
                }
            }
            finally
            {
                _isLoadingConversation = false;
            }
        }

        private void lstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                _selectedChatUser = lstUsers.SelectedItem.ToString();
                lstChatHistory.Items.Clear();
                lstChatHistory.Items.Add(new ChatMessageDisplay(null, "Loading conversation..."));
                LoadConversation();
            }
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedChatUser))
            {
                MessageBox.Show("Please select a user.", "No Recipient", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string messageText = txtMessageInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(messageText)) return;

            if (_messageService.SendMessage(_loggedInUsername, _selectedChatUser, messageText, _authService))
            {
                txtMessageInput.Clear();
                LoadConversation();
            }
            else
            {
                MessageBox.Show("Failed to send message.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void lstChatHistory_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int index = this.lstChatHistory.IndexFromPoint(e.Location);
                if (index != ListBox.NoMatches)
                {
                    this.lstChatHistory.SelectedIndex = index;
                }
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (lstChatHistory.SelectedItem == null)
            {
                e.Cancel = true; return;
            }
            var selectedMessageDisplay = lstChatHistory.SelectedItem as ChatMessageDisplay;
            if (selectedMessageDisplay == null || selectedMessageDisplay.OriginalMessage == null)
            {
                e.Cancel = true; return;
            }

            bool isMyMessage = selectedMessageDisplay.OriginalMessage.SenderUsername == _loggedInUsername;

            contextMenuStrip1.Items[0].Enabled = isMyMessage;
            contextMenuStrip1.Items[1].Enabled = isMyMessage;
        }

        private void editMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedMessageDisplay = lstChatHistory.SelectedItem as ChatMessageDisplay;
            if (selectedMessageDisplay == null || selectedMessageDisplay.OriginalMessage == null) return;

            string originalText = _messageService.DecryptMessage(selectedMessageDisplay.OriginalMessage, _loggedInUsername, _authService);

            using (var editForm = new EditMessageForm(originalText))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    string newText = editForm.EditedMessageText;
                    if (_messageService.EditMessage(selectedMessageDisplay.OriginalMessage.Id, newText, _loggedInUsername, _authService))
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

            var result = MessageBox.Show("Are you sure you want to delete this message?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                if (_messageService.DeleteMessage(selectedMessageDisplay.OriginalMessage.Id, _loggedInUsername))
                {
                    LoadConversation();
                }
                else
                {
                    MessageBox.Show("Failed to delete message.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
