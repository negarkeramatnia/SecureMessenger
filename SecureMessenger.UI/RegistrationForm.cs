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
            _authService = new AuthService(new CryptoService());
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            // 1. Get input from text boxes
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            // 2. Validate the input
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Username and password cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. If validation passes, register the user
            try
            {
                // authService.RegisterUser now returns the full User object
                var newUser = _authService.RegisterUser(username, password);

                if (newUser == null)
                {
                    // This case should not be hit if the username is unique, but it's good practice.
                    MessageBox.Show("Registration failed for an unknown reason.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.DialogResult = DialogResult.None;
                    return;
                }

                // Now, we save the user to the database and check the result of THAT operation
                var userDataService = new UserDataService();
                bool success = userDataService.CreateUser(newUser);

                if (success)
                {
                    MessageBox.Show($"User '{username}' registered successfully! You can now log in.", "Registration Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("This username is already taken. Please choose another one.", "Username Taken", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred during registration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }
    }
}