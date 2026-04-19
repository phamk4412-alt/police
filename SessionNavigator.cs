namespace WinFormsApp
{
    internal static class SessionNavigator
    {
        public static void Logout()
        {
            AppSessionContext.Current.ShowLogin();
        }
    }
}
