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
            UiFactory.CreateLabel(canvas, "AI hỗ trợ khách hàng và báo cáo vụ việc", 36, 58, 24, FontStyle.Bold, Color.White);
            UiFactory.CreateLabel(canvas, "Nhập mô tả tình huống để AI gợi ý hướng xử lý, mức ưu tiên và hỗ trợ điền báo cáo nhanh.", 40, 100, 10.5f, FontStyle.Regular, Color.FromArgb(220, 220, 230));

            var newsCard = UiFactory.CreateCard(canvas, new Rectangle(38, 150, 360, 540), Color.FromArgb(15, 22, 40), Color.FromArgb(60, 255, 169, 160));
            UiFactory.CreateLabel(newsCard, "Tin tức mới", 22, 18, 14, FontStyle.Bold, Color.White);
            var newsList = UiFactory.CreateListView(newsCard, new Rectangle(22, 56, 316, 452),
                ("Tiêu đề", 160), ("Khu vực", 80), ("Cập nhật", 72));
            newsList.Items.Add(new ListViewItem(new[] { "Cảnh báo trộm cắp", "Quận 5", "08:30" }));
            newsList.Items.Add(new ListViewItem(new[] { "Điều chỉnh giao thông", "Quận 1", "09:00" }));
            newsList.Items.Add(new ListViewItem(new[] { "Khuyến nghị dịp lễ", "Toàn TP", "09:20" }));
            newsList.Items.Add(new ListViewItem(new[] { "Chốt lưu động", "Thủ Đức", "10:00" }));
            newsList.Items.Add(new ListViewItem(new[] { "Cảnh báo lừa đảo", "Online", "10:20" }));

            var reportCard = UiFactory.CreateCard(canvas, new Rectangle(420, 150, 360, 540), Color.FromArgb(15, 22, 40), Color.FromArgb(60, 255, 111, 97));
            UiFactory.CreateLabel(reportCard, "Báo cáo vụ việc", 22, 18, 14, FontStyle.Bold, Color.White);
            UiFactory.CreateLabel(reportCard, "Loại vụ việc", 22, 58, 9.5f, FontStyle.Bold, Color.FromArgb(205, 220, 235));
            var typeBox = UiFactory.CreateTextBox(reportCard, new Rectangle(22, 80, 316, 34), "Mất cắp tài sản");
            UiFactory.CreateLabel(reportCard, "Tọa độ hiện tại", 22, 126, 9.5f, FontStyle.Bold, Color.FromArgb(205, 220, 235));
            var locationBox = UiFactory.CreateTextBox(reportCard, new Rectangle(22, 148, 316, 34), "10.7769, 106.7009");
            UiFactory.CreateLabel(reportCard, "Mô tả", 22, 194, 9.5f, FontStyle.Bold, Color.FromArgb(205, 220, 235));
            var descriptionBox = UiFactory.CreateTextBox(reportCard, new Rectangle(22, 216, 316, 114),
                "Tôi vừa phát hiện xe máy bị lấy mất trước cửa hàng, khu vực có camera gần ngã tư.",
                multiline: true);

            var locateButton = UiFactory.CreateButton(reportCard, "Lấy tọa độ", new Rectangle(22, 346, 96, 38), Color.FromArgb(34, 51, 84), Color.White);
            locateButton.Click += (s, e) => locationBox.Text = "10.7756, 106.7019";

            var callButton = UiFactory.CreateButton(reportCard, "Gọi 113", new Rectangle(132, 346, 92, 38), Color.FromArgb(220, 53, 69), Color.White);
            callButton.Click += (s, e) =>
                MessageBox.Show("Đang kết nối tổng đài cảnh sát 113.", "Khẩn cấp", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            var reportButton = UiFactory.CreateButton(reportCard, "Gửi báo cáo", new Rectangle(238, 346, 100, 38), Color.FromArgb(255, 111, 97), Color.White);
            reportButton.Click += (s, e) =>
                MessageBox.Show($"Đã ghi nhận vụ việc '{typeBox.Text}' tại {locationBox.Text}.", "Người dùng", MessageBoxButtons.OK, MessageBoxIcon.Information);

            var timeline = UiFactory.CreateListView(reportCard, new Rectangle(22, 404, 316, 104),
                ("Bước xử lý", 170), ("Thời gian", 74), ("Trạng thái", 70));
            timeline.Items.Add(new ListViewItem(new[] { "Tiếp nhận báo cáo", "Ngay", "Sẵn sàng" }));
            timeline.Items.Add(new ListViewItem(new[] { "Điều phối lực lượng", "1-3p", "Tự động" }));

            var aiCard = UiFactory.CreateCard(canvas, new Rectangle(802, 150, 340, 540), Color.FromArgb(15, 22, 40), Color.FromArgb(60, 111, 168, 255));
            UiFactory.CreateLabel(aiCard, "AI hỗ trợ khách hàng", 22, 18, 14, FontStyle.Bold, Color.White);
            UiFactory.CreateLabel(aiCard, "Mô tả tình huống", 22, 58, 9.5f, FontStyle.Bold, Color.FromArgb(205, 220, 235));
            var aiPromptBox = UiFactory.CreateTextBox(aiCard, new Rectangle(22, 80, 296, 110),
                "Có hai người đi xe máy áp sát, giật túi rồi chạy về hướng ngã sáu.",
                multiline: true);

            UiFactory.CreateLabel(aiCard, "AI đánh giá", 22, 206, 9.5f, FontStyle.Bold, Color.FromArgb(205, 220, 235));
            var aiResultBox = UiFactory.CreateTextBox(aiCard, new Rectangle(22, 228, 296, 162), string.Empty, multiline: true);

            var aiAnalyzeButton = UiFactory.CreateButton(aiCard, "Phân tích AI", new Rectangle(22, 406, 116, 38), Color.FromArgb(59, 130, 246), Color.White);
            var aiApplyButton = UiFactory.CreateButton(aiCard, "Điền báo cáo", new Rectangle(152, 406, 116, 38), Color.FromArgb(16, 185, 129), Color.White);
            var aiEmergencyButton = UiFactory.CreateButton(aiCard, "Khẩn cấp", new Rectangle(22, 458, 116, 38), Color.FromArgb(239, 68, 68), Color.White);

            void ApplyAiSuggestion()
            {
                var analysis = CustomerAiAssistant.Analyze(aiPromptBox.Text);
                aiResultBox.Text =
                    $"Phân loại: {analysis.Category}{Environment.NewLine}" +
                    $"Mức ưu tiên: {analysis.Priority}{Environment.NewLine}{Environment.NewLine}" +
                    $"Gợi ý xử lý:{Environment.NewLine}{analysis.Guidance}{Environment.NewLine}{Environment.NewLine}" +
                    $"Khuyến nghị tiếp theo:{Environment.NewLine}{analysis.RecommendedAction}";

                typeBox.Text = analysis.Category;
                descriptionBox.Text = aiPromptBox.Text.Trim();

                if (analysis.ShouldCallEmergency)
                {
                    timeline.Items.Clear();
                    timeline.Items.Add(new ListViewItem(new[] { "Ưu tiên gọi 113", "Ngay", "Khẩn" }));
                    timeline.Items.Add(new ListViewItem(new[] { "Chia sẻ vị trí hiện tại", "Dưới 1p", "Cần làm" }));
                }
                else
                {
                    timeline.Items.Clear();
                    timeline.Items.Add(new ListViewItem(new[] { "Tiếp nhận báo cáo", "Ngay", "Sẵn sàng" }));
                    timeline.Items.Add(new ListViewItem(new[] { "Xác minh thông tin", "1-3p", "Tự động" }));
                }
            }

            aiAnalyzeButton.Click += (s, e) => ApplyAiSuggestion();
            aiApplyButton.Click += (s, e) => ApplyAiSuggestion();
            aiEmergencyButton.Click += (s, e) =>
            {
                var analysis = CustomerAiAssistant.Analyze(aiPromptBox.Text);
                ApplyAiSuggestion();
                if (analysis.ShouldCallEmergency)
                {
                    MessageBox.Show("AI đánh giá đây là tình huống khẩn cấp. Hãy gọi 113 ngay và giữ vị trí an toàn.", "AI hỗ trợ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("AI chưa thấy dấu hiệu khẩn cấp tức thời, nhưng bạn vẫn nên gửi báo cáo đầy đủ để được hỗ trợ.", "AI hỗ trợ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            ApplyAiSuggestion();

            var backButton = UiFactory.CreateButton(canvas, "Quay lại", new Rectangle(38, 708, 120, 36), Color.FromArgb(34, 51, 84), Color.White);
            backButton.Click += (s, e) => Close();
            var logoutButton = UiFactory.CreateButton(canvas, "Đăng xuất", new Rectangle(174, 708, 120, 36), Color.FromArgb(220, 53, 69), Color.White);
            logoutButton.Click += (s, e) => SessionNavigator.Logout();
        }
    }
}
