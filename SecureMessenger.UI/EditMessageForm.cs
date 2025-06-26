using System;
using System.Windows.Forms;

namespace SecureMessenger.UI
{
    public partial class EditMessageForm : Form
    {
        // This property will hold the new text from the user
        public string EditedMessageText { get; private set; }

        // The constructor will accept the original message text
        public EditMessageForm(string originalMessageText)
        {
            InitializeComponent();
            // Pre-fill the textbox with the old message
            txtEditMessage.Text = originalMessageText;
            EditedMessageText = originalMessageText; // Default to original text
        }

        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            // Store the new text and signal success
            this.EditedMessageText = txtEditMessage.Text.Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Signal cancellation
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}