using SecureMessenger.Core.Services;
using System;
using System.Windows.Forms;

namespace SecureMessenger.UI
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService;
        private readonly UserDataService _userDataService;

        //public variables that other parts of the application can read. after login, this property will store the username
        public string LoggedInUsername { get; private set; }

        public LoginForm(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
            _userDataService = new UserDataService();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                //use our UserDataService to find the user's data in the database
                var userDataService = new UserDataService();
                var storedUser = userDataService.GetUserByUsername(username);

                if (storedUser == null)
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Login now returns 'true' on success, not the private key.
                if (_authService.Login(password, storedUser))
                {
                    // Login successful!
                    MessageBox.Show($"Welcome, {username}!", "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //store the username in our public property so Program.cs can access it
                    LoggedInUsername = username;

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
                //hide the login form while registration is open
                this.Hide();
                registrationForm.ShowDialog();
                //show the login form again when registration is done
                this.Show(); 
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}