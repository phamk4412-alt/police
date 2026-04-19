using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp
{
    public class UserForm : Form
    {
        public UserForm()
        {
            Text = "Giao diện Người dùng";
            ClientSize = new Size(1180, 760);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var canvas = UiFactory.CreateCanvas(this, Color.FromArgb(44, 20, 32), Color.FromArgb(12, 18, 30));

            UiFactory.CreateLabel(canvas, "CÔNG DÂN", 40, 28, 11, FontStyle.Bold, Color.FromArgb(255, 169, 160));
            UiFactory.CreateLabel(canvas, "Tin tức an ninh và báo cáo vụ án", 36, 58, 24, FontStyle.Bold, Color.White);
            UiFactory.CreateLabel(canvas, "Gửi thông tin khẩn dựa trên tọa độ hiện tại và liên hệ cảnh sát ngay.", 40, 100, 10.5f, FontStyle.Regular, Color.FromArgb(220, 220, 230));

            var newsCard = UiFactory.CreateCard(canvas, new Rectangle(38, 150, 520, 540), Color.FromArgb(15, 22, 40), Color.FromArgb(60, 255, 169, 160));
            UiFactory.CreateLabel(newsCard, "Tin tức mới", 22, 18, 14, FontStyle.Bold, Color.White);
            var newsList = UiFactory.CreateListView(newsCard, new Rectangle(22, 56, 476, 452),
                ("Tiêu đề", 250), ("Khu vực", 100), ("Cập nhật", 110));
            newsList.Items.Add(new ListViewItem(new[] { "Cảnh báo trộm cắp khu dân cư", "Quận 5", "08:30" }));
            newsList.Items.Add(new ListViewItem(new[] { "Điều chỉnh phân luồng giờ cao điểm", "Quận 1", "09:00" }));
            newsList.Items.Add(new ListViewItem(new[] { "Khuyến nghị an toàn dịp lễ", "Toàn thành phố", "09:20" }));
            newsList.Items.Add(new ListViewItem(new[] { "Thông báo chốt kiểm tra lưu động", "Thủ Đức", "10:00" }));

            var actionCard = UiFactory.CreateCard(canvas, new Rectangle(590, 150, 552, 540), Color.FromArgb(15, 22, 40), Color.FromArgb(60, 255, 111, 97));
            UiFactory.CreateLabel(actionCard, "Báo cáo vụ án", 22, 18, 14, FontStyle.Bold, Color.White);
            UiFactory.CreateLabel(actionCard, "Loại vụ việc", 22, 58, 9.5f, FontStyle.Bold, Color.FromArgb(205, 220, 235));
            var typeBox = UiFactory.CreateTextBox(actionCard, new Rectangle(22, 80, 240, 34), "Mất cắp tài sản");
            UiFactory.CreateLabel(actionCard, "Tọa độ hiện tại", 286, 58, 9.5f, FontStyle.Bold, Color.FromArgb(205, 220, 235));
            var locationBox = UiFactory.CreateTextBox(actionCard, new Rectangle(286, 80, 244, 34), "10.7769, 106.7009");
            UiFactory.CreateLabel(actionCard, "Mô tả", 22, 128, 9.5f, FontStyle.Bold, Color.FromArgb(205, 220, 235));
            UiFactory.CreateTextBox(actionCard, new Rectangle(22, 150, 508, 180),
                "Người dùng mô tả ngắn tình huống, phương tiện liên quan và dấu hiệu nhận diện.",
                multiline: true);

            var locateButton = UiFactory.CreateButton(actionCard, "Lấy tọa độ hiện tại", new Rectangle(22, 350, 170, 40), Color.FromArgb(34, 51, 84), Color.White);
            locateButton.Click += (s, e) => locationBox.Text = "10.7756, 106.7019";

            var callButton = UiFactory.CreateButton(actionCard, "Gọi cảnh sát", new Rectangle(208, 350, 150, 40), Color.FromArgb(220, 53, 69), Color.White);
            callButton.Click += (s, e) =>
                MessageBox.Show("Đang kết nối tổng đài cảnh sát 113.", "Khẩn cấp", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            var reportButton = UiFactory.CreateButton(actionCard, "Gửi báo cáo", new Rectangle(380, 350, 150, 40), Color.FromArgb(255, 111, 97), Color.White);
            reportButton.Click += (s, e) =>
                MessageBox.Show($"Đã ghi nhận vụ việc '{typeBox.Text}' tại {locationBox.Text}.", "Người dùng", MessageBoxButtons.OK, MessageBoxIcon.Information);

            var timeline = UiFactory.CreateListView(actionCard, new Rectangle(22, 414, 508, 94),
                ("Bước xử lý", 280), ("Thời gian", 120), ("Trạng thái", 100));
            timeline.Items.Add(new ListViewItem(new[] { "Tiếp nhận báo cáo", "Ngay lập tức", "Sẵn sàng" }));
            timeline.Items.Add(new ListViewItem(new[] { "Điều phối lực lượng gần nhất", "1-3 phút", "Tự động" }));

            var backButton = UiFactory.CreateButton(canvas, "Quay lại", new Rectangle(38, 708, 120, 36), Color.FromArgb(34, 51, 84), Color.White);
            backButton.Click += (s, e) => Close();
            var logoutButton = UiFactory.CreateButton(canvas, "Đăng xuất", new Rectangle(174, 708, 120, 36), Color.FromArgb(220, 53, 69), Color.White);
            logoutButton.Click += (s, e) => SessionNavigator.Logout();
        }
    }
}
