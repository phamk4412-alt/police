using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace WinFormsApp
{
    public class KhanhForm : Form
    {
        private readonly ListBox accountList = new ListBox();
        private readonly TextBox txtFullName = new TextBox();
        private readonly TextBox txtUsername = new TextBox();
        private readonly TextBox txtPassword = new TextBox();
        private readonly ComboBox cboRole = new ComboBox();
        private readonly Label lblStatus = new Label();
        private readonly List<AccountRecord> accounts = new List<AccountRecord>();

        public KhanhForm()
        {
            Text = "Đăng ký tài khoản - Khánh";
            ClientSize = new Size(920, 560);
            BackColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var canvas = new Panel { Dock = DockStyle.Fill };
            canvas.Paint += Canvas_Paint;
            Controls.Add(canvas);

            var headerLabel = CreateLabel("TRUNG TÂM CHỈ HUY", 42, 38, 11, FontStyle.Bold, Color.FromArgb(255, 220, 120));
            var titleLabel = CreateLabel("Khánh", 40, 72, 28, FontStyle.Bold, Color.FromArgb(34, 34, 34));
            var subtitleLabel = CreateLabel("Tạo tài khoản truy cập nội bộ cho cán bộ mới.", 42, 118, 12, FontStyle.Regular, Color.FromArgb(110, 110, 110));

            var formPanel = new Panel
            {
                Location = new Point(40, 170),
                Size = new Size(390, 320),
                BackColor = Color.FromArgb(248, 248, 248)
            };
            formPanel.Paint += (s, e) => DrawPanelBorder(e.Graphics, formPanel.ClientRectangle, Color.FromArgb(220, 220, 220));

            formPanel.Controls.Add(CreateFieldLabel("Họ và tên", 22, 26));
            txtFullName.Location = new Point(22, 48);
            txtFullName.Size = new Size(340, 29);

            formPanel.Controls.Add(CreateFieldLabel("Tên đăng nhập", 22, 92));
            txtUsername.Location = new Point(22, 114);
            txtUsername.Size = new Size(340, 29);

            formPanel.Controls.Add(CreateFieldLabel("Mật khẩu", 22, 158));
            txtPassword.Location = new Point(22, 180);
            txtPassword.Size = new Size(340, 29);
            txtPassword.UseSystemPasswordChar = true;

            formPanel.Controls.Add(CreateFieldLabel("Vai trò", 22, 224));
            cboRole.Location = new Point(22, 246);
            cboRole.Size = new Size(340, 29);
            cboRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cboRole.Items.AddRange(new object[]
            {
                "Cán bộ hiện trường",
                "Điều phối viên",
                "Quản trị hệ thống"
            });
            cboRole.SelectedIndex = 0;

            var registerButton = new Button
            {
                Text = "Đăng ký tài khoản",
                Size = new Size(170, 40),
                Location = new Point(40, 505),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(245, 245, 245),
                ForeColor = Color.FromArgb(34, 34, 34),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            registerButton.FlatAppearance.BorderSize = 1;
            registerButton.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 210);
            registerButton.Click += RegisterButton_Click;

            var backButton = new Button
            {
                Text = "Quay lại",
                Size = new Size(120, 40),
                Location = new Point(230, 505),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(70, 70, 70),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            backButton.FlatAppearance.BorderSize = 1;
            backButton.FlatAppearance.BorderColor = Color.FromArgb(210, 210, 210);
            backButton.Click += (s, e) => Close();

            lblStatus.AutoSize = false;
            lblStatus.Location = new Point(40, 495);
            lblStatus.Size = new Size(390, 0);
            lblStatus.ForeColor = Color.FromArgb(90, 90, 90);
            lblStatus.Font = new Font("Segoe UI", 9, FontStyle.Regular);

            var listPanel = new Panel
            {
                Location = new Point(470, 60),
                Size = new Size(390, 430),
                BackColor = Color.FromArgb(250, 250, 250)
            };
            listPanel.Paint += (s, e) => DrawPanelBorder(e.Graphics, listPanel.ClientRectangle, Color.FromArgb(220, 220, 220));

            var listTitle = CreateLabel("Tài khoản đã đăng ký", 18, 18, 14, FontStyle.Bold, Color.FromArgb(34, 34, 34));
            var listHint = CreateLabel("Danh sách này được lưu trong phiên làm việc hiện tại.", 18, 48, 10, FontStyle.Regular, Color.FromArgb(120, 120, 120));

            accountList.Location = new Point(18, 82);
            accountList.Size = new Size(354, 320);
            accountList.BorderStyle = BorderStyle.None;
            accountList.BackColor = Color.White;
            accountList.ForeColor = Color.FromArgb(45, 45, 45);
            accountList.Font = new Font("Segoe UI", 10, FontStyle.Regular);

            listPanel.Controls.Add(listTitle);
            listPanel.Controls.Add(listHint);
            listPanel.Controls.Add(accountList);

            SeedAccounts();

            formPanel.Controls.Add(txtFullName);
            formPanel.Controls.Add(txtUsername);
            formPanel.Controls.Add(txtPassword);
            formPanel.Controls.Add(cboRole);

            canvas.Controls.Add(headerLabel);
            canvas.Controls.Add(titleLabel);
            canvas.Controls.Add(subtitleLabel);
            canvas.Controls.Add(formPanel);
            canvas.Controls.Add(listPanel);
            canvas.Controls.Add(registerButton);
            canvas.Controls.Add(backButton);
            canvas.Controls.Add(lblStatus);
        }

        private void Canvas_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = ClientRectangle;

            using (var bg = new LinearGradientBrush(rect, Color.FromArgb(255, 255, 255), Color.FromArgb(242, 242, 242), LinearGradientMode.Vertical))
            {
                g.FillRectangle(bg, rect);
            }

            using (var accent = new Pen(Color.FromArgb(225, 225, 225), 2))
            {
                g.DrawRectangle(accent, 455, 38, 405, 470);
                g.DrawRectangle(accent, 35, 165, 400, 330);
            }
        }

        private void RegisterButton_Click(object? sender, EventArgs e)
        {
            var fullName = txtFullName.Text.Trim();
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Text;
            var role = cboRole.SelectedItem?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowStatus("Cần nhập đầy đủ họ tên, tên đăng nhập và mật khẩu.", false);
                return;
            }

            if (password.Length < 6)
            {
                ShowStatus("Mật khẩu phải có ít nhất 6 ký tự.", false);
                return;
            }

            if (accounts.Any(account => account.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                ShowStatus("Tên đăng nhập đã tồn tại. Hãy chọn tên khác.", false);
                return;
            }

            var account = new AccountRecord(fullName, username, role);
            accounts.Add(account);
            accountList.Items.Add(FormatAccount(account));

            txtFullName.Clear();
            txtUsername.Clear();
            txtPassword.Clear();
            cboRole.SelectedIndex = 0;

            ShowStatus($"Đăng ký thành công tài khoản {username}.", true);
        }

        private void SeedAccounts()
        {
            accounts.Add(new AccountRecord("Nguyễn Văn Minh", "minh.command", "Điều phối viên"));
            accounts.Add(new AccountRecord("Lê Thu Hà", "ha.ops", "Cán bộ hiện trường"));

            foreach (var account in accounts)
            {
                accountList.Items.Add(FormatAccount(account));
            }
        }

        private void ShowStatus(string message, bool success)
        {
            lblStatus.Text = message;
            lblStatus.Size = new Size(390, 24);
            lblStatus.ForeColor = success
                ? Color.FromArgb(46, 125, 50)
                : Color.FromArgb(198, 40, 40);
        }

        private static string FormatAccount(AccountRecord account)
        {
            return $"{account.Username} | {account.FullName} | {account.Role}";
        }

        private static void DrawPanelBorder(Graphics graphics, Rectangle rect, Color color)
        {
            rect.Width -= 1;
            rect.Height -= 1;

            using (var pen = new Pen(color, 1.5f))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.DrawRectangle(pen, rect);
            }
        }

        private static Label CreateLabel(string text, int x, int y, float fontSize, FontStyle style, Color color)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = color,
                Font = new Font("Segoe UI", fontSize, style)
            };
        }

        private static Label CreateFieldLabel(string text, int x, int y)
        {
            return CreateLabel(text, x, y, 10, FontStyle.Bold, Color.FromArgb(90, 90, 90));
        }

        private sealed class AccountRecord
        {
            public AccountRecord(string fullName, string username, string role)
            {
                FullName = fullName;
                Username = username;
                Role = role;
            }

            public string FullName { get; }

            public string Username { get; }

            public string Role { get; }
        }
    }
}
