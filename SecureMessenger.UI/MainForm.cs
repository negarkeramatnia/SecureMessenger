using SecureMessenger.Core.Models;
using SecureMessenger.Core.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Threading.Channels;
using System.Windows.Forms;

namespace SecureMessenger.UI
{
    public partial class MainForm : Form
    {
        //manages the user's login session and private key
        private readonly AuthService _authService;
        //handles sending, receiving, and decrypting messages
        private readonly MessageService _messageService;
        //handles getting user information from the database
        private readonly UserDataService _userDataService;

        private readonly string _loggedInUsername;
        private string _selectedChatUser;
        private System.Windows.Forms.Timer _refreshTimer;
        //s flag to prevent the app from freezing
        private bool _isLoadingConversation = false;
        //authService session from the LoginForm
        public MainForm(string loggedInUsername, AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            //stores in private field
            _loggedInUsername = loggedInUsername;
            _messageService = new MessageService();
            _userDataService = new UserDataService();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Text = $"Secure Messenger - {_loggedInUsername}";
            lblStatus.Text = $"Logged in as: {_loggedInUsername}";
            //fill the user list from the database
            PopulateUserList();
            SetupRefreshTimer();
        }

        private void PopulateUserList()
        {
            //saves the name of the user that was previously selected
            string previouslySelected = _selectedChatUser;
            lstUsers.Items.Clear();
            try
            {
                //gets a list of all usernames from the database
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

        //automatically check for new messages
        private void SetupRefreshTimer()
        {
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 5000;
            _refreshTimer.Tick += (s, e) => LoadConversation();
            _refreshTimer.Start();
        }

        private void LoadConversation()
        {
            //safety check If the method is already busy loading, it stops to prevent freezing the app
            if (_isLoadingConversation) return;
            //if the user hasn't clicked on anyone to chat with, stop here
            if (string.IsNullOrEmpty(_selectedChatUser)) return;

            try
            {
                _isLoadingConversation = true;
                var fullConversation = _messageService.GetConversation(_loggedInUsername, _selectedChatUser);
                //create a new, empty list to hold the formatted message strings
                var newItems = new List<ChatMessageDisplay>();
                foreach (var message in fullConversation)
                {
                    string decryptedText = _messageService.DecryptMessage(message, _loggedInUsername, _authService);
                    //"You:"
                    string prefix = message.SenderUsername == _loggedInUsername ? "You" : message.SenderUsername;
                    string displayText = $"[{message.Timestamp:G}] {prefix}: {decryptedText}";
                    //"(edited)"
                    if (message.IsEdited)
                    {
                        displayText += " (edited)";
                    }
                    //add a new ChatMessageDisplay object
                    newItems.Add(new ChatMessageDisplay(message, displayText));
                }

                //it checks if anything has actually changed before it redraws the screen
                var currentIds = lstChatHistory.Items.Cast<ChatMessageDisplay>().Where(i => i.OriginalMessage != null).Select(item => item.OriginalMessage.Id);
                //it gets the IDs of the new messages we just loaded
                var newIds = newItems.Select(item => item.OriginalMessage.Id);

                //it only updates the listbox if the new list of messages is different from the old one
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
            //always runs
            finally
            {
                _isLoadingConversation = false;
            }
        }

        //this method runs when the user clicks on a different name in the user list
        private void lstUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstUsers.SelectedItem != null)
            {
                //store the name of the user we want to chat with
                _selectedChatUser = lstUsers.SelectedItem.ToString();
                lstChatHistory.Items.Clear();
                lstChatHistory.Items.Add(new ChatMessageDisplay(null, "Loading conversation..."));
                LoadConversation();
            }
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            //check if a user has been selected to send a message to
            if (string.IsNullOrEmpty(_selectedChatUser))
            {
                MessageBox.Show("Please select a user.", "No Recipient", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string messageText = txtMessageInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(messageText)) return;

            //cal MessageService to securely send the message
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
            //set the form's result to 'Retry'. This tells Program.cs to show the login screen again
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
            //only care about right clicks
            if (e.Button == MouseButtons.Right)
            {
                //find out which message is under the mouse pointer
                int index = this.lstChatHistory.IndexFromPoint(e.Location);
                // If it's a valid message
                if (index != ListBox.NoMatches)
                {
                    //select that message in the list
                    this.lstChatHistory.SelectedIndex = index;
                }
            }
        }

        // This method runs just before the right-click
        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (lstChatHistory.SelectedItem == null)
            {
                e.Cancel = true; return;
            }
            //get the selected message object
            var selectedMessageDisplay = lstChatHistory.SelectedItem as ChatMessageDisplay;
            if (selectedMessageDisplay == null || selectedMessageDisplay.OriginalMessage == null)
            {
                e.Cancel = true; return;
            }

            //check if it was sent by the currently logged-in user
            bool isMyMessage = selectedMessageDisplay.OriginalMessage.SenderUsername == _loggedInUsername;

            contextMenuStrip1.Items[0].Enabled = isMyMessage;
            contextMenuStrip1.Items[1].Enabled = isMyMessage;
        }

        private void editMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedMessageDisplay = lstChatHistory.SelectedItem as ChatMessageDisplay;
            if (selectedMessageDisplay == null || selectedMessageDisplay.OriginalMessage == null) return;

            //Decrypt the original message text to show it in the edit window
            string originalText = _messageService.DecryptMessage(selectedMessageDisplay.OriginalMessage, _loggedInUsername, _authService);

            //create and show edit form
            using (var editForm = new EditMessageForm(originalText))
            {
                //click on "Save Changes"
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    string newText = editForm.EditedMessageText;
                    //securely edit the message in the database
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
