using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp
{
    public class SupportStaffForm : Form
    {
        public SupportStaffForm()
        {
            Text = "Giao diện Nhân viên hỗ trợ";
            ClientSize = new Size(1180, 760);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var canvas = UiFactory.CreateCanvas(this, Color.FromArgb(44, 34, 10), Color.FromArgb(14, 16, 28));

            UiFactory.CreateLabel(canvas, "HỖ TRỢ TỔNG ĐÀI", 40, 28, 11, FontStyle.Bold, Color.FromArgb(255, 222, 124));
            UiFactory.CreateLabel(canvas, "Quan sát, theo dõi và tiếp nhận điện thoại", 36, 58, 24, FontStyle.Bold, Color.White);
            UiFactory.CreateLabel(canvas, "Màn hình trung gian để giám sát toàn bộ cuộc gọi và luồng xử lý sự cố.", 40, 100, 10.5f, FontStyle.Regular, Color.FromArgb(214, 224, 234));

            var monitorCard = UiFactory.CreateCard(canvas, new Rectangle(38, 150, 624, 542), Color.FromArgb(18, 20, 34), Color.FromArgb(60, 255, 193, 7));
            UiFactory.CreateLabel(monitorCard, "Bảng theo dõi", 22, 18, 14, FontStyle.Bold, Color.White);
            var monitorList = UiFactory.CreateListView(monitorCard, new Rectangle(22, 58, 580, 458),
                ("Kênh", 110), ("Sự kiện", 220), ("Khu vực", 120), ("Trạng thái", 120));
            monitorList.Items.Add(new ListViewItem(new[] { "Camera 02", "Tập trung đông người", "Quận 10", "Đang theo dõi" }));
            monitorList.Items.Add(new ListViewItem(new[] { "App công dân", "Báo mất cắp", "Quận 1", "Đã chuyển đội" }));
            monitorList.Items.Add(new ListViewItem(new[] { "Camera 11", "Va chạm giao thông", "Bình Thạnh", "Chờ xác minh" }));
            monitorList.Items.Add(new ListViewItem(new[] { "Hotline", "Yêu cầu hỗ trợ", "Quận 3", "Đang gọi lại" }));

            var callCard = UiFactory.CreateCard(canvas, new Rectangle(694, 150, 448, 542), Color.FromArgb(18, 20, 34), Color.FromArgb(60, 255, 111, 97));
            UiFactory.CreateLabel(callCard, "Tiếp nhận điện thoại", 22, 18, 14, FontStyle.Bold, Color.White);
            UiFactory.CreateLabel(callCard, "Số gọi đến", 22, 58, 10, FontStyle.Bold, Color.FromArgb(220, 220, 190));
            UiFactory.CreateTextBox(callCard, new Rectangle(22, 80, 180, 34), "0909 123 456");
            UiFactory.CreateLabel(callCard, "Người tiếp nhận", 222, 58, 10, FontStyle.Bold, Color.FromArgb(220, 220, 190));
            UiFactory.CreateTextBox(callCard, new Rectangle(222, 80, 190, 34), "NV Hỗ trợ 01");

            UiFactory.CreateLabel(callCard, "Nội dung cuộc gọi", 22, 130, 10, FontStyle.Bold, Color.FromArgb(220, 220, 190));
            var callNoteBox = UiFactory.CreateTextBox(callCard, new Rectangle(22, 152, 390, 146),
                "Mô tả tóm tắt nội dung, mức độ khẩn, thông tin nhận dạng và yêu cầu hỗ trợ.",
                multiline: true);

            var answerButton = UiFactory.CreateButton(callCard, "Tiếp nhận", new Rectangle(22, 320, 120, 40), Color.FromArgb(255, 193, 7), Color.FromArgb(32, 32, 32));
            var transferButton = UiFactory.CreateButton(callCard, "Chuyển cảnh sát", new Rectangle(156, 320, 130, 40), Color.FromArgb(255, 111, 97), Color.White);
            var closeCallButton = UiFactory.CreateButton(callCard, "Kết thúc", new Rectangle(300, 320, 112, 40), Color.FromArgb(34, 51, 84), Color.White);

            var statusBox = UiFactory.CreateTextBox(callCard, new Rectangle(22, 382, 390, 34), "Trạng thái: Chờ xử lý");
            answerButton.Click += (s, e) => statusBox.Text = "Trạng thái: Đã tiếp nhận cuộc gọi";
            closeCallButton.Click += (s, e) => statusBox.Text = "Trạng thái: Cuộc gọi đã kết thúc";

            UiFactory.CreateLabel(callCard, "Nhật ký cuộc gọi", 22, 438, 10, FontStyle.Bold, Color.FromArgb(220, 220, 190));
            var callHistory = UiFactory.CreateListView(callCard, new Rectangle(22, 462, 390, 60),
                ("Thời điểm", 110), ("Nội dung", 180), ("Kết quả", 100));
            callHistory.Items.Add(new ListViewItem(new[] { "09:42", "Tiếp nhận hotline", "Mở hồ sơ" }));

            transferButton.Click += (s, e) =>
            {
                statusBox.Text = "Trạng thái: Đã chuyển cho đội phản ứng";
                var shortNote = callNoteBox.Text.Length > 18 ? callNoteBox.Text[..18] + "..." : callNoteBox.Text;
                callHistory.Items.Add(new ListViewItem(new[] { DateTime.Now.ToString("HH:mm"), shortNote, "Đã chuyển" }));
            };

            var backButton = UiFactory.CreateButton(canvas, "Quay lại", new Rectangle(38, 708, 120, 36), Color.FromArgb(34, 51, 84), Color.White);
            backButton.Click += (s, e) => Close();
            var logoutButton = UiFactory.CreateButton(canvas, "Đăng xuất", new Rectangle(174, 708, 120, 36), Color.FromArgb(220, 53, 69), Color.White);
            logoutButton.Click += (s, e) => SessionNavigator.Logout(this);
        }
    }
}
