using SecureMessenger.Core.Services;
using System;
using System.Windows.Forms;

namespace SecureMessenger.UI
{
    //it just holds the starting point of our app
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            //making it look better
            Application.SetCompatibleTextRenderingDefault(false);
            //ONE single instance of your AuthService
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
                                //will show the login form again
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