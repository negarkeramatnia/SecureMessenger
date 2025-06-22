using SecureMessenger.Core.Services;
using System;
using System.Windows.Forms;

namespace SecureMessenger.UI
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService;

        // This property will hold the logged-in username to pass to the main form
        public string LoggedInUsername { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            _authService = new AuthService(new CryptoService());
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
                byte[] decryptedPrivateKey = _authService.Login(username, password);
                LoggedInUserPrivateKey = decryptedPrivateKey;
                if (decryptedPrivateKey != null)
                {
                    // Login successful!
                    MessageBox.Show($"Welcome, {username}!", "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoggedInUsername = username;

                    // In a real app, you would securely store the decryptedPrivateKey for the session.
                    Array.Clear(decryptedPrivateKey, 0, decryptedPrivateKey.Length);

                    // --- THESE ARE THE CRITICAL LINES ---
                    this.DialogResult = DialogResult.OK; // Signal success to Program.cs
                    this.Close();                        // Close the login form
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