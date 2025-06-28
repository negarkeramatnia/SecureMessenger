using SecureMessenger.Core.Services;
using System;
using System.Windows.Forms;

namespace SecureMessenger.UI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var authService = new AuthService(new CryptoService());

            while (true)
            {
                using (var loginForm = new LoginForm(authService))
                {
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
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