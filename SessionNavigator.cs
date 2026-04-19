using System.Windows.Forms;

namespace WinFormsApp
{
    internal static class SessionNavigator
    {
        public static void Logout(Form currentForm)
        {
            var loginForm = new LoginForm();
            loginForm.Show();
            currentForm.Close();
        }
    }
}
