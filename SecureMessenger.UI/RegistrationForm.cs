using SecureMessenger.Core.Services;
using System;
using System.Windows.Forms;

namespace SecureMessenger.UI
{
    public partial class RegistrationForm : Form
    {
        private readonly AuthService _authService;

        public RegistrationForm()
        {
            InitializeComponent();
            // In a real app with Dependency Injection, you'd get this from the container.
            // For now, we create it directly.
            _authService = new AuthService();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) { /* ... */ }
            if (password != confirmPassword) { /* ... */ }

            try
            {
                if (_authService.RegisterUser(username, password))
                {
                    MessageBox.Show($"User '{username}' registered successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("This username is already taken.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}