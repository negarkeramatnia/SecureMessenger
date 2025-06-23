using SecureMessenger.Core.Services;
using System;
using System.Windows.Forms;

namespace SecureMessenger.UI
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService;
        private readonly UserDataService _userDataService;

        public string LoggedInUsername { get; private set; }

        // MODIFIED: Constructor now accepts an AuthService
        public LoginForm(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            _userDataService = new UserDataService();
        }
        public byte[] LoggedInUserPrivateKey { get; private set; }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var userDataService = new UserDataService();
                var storedUser = userDataService.GetUserByUsername(username);

                if (storedUser == null)
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // --- THIS IS THE CORRECTED PART ---
                // Login now returns 'true' on success, not the private key.
                if (_authService.Login(password, storedUser))
                {
                    // Login successful!
                    MessageBox.Show($"Welcome, {username}!", "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoggedInUsername = username;

                    // We no longer get the key here, so delete the old LoggedInUserPrivateKey line.

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    // Login failed (wrong password)
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during login: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Open the registration form as a dialog
            using (var registrationForm = new RegistrationForm())
            {
                this.Hide(); // Hide the login form while registration is open
                registrationForm.ShowDialog();
                this.Show(); // Show the login form again when registration is done
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            // Exit the entire application
            Application.Exit();
        }
    }
}