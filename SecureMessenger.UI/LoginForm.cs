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

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) { /* ... */ return; }

            // The Login method now just returns true or false
            if (_authService.Login(username, password))
            {
                LoggedInUsername = username;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
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