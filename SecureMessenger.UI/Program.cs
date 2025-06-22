using System;
using System.Windows.Forms;

namespace SecureMessenger.UI
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Application loop to allow for logout/login
            while (true)
            {
                // Show the login form as a dialog
                using (var loginForm = new LoginForm())
                {
                    // If the user logs in successfully (DialogResult.OK), then proceed.
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        // Show the main form with the logged-in user's info
                        using (var mainForm = new MainForm(loginForm.LoggedInUsername))
                        {
                            mainForm.ShowDialog();

                            // If the main form was closed with a "Retry" result, it means the user logged out.
                            // The loop will continue, showing the login form again.
                            if (mainForm.DialogResult == DialogResult.Retry)
                            {
                                continue;
                            }
                        }
                    }

                    // If the login form is closed in any other way (e.g., user clicks Exit or the 'X' button),
                    // break the loop and end the application.
                    break;
                }
            }
        }
    }
}