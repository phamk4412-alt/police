using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp
{
    public class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Hệ thống điều phối cảnh sát";
            ClientSize = new Size(1180, 760);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var canvas = UiFactory.CreateCanvas(this, Color.FromArgb(8, 18, 36), Color.FromArgb(18, 42, 76));

            UiFactory.CreateLabel(canvas, "POLICE SMART HUB", 56, 36, 11, FontStyle.Bold, Color.FromArgb(129, 194, 255));
            UiFactory.CreateLabel(canvas, "Chọn giao diện nghiệp vụ", 54, 72, 28, FontStyle.Bold, Color.White);
            UiFactory.CreateLabel(
                canvas,
                "4 dashboard tách theo vai trò để quản trị, tiếp nhận tin báo và phản ứng hiện trường nhanh.",
                58,
                124,
                11,
                FontStyle.Regular,
                Color.FromArgb(210, 220, 235));

            CreateRoleButton(canvas, "Admin", "Xem báo cáo và cập nhật tin tức", new Rectangle(56, 210, 250, 220), Color.FromArgb(0, 172, 193), () => OpenPage(new AdminForm()));
            CreateRoleButton(canvas, "Người dùng", "Xem tin tức, gọi cảnh sát, báo cáo vụ án", new Rectangle(332, 210, 250, 220), Color.FromArgb(255, 111, 97), () => OpenPage(new UserForm()));
            CreateRoleButton(canvas, "Cảnh sát", "Hiển thị vụ án gần nhất, gọi trụ sở, chấm công", new Rectangle(608, 210, 250, 220), Color.FromArgb(76, 175, 80), () => OpenPage(new PoliceOfficerForm()));
            CreateRoleButton(canvas, "Hỗ trợ", "Quan sát, theo dõi và tiếp nhận điện thoại", new Rectangle(884, 210, 250, 220), Color.FromArgb(255, 193, 7), () => OpenPage(new SupportStaffForm()));

            var infoCard = UiFactory.CreateCard(canvas, new Rectangle(56, 480, 1078, 190), Color.FromArgb(10, 24, 46), Color.FromArgb(60, 129, 194, 255));
            UiFactory.CreateLabel(infoCard, "Luồng xử lý đề xuất", 24, 22, 15, FontStyle.Bold, Color.White);
            UiFactory.CreateLabel(infoCard, "1. Người dân gửi tin báo có tọa độ hiện tại.", 24, 62, 11, FontStyle.Regular, Color.FromArgb(220, 228, 240));
            UiFactory.CreateLabel(infoCard, "2. Nhân viên hỗ trợ tiếp nhận cuộc gọi và theo dõi trạng thái.", 24, 92, 11, FontStyle.Regular, Color.FromArgb(220, 228, 240));
            UiFactory.CreateLabel(infoCard, "3. Cảnh sát nhận vụ gần nhất để điều phối xử lý thực địa.", 24, 122, 11, FontStyle.Regular, Color.FromArgb(220, 228, 240));
            UiFactory.CreateLabel(infoCard, "4. Admin tổng hợp báo cáo và đăng tin tức cảnh báo cho cộng đồng.", 24, 152, 11, FontStyle.Regular, Color.FromArgb(220, 228, 240));
        }

        private void CreateRoleButton(Control parent, string title, string subtitle, Rectangle bounds, Color accentColor, Action onClick)
        {
            var button = new Button
            {
                Bounds = bounds,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(10, 24, 46),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Text = $"{title}{Environment.NewLine}{Environment.NewLine}{subtitle}"
            };

            button.FlatAppearance.BorderColor = accentColor;
            button.FlatAppearance.BorderSize = 2;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(18, 36, 68);
            button.Click += (s, e) => onClick();
            button.Paint += (s, e) =>
            {
                using var brush = new SolidBrush(accentColor);
                e.Graphics.FillRectangle(brush, 0, 0, button.Width, 8);
            };

            parent.Controls.Add(button);
        }

        private void OpenPage(Form page)
        {
            page.Owner = this;
            page.StartPosition = FormStartPosition.CenterScreen;
            page.Show();
        }
    }
}
