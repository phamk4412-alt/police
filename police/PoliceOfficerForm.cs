using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp
{
    public class PoliceOfficerForm : Form
    {
        public PoliceOfficerForm()
        {
            Text = "Giao diện Cảnh sát";
            ClientSize = new Size(1180, 760);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var canvas = UiFactory.CreateCanvas(this, Color.FromArgb(10, 34, 18), Color.FromArgb(7, 16, 26));

            UiFactory.CreateLabel(canvas, "ĐỘI PHẢN ỨNG", 40, 28, 11, FontStyle.Bold, Color.FromArgb(167, 231, 170));
            UiFactory.CreateLabel(canvas, "Hiện trường gần nhất và trạng thái trực", 36, 58, 24, FontStyle.Bold, Color.White);
            UiFactory.CreateLabel(canvas, "Màn hình ưu tiên cho cán bộ đang làm nhiệm vụ ngoài thực địa.", 40, 100, 10.5f, FontStyle.Regular, Color.FromArgb(214, 224, 234));

            var nearestCaseCard = UiFactory.CreateCard(canvas, new Rectangle(38, 150, 530, 546), Color.FromArgb(11, 24, 33), Color.FromArgb(60, 76, 175, 80));
            UiFactory.CreateLabel(nearestCaseCard, "Vụ án gần nhất", 22, 18, 14, FontStyle.Bold, Color.White);
            UiFactory.CreateLabel(nearestCaseCard, "Mã vụ việc", 22, 64, 10, FontStyle.Bold, Color.FromArgb(185, 220, 190));
            UiFactory.CreateLabel(nearestCaseCard, "CASE-240419-17", 170, 64, 10, FontStyle.Regular, Color.White);
            UiFactory.CreateLabel(nearestCaseCard, "Khoảng cách", 22, 98, 10, FontStyle.Bold, Color.FromArgb(185, 220, 190));
            UiFactory.CreateLabel(nearestCaseCard, "1.2 km từ vị trí tổ tuần tra", 170, 98, 10, FontStyle.Regular, Color.White);
            UiFactory.CreateLabel(nearestCaseCard, "Địa điểm", 22, 132, 10, FontStyle.Bold, Color.FromArgb(185, 220, 190));
            UiFactory.CreateLabel(nearestCaseCard, "45 Nguyễn Huệ, Quận 1", 170, 132, 10, FontStyle.Regular, Color.White);
            UiFactory.CreateLabel(nearestCaseCard, "Mô tả", 22, 178, 10, FontStyle.Bold, Color.FromArgb(185, 220, 190));
            UiFactory.CreateLabel(
                nearestCaseCard,
                "Tin báo về hành vi giật tài sản, đối tượng di chuyển bằng xe máy màu đen, hướng về cầu Khánh Hội.",
                22,
                204,
                10.5f,
                FontStyle.Regular,
                Color.FromArgb(226, 232, 236));

            var evidenceList = UiFactory.CreateListView(nearestCaseCard, new Rectangle(22, 280, 486, 180),
                ("Hạng mục", 220), ("Chi tiết", 250));
            evidenceList.Items.Add(new ListViewItem(new[] { "Người báo tin", "Nguyễn Văn A - 09xx xxx xxx" }));
            evidenceList.Items.Add(new ListViewItem(new[] { "Tọa độ", "10.7731, 106.7042" }));
            evidenceList.Items.Add(new ListViewItem(new[] { "Mức độ", "Khẩn cấp" }));
            evidenceList.Items.Add(new ListViewItem(new[] { "Ưu tiên", "Đội tuần tra gần nhất" }));

            UiFactory.CreateButton(nearestCaseCard, "Nhận nhiệm vụ", new Rectangle(22, 484, 150, 40), Color.FromArgb(76, 175, 80), Color.White);

            var opsCard = UiFactory.CreateCard(canvas, new Rectangle(600, 150, 542, 546), Color.FromArgb(11, 24, 33), Color.FromArgb(60, 120, 200, 255));
            UiFactory.CreateLabel(opsCard, "Tác vụ nhanh", 22, 18, 14, FontStyle.Bold, Color.White);

            var callButton = UiFactory.CreateButton(opsCard, "Gọi điện trụ sở", new Rectangle(22, 60, 220, 46), Color.FromArgb(0, 123, 255), Color.White);
            callButton.Click += (s, e) =>
                MessageBox.Show("Đang gọi về trụ sở điều phối.", "Cảnh sát", MessageBoxButtons.OK, MessageBoxIcon.Information);

            var checkInButton = UiFactory.CreateButton(opsCard, "Chấm công vào ca", new Rectangle(264, 60, 220, 46), Color.FromArgb(255, 193, 7), Color.FromArgb(30, 30, 30));
            var attendanceStatus = UiFactory.CreateTextBox(opsCard, new Rectangle(22, 122, 462, 34), "Trạng thái ca trực: Chưa chấm công");
            checkInButton.Click += (s, e) => attendanceStatus.Text = $"Trạng thái ca trực: Đã chấm công lúc {DateTime.Now:HH:mm}";

            UiFactory.CreateLabel(opsCard, "Lộ trình xử lý", 22, 188, 10, FontStyle.Bold, Color.FromArgb(190, 218, 235));
            var routeList = UiFactory.CreateListView(opsCard, new Rectangle(22, 214, 486, 182),
                ("Điểm", 170), ("Khoảng cách", 110), ("Ghi chú", 190));
            routeList.Items.Add(new ListViewItem(new[] { "Tổ tuần tra hiện tại", "0 km", "Xuất phát" }));
            routeList.Items.Add(new ListViewItem(new[] { "Ngã tư Tôn Đức Thắng", "0.8 km", "Nguy cơ ùn tắc" }));
            routeList.Items.Add(new ListViewItem(new[] { "Hiện trường", "1.2 km", "Tiếp cận phía Đông" }));

            UiFactory.CreateLabel(opsCard, "Ghi chú nhanh", 22, 420, 10, FontStyle.Bold, Color.FromArgb(190, 218, 235));
            UiFactory.CreateTextBox(opsCard, new Rectangle(22, 446, 486, 78), "Nhập ghi chú tuần tra hoặc kết quả xác minh...", multiline: true);

            var backButton = UiFactory.CreateButton(canvas, "Quay lại", new Rectangle(38, 708, 120, 36), Color.FromArgb(34, 51, 84), Color.White);
            backButton.Click += (s, e) => Close();
            var logoutButton = UiFactory.CreateButton(canvas, "Đăng xuất", new Rectangle(174, 708, 120, 36), Color.FromArgb(220, 53, 69), Color.White);
            logoutButton.Click += (s, e) => SessionNavigator.Logout();
        }
    }
}
