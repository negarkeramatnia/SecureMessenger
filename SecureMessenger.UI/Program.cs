using SecureMessenger.Core.Services;
using System;
using System.Windows.Forms;

namespace SecureMessenger.UI
{
    internal static class Program
    {
        // Replace the entire Main method in Program.cs
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Create ONE instance of AuthService for the entire application lifetime
            var authService = new AuthService(new CryptoService());

            while (true)
            {
                // Pass the single authService instance to the login form
                using (var loginForm = new LoginForm(authService))
                {
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        // Pass the same authService instance to the main form
                        using (var mainForm = new MainForm(loginForm.LoggedInUsername, authService))
                        {
                            mainForm.ShowDialog();

                            if (mainForm.DialogResult == DialogResult.Retry)
                            {
                                continue;
                            }
                        }
                    }
                    break;
                }
            }
        }
    }
}