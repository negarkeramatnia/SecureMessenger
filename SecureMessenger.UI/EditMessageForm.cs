using System;
using System.Windows.Forms;

namespace SecureMessenger.UI
{
    public partial class EditMessageForm : Form
    {
        // This property will store the new edited text
        //other codes can read its value but can't change it 
        public string EditedMessageText { get; private set; }

        // The constructor will accept the original message text
        public EditMessageForm(string originalMessageText)
        {
            InitializeComponent();
            // Pre-fill the textbox with the old message
            txtEditMessage.Text = originalMessageText;
            //Default to original text if the user cancels
            EditedMessageText = originalMessageText;
        }

        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            // Store the new text and signal success
            this.EditedMessageText = txtEditMessage.Text.Trim();
            //signal to the MainForm that the user confirmed the changes
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