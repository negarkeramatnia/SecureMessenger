using SecureMessenger.Core.Services;
using System;
using System.Windows.Forms;

namespace SecureMessenger.UI
{
    public partial class RegistrationForm : Form
    {
        private readonly AuthService _authService;
        //Constructor
        public RegistrationForm()
        {
            InitializeComponent();
            //new instance of AuthService that needs a CryptoService
            _authService = new AuthService(new CryptoService());
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            // Get the text from the username textbox
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

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

            try
            {
                //returns a complete User object
                var newUser = _authService.RegisterUser(username, password);
                //if the username is already taken
                if (newUser == null)
                {
                    MessageBox.Show("Registration failed for an unknown reason.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //set the form's result to None so it doesn't close automatically
                    this.DialogResult = DialogResult.None;
                    return;
                }
                //create a new instance of our database
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