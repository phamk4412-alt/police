using System;
using System.Windows.Forms;

namespace WinFormsApp
{
    internal sealed class AppSessionContext : ApplicationContext
    {
        public static AppSessionContext Current { get; private set; } = null!;

        private Form? currentForm;

        public AppSessionContext()
        {
            Current = this;
            ShowLogin();
        }

        public void ShowLogin()
        {
            SwitchTo(new LoginForm());
        }

        public void OpenRoleForm(RoleType role, string displayName)
        {
            Form form = role switch
            {
                RoleType.Admin => new AdminForm(),
                RoleType.User => new UserForm(),
                RoleType.Police => new PoliceOfficerForm(),
                RoleType.Support => new SupportStaffForm(),
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Vai trò không hợp lệ.")
            };

            form.Text = $"{form.Text} - {displayName}";
            SwitchTo(form);
        }

        private void SwitchTo(Form nextForm)
        {
            if (currentForm is not null)
            {
                currentForm.FormClosed -= HandleCurrentFormClosed;
                currentForm.Hide();
                currentForm.Close();
            }

            currentForm = nextForm;
            currentForm.StartPosition = FormStartPosition.CenterScreen;
            currentForm.FormClosed += HandleCurrentFormClosed;
            currentForm.Show();
        }

        private void HandleCurrentFormClosed(object? sender, FormClosedEventArgs e)
        {
            if (ReferenceEquals(sender, currentForm))
            {
                ExitThread();
            }
        }
    }
}
