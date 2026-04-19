using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp
{
    public class LoginForm : Form
    {
        private readonly Dictionary<string, (string Password, RoleType Role, string DisplayName)> accounts =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["admin"] = ("admin123", RoleType.Admin, "Quản trị viên"),
                ["user"] = ("user123", RoleType.User, "Người dùng"),
                ["police"] = ("police123", RoleType.Police, "Cảnh sát"),
                ["support"] = ("support123", RoleType.Support, "Nhân viên hỗ trợ")
            };

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Đăng nhập hệ thống cảnh sát";
            ClientSize = new Size(980, 620);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var canvas = UiFactory.CreateCanvas(this, Color.FromArgb(5, 18, 36), Color.FromArgb(18, 48, 92));

            UiFactory.CreateLabel(canvas, "SECURE ACCESS", 60, 44, 11, FontStyle.Bold, Color.FromArgb(139, 204, 255));
            UiFactory.CreateLabel(canvas, "Đăng nhập và phân quyền truy cập", 56, 76, 28, FontStyle.Bold, Color.White);
            UiFactory.CreateLabel(
                canvas,
                "Mỗi tài khoản chỉ truy cập đúng giao diện nghiệp vụ được cấp quyền.",
                60,
                126,
                11,
                FontStyle.Regular,
                Color.FromArgb(214, 224, 236));

            var loginCard = UiFactory.CreateCard(canvas, new Rectangle(60, 190, 380, 310), Color.FromArgb(8, 22, 42), Color.FromArgb(80, 100, 195, 255));
            UiFactory.CreateLabel(loginCard, "Tên đăng nhập", 28, 30, 10, FontStyle.Bold, Color.FromArgb(200, 216, 232));
            var usernameBox = UiFactory.CreateTextBox(loginCard, new Rectangle(28, 54, 320, 34), "admin");
            UiFactory.CreateLabel(loginCard, "Mật khẩu", 28, 108, 10, FontStyle.Bold, Color.FromArgb(200, 216, 232));
            var passwordBox = UiFactory.CreateTextBox(loginCard, new Rectangle(28, 132, 320, 34), "admin123");
            passwordBox.UseSystemPasswordChar = true;

            var messageLabel = UiFactory.CreateLabel(loginCard, "", 28, 178, 9.5f, FontStyle.Regular, Color.FromArgb(255, 189, 189));
            messageLabel.AutoSize = false;
            messageLabel.Size = new Size(320, 34);

            var loginButton = UiFactory.CreateButton(loginCard, "Đăng nhập", new Rectangle(28, 226, 152, 42), Color.FromArgb(0, 172, 193), Color.White);
            var exitButton = UiFactory.CreateButton(loginCard, "Thoát", new Rectangle(196, 226, 152, 42), Color.FromArgb(34, 51, 84), Color.White);

            loginButton.Click += (s, e) => TryLogin(usernameBox.Text, passwordBox.Text, messageLabel);
            exitButton.Click += (s, e) => Close();
            AcceptButton = loginButton;

            var infoCard = UiFactory.CreateCard(canvas, new Rectangle(486, 190, 430, 310), Color.FromArgb(8, 22, 42), Color.FromArgb(80, 255, 193, 7));
            UiFactory.CreateLabel(infoCard, "Tài khoản mẫu", 24, 24, 14, FontStyle.Bold, Color.White);
            UiFactory.CreateLabel(infoCard, "Admin", 24, 74, 11, FontStyle.Bold, Color.FromArgb(255, 193, 7));
            UiFactory.CreateLabel(infoCard, "admin / admin123", 154, 74, 11, FontStyle.Regular, Color.White);
            UiFactory.CreateLabel(infoCard, "Người dùng", 24, 114, 11, FontStyle.Bold, Color.FromArgb(255, 111, 97));
            UiFactory.CreateLabel(infoCard, "user / user123", 154, 114, 11, FontStyle.Regular, Color.White);
            UiFactory.CreateLabel(infoCard, "Cảnh sát", 24, 154, 11, FontStyle.Bold, Color.FromArgb(76, 175, 80));
            UiFactory.CreateLabel(infoCard, "police / police123", 154, 154, 11, FontStyle.Regular, Color.White);
            UiFactory.CreateLabel(infoCard, "Nhân viên hỗ trợ", 24, 194, 11, FontStyle.Bold, Color.FromArgb(0, 172, 193));
            UiFactory.CreateLabel(infoCard, "support / support123", 154, 194, 11, FontStyle.Regular, Color.White);
            UiFactory.CreateLabel(
                infoCard,
                "Sau khi đăng nhập, hệ thống sẽ mở trực tiếp giao diện đúng quyền và không hiển thị các màn hình còn lại.",
                24,
                246,
                10,
                FontStyle.Regular,
                Color.FromArgb(220, 228, 236));
        }

        private void TryLogin(string username, string password, Label messageLabel)
        {
            if (!accounts.TryGetValue(username.Trim(), out var account) || account.Password != password)
            {
                messageLabel.Text = "Sai tên đăng nhập hoặc mật khẩu.";
                return;
            }

            var targetForm = CreateRoleForm(account.Role);
            targetForm.Text = $"{targetForm.Text} - {account.DisplayName}";
            targetForm.FormClosed += (s, e) => Close();

            Hide();
            targetForm.Show();
        }

        private Form CreateRoleForm(RoleType role)
        {
            return role switch
            {
                RoleType.Admin => new AdminForm(),
                RoleType.User => new UserForm(),
                RoleType.Police => new PoliceOfficerForm(),
                RoleType.Support => new SupportStaffForm(),
                _ => new MainForm()
            };
        }
    }
}
