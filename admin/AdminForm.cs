using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp
{
    public class AdminForm : Form
    {
        public AdminForm()
        {
            Text = "Giao diện Admin";
            ClientSize = new Size(1200, 760);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var canvas = UiFactory.CreateCanvas(this, Color.FromArgb(255, 255, 255), Color.FromArgb(243, 247, 252));

            UiFactory.CreateLabel(canvas, "ADMIN DASHBOARD", 38, 28, 11, FontStyle.Bold, Color.FromArgb(8, 145, 178));
            UiFactory.CreateLabel(canvas, "Quản trị báo cáo và cập nhật tin tức", 34, 58, 24, FontStyle.Bold, Color.FromArgb(15, 23, 42));
            UiFactory.CreateLabel(canvas, "Tổng quan dữ liệu ngày 19/04/2026", 38, 100, 10, FontStyle.Regular, Color.FromArgb(100, 116, 139));

            CreateStatCard(canvas, "Báo cáo mới", "128", "14 báo cáo đang chờ duyệt", new Rectangle(38, 150, 250, 120), Color.FromArgb(0, 172, 193));
            CreateStatCard(canvas, "Tin tức đã đăng", "36", "3 bản tin cần cập nhật", new Rectangle(310, 150, 250, 120), Color.FromArgb(52, 152, 219));
            CreateStatCard(canvas, "Cuộc gọi khẩn", "21", "2 cuộc gọi đang hoạt động", new Rectangle(582, 150, 250, 120), Color.FromArgb(255, 111, 97));
            CreateStatCard(canvas, "Đội phản ứng", "9", "7 đội đang trực", new Rectangle(854, 150, 250, 120), Color.FromArgb(46, 204, 113));

            var reportCard = UiFactory.CreateCard(canvas, new Rectangle(38, 300, 560, 390), Color.White, Color.FromArgb(210, 223, 236));
            UiFactory.CreateLabel(reportCard, "Báo cáo hiện trường", 22, 18, 14, FontStyle.Bold, Color.FromArgb(15, 23, 42));
            UiFactory.CreateLabel(reportCard, "Khu vực", 22, 58, 9.5f, FontStyle.Bold, Color.FromArgb(100, 116, 139));
            UiFactory.CreateLabel(reportCard, "Mức độ", 182, 58, 9.5f, FontStyle.Bold, Color.FromArgb(100, 116, 139));
            UiFactory.CreateLabel(reportCard, "Trạng thái", 302, 58, 9.5f, FontStyle.Bold, Color.FromArgb(100, 116, 139));
            UiFactory.CreateLabel(reportCard, "Thời gian", 432, 58, 9.5f, FontStyle.Bold, Color.FromArgb(100, 116, 139));

            var reports = UiFactory.CreateListView(reportCard, new Rectangle(22, 84, 516, 236),
                ("Khu vực", 160), ("Mức độ", 100), ("Trạng thái", 120), ("Thời gian", 120));
            reports.BackColor = Color.FromArgb(248, 250, 252);
            reports.ForeColor = Color.FromArgb(15, 23, 42);
            reports.Items.Add(new ListViewItem(new[] { "Quận 1", "Khẩn", "Đã điều phối", "08:42" }));
            reports.Items.Add(new ListViewItem(new[] { "Quận 3", "Trung bình", "Đang xác minh", "09:15" }));
            reports.Items.Add(new ListViewItem(new[] { "Thủ Đức", "Khẩn", "Chờ xử lý", "09:28" }));
            reports.Items.Add(new ListViewItem(new[] { "Bình Thạnh", "Thấp", "Đã đóng", "10:06" }));

            UiFactory.CreateButton(reportCard, "Xuất báo cáo", new Rectangle(22, 336, 140, 38), Color.FromArgb(0, 172, 193), Color.White);
            UiFactory.CreateButton(reportCard, "Làm mới dữ liệu", new Rectangle(176, 336, 150, 38), Color.FromArgb(226, 232, 240), Color.FromArgb(15, 23, 42));

            var newsCard = UiFactory.CreateCard(canvas, new Rectangle(628, 300, 534, 390), Color.White, Color.FromArgb(210, 223, 236));
            UiFactory.CreateLabel(newsCard, "Cập nhật tin tức", 22, 18, 14, FontStyle.Bold, Color.FromArgb(15, 23, 42));
            UiFactory.CreateLabel(newsCard, "Tiêu đề", 22, 58, 9.5f, FontStyle.Bold, Color.FromArgb(100, 116, 139));
            var titleBox = UiFactory.CreateTextBox(newsCard, new Rectangle(22, 80, 486, 34), "Cảnh báo giao thông khu vực trung tâm");
            UiFactory.CreateLabel(newsCard, "Nội dung", 22, 128, 9.5f, FontStyle.Bold, Color.FromArgb(100, 116, 139));
            var contentBox = UiFactory.CreateTextBox(newsCard, new Rectangle(22, 150, 486, 150),
                "Cập nhật tình hình ùn tắc và hướng dẫn người dân chọn lộ trình thay thế an toàn.",
                multiline: true);
            UiFactory.CreateLabel(newsCard, "Trạng thái đăng", 22, 316, 9.5f, FontStyle.Bold, Color.FromArgb(100, 116, 139));
            var statusBox = UiFactory.CreateTextBox(newsCard, new Rectangle(22, 338, 160, 34), "Chờ xuất bản");
            var publishButton = UiFactory.CreateButton(newsCard, "Đăng tin", new Rectangle(358, 334, 150, 40), Color.FromArgb(255, 111, 97), Color.White);
            publishButton.Click += (s, e) =>
            {
                statusBox.Text = $"Đã đăng lúc {DateTime.Now:HH:mm}";
                MessageBox.Show($"Đã cập nhật tin tức:\n{titleBox.Text}\n\n{contentBox.Text}", "Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            var backButton = UiFactory.CreateButton(canvas, "Quay lại", new Rectangle(38, 708, 120, 36), Color.FromArgb(226, 232, 240), Color.FromArgb(15, 23, 42));
            backButton.Click += (s, e) => Close();
            var logoutButton = UiFactory.CreateButton(canvas, "Đăng xuất", new Rectangle(174, 708, 120, 36), Color.FromArgb(220, 53, 69), Color.White);
            logoutButton.Click += (s, e) => SessionNavigator.Logout();
        }

        private void CreateStatCard(Control parent, string title, string value, string description, Rectangle bounds, Color accent)
        {
            var card = UiFactory.CreateCard(parent, bounds, Color.White, Color.FromArgb(220, accent));
            UiFactory.CreateLabel(card, title, 18, 18, 10, FontStyle.Bold, Color.FromArgb(100, 116, 139));
            UiFactory.CreateLabel(card, value, 18, 46, 24, FontStyle.Bold, accent);
            UiFactory.CreateLabel(card, description, 18, 84, 9.5f, FontStyle.Regular, Color.FromArgb(30, 41, 59));
        }
    }
}
