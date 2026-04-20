using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WinFormsApp
{
    public class AdminForm : Form
    {
        private readonly string[] _areas = { "Quận 1", "Quận 3", "Thủ Đức", "Bình Thạnh", "Quận 7", "Gò Vấp", "Tân Bình" };
        private readonly string[] _categories = { "Mất cắp tài sản", "Va chạm giao thông", "Nghi ngờ lừa đảo", "Mất trật tự công cộng" };

        private ListView _reportList = null!;
        private Label _reportCountValue = null!;
        private Label _reportCountDescription = null!;
        private Label _newsCountValue = null!;
        private Label _newsCountDescription = null!;
        private Label _urgentCountValue = null!;
        private Label _urgentCountDescription = null!;
        private Label _teamCountValue = null!;
        private Label _teamCountDescription = null!;
        private Label _handledRateValue = null!;
        private Label _handledRateDescription = null!;
        private Label _urgentSummaryValue = null!;
        private Label _urgentSummaryDescription = null!;
        private Label _focusAreaValue = null!;
        private Label _focusAreaDescription = null!;
        private Label _severityBreakdownLabel = null!;
        private Label _statusBreakdownLabel = null!;
        private Label _updatedAtLabel = null!;
        private TextBox _statusBox = null!;
        private TextBox _titleBox = null!;
        private TextBox _contentBox = null!;

        public AdminForm()
        {
            Text = "Giao diện Admin";
            ClientSize = new Size(1200, 760);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var canvas = UiFactory.CreateCanvas(this, Color.FromArgb(248, 250, 252), Color.FromArgb(237, 242, 247));

            UiFactory.CreateLabel(canvas, "ADMIN DASHBOARD", 38, 28, 11, FontStyle.Bold, Color.FromArgb(30, 64, 175));
            UiFactory.CreateLabel(canvas, "Quản trị báo cáo, thống kê và cập nhật tin tức", 34, 58, 24, FontStyle.Bold, Color.FromArgb(15, 23, 42));
            UiFactory.CreateLabel(canvas, "Dữ liệu đang được lưu thật vào bộ nhớ cục bộ của ứng dụng.", 38, 100, 10, FontStyle.Regular, Color.FromArgb(71, 85, 105));

            var reportCard = CreateOverviewCard(canvas, "Báo cáo hôm nay", new Rectangle(38, 150, 250, 120), Color.FromArgb(30, 64, 175));
            _reportCountValue = reportCard.ValueLabel;
            _reportCountDescription = reportCard.DescriptionLabel;

            var newsCard = CreateOverviewCard(canvas, "Tin tức đã đăng", new Rectangle(310, 150, 250, 120), Color.FromArgb(37, 99, 235));
            _newsCountValue = newsCard.ValueLabel;
            _newsCountDescription = newsCard.DescriptionLabel;

            var urgentCard = CreateOverviewCard(canvas, "Ca khẩn cấp", new Rectangle(582, 150, 250, 120), Color.FromArgb(220, 38, 38));
            _urgentCountValue = urgentCard.ValueLabel;
            _urgentCountDescription = urgentCard.DescriptionLabel;

            var teamCard = CreateOverviewCard(canvas, "Đội phản ứng", new Rectangle(854, 150, 250, 120), Color.FromArgb(217, 119, 6));
            _teamCountValue = teamCard.ValueLabel;
            _teamCountDescription = teamCard.DescriptionLabel;

            var reportPanel = UiFactory.CreateCard(canvas, new Rectangle(38, 300, 520, 390), Color.White, Color.FromArgb(203, 213, 225));
            UiFactory.CreateLabel(reportPanel, "Báo cáo hiện trường", 22, 18, 14, FontStyle.Bold, Color.FromArgb(15, 23, 42));
            UiFactory.CreateLabel(reportPanel, "Khu vực", 22, 58, 9.5f, FontStyle.Bold, Color.FromArgb(71, 85, 105));
            UiFactory.CreateLabel(reportPanel, "Mức độ", 172, 58, 9.5f, FontStyle.Bold, Color.FromArgb(71, 85, 105));
            UiFactory.CreateLabel(reportPanel, "Trạng thái", 282, 58, 9.5f, FontStyle.Bold, Color.FromArgb(71, 85, 105));
            UiFactory.CreateLabel(reportPanel, "Thời gian", 402, 58, 9.5f, FontStyle.Bold, Color.FromArgb(71, 85, 105));

            _reportList = UiFactory.CreateListView(reportPanel, new Rectangle(22, 84, 476, 236),
                ("Khu vực", 150), ("Mức độ", 110), ("Trạng thái", 120), ("Thời gian", 96));
            _reportList.BackColor = Color.FromArgb(249, 250, 251);
            _reportList.ForeColor = Color.FromArgb(15, 23, 42);

            var exportButton = UiFactory.CreateButton(reportPanel, "Xuất báo cáo", new Rectangle(22, 336, 140, 38), Color.FromArgb(30, 64, 175), Color.White);
            exportButton.Click += (s, e) => ExportStatistics();

            var refreshButton = UiFactory.CreateButton(reportPanel, "Tạo dữ liệu mới", new Rectangle(176, 336, 150, 38), Color.FromArgb(226, 232, 240), Color.FromArgb(30, 41, 59));
            refreshButton.Click += (s, e) =>
            {
                AddIncomingReport();
                RenderDashboard();
            };

            _updatedAtLabel = UiFactory.CreateLabel(reportPanel, string.Empty, 352, 346, 9.5f, FontStyle.Regular, Color.FromArgb(100, 116, 139));

            var statisticsPanel = UiFactory.CreateCard(canvas, new Rectangle(582, 300, 250, 390), Color.White, Color.FromArgb(203, 213, 225));
            UiFactory.CreateLabel(statisticsPanel, "Thống kê nhanh", 22, 18, 14, FontStyle.Bold, Color.FromArgb(15, 23, 42));

            var handledRateCard = CreateMetricCard(statisticsPanel, "Tỷ lệ xử lý", new Rectangle(22, 58, 206, 84), Color.FromArgb(30, 64, 175));
            _handledRateValue = handledRateCard.ValueLabel;
            _handledRateDescription = handledRateCard.DescriptionLabel;

            var urgentSummaryCard = CreateMetricCard(statisticsPanel, "Mức khẩn", new Rectangle(22, 152, 206, 84), Color.FromArgb(220, 38, 38));
            _urgentSummaryValue = urgentSummaryCard.ValueLabel;
            _urgentSummaryDescription = urgentSummaryCard.DescriptionLabel;

            var focusAreaCard = CreateMetricCard(statisticsPanel, "Khu vực nổi bật", new Rectangle(22, 246, 206, 84), Color.FromArgb(217, 119, 6));
            _focusAreaValue = focusAreaCard.ValueLabel;
            _focusAreaDescription = focusAreaCard.DescriptionLabel;

            _severityBreakdownLabel = UiFactory.CreateLabel(statisticsPanel, string.Empty, 22, 344, 9.5f, FontStyle.Regular, Color.FromArgb(71, 85, 105));
            _severityBreakdownLabel.AutoSize = false;
            _severityBreakdownLabel.Size = new Size(206, 20);

            _statusBreakdownLabel = UiFactory.CreateLabel(statisticsPanel, string.Empty, 22, 364, 9.5f, FontStyle.Regular, Color.FromArgb(71, 85, 105));
            _statusBreakdownLabel.AutoSize = false;
            _statusBreakdownLabel.Size = new Size(206, 20);

            var publishPanel = UiFactory.CreateCard(canvas, new Rectangle(856, 300, 306, 390), Color.White, Color.FromArgb(203, 213, 225));
            UiFactory.CreateLabel(publishPanel, "Cập nhật tin tức", 22, 18, 14, FontStyle.Bold, Color.FromArgb(15, 23, 42));
            UiFactory.CreateLabel(publishPanel, "Tiêu đề", 22, 58, 9.5f, FontStyle.Bold, Color.FromArgb(71, 85, 105));
            _titleBox = UiFactory.CreateTextBox(publishPanel, new Rectangle(22, 80, 262, 34), "Cảnh báo giao thông khu vực trung tâm");
            UiFactory.CreateLabel(publishPanel, "Nội dung", 22, 128, 9.5f, FontStyle.Bold, Color.FromArgb(71, 85, 105));
            _contentBox = UiFactory.CreateTextBox(publishPanel, new Rectangle(22, 150, 262, 146),
                "Cập nhật tình hình hiện trường và hướng dẫn người dân chọn lộ trình thay thế an toàn.",
                multiline: true);
            UiFactory.CreateLabel(publishPanel, "Trạng thái đăng", 22, 316, 9.5f, FontStyle.Bold, Color.FromArgb(71, 85, 105));
            _statusBox = UiFactory.CreateTextBox(publishPanel, new Rectangle(22, 338, 118, 34), "Chờ xuất bản");
            var publishButton = UiFactory.CreateButton(publishPanel, "Đăng tin", new Rectangle(154, 334, 130, 40), Color.FromArgb(30, 64, 175), Color.White);
            publishButton.Click += (s, e) => PublishNews();

            var backButton = UiFactory.CreateButton(canvas, "Quay lại", new Rectangle(38, 708, 120, 36), Color.FromArgb(226, 232, 240), Color.FromArgb(30, 41, 59));
            backButton.Click += (s, e) => Close();
            var logoutButton = UiFactory.CreateButton(canvas, "Đăng xuất", new Rectangle(174, 708, 120, 36), Color.FromArgb(220, 53, 69), Color.White);
            logoutButton.Click += (s, e) => SessionNavigator.Logout();

            RenderDashboard();
        }

        private void RenderDashboard()
        {
            var reports = LocalDataStore.GetReports();
            var news = LocalDataStore.GetNews();

            _reportList.Items.Clear();
            foreach (var report in reports)
            {
                _reportList.Items.Add(new ListViewItem(new[]
                {
                    report.Area,
                    report.Severity,
                    report.Status,
                    report.ReportedAt.ToString("HH:mm")
                }));
            }

            var urgentCount = reports.Count(r => r.Severity == "Khẩn");
            var handledCount = reports.Count(r => r.Status is "Đã điều phối" or "Đã đóng");
            var processingCount = reports.Count(r => r.Status is "Đang xác minh" or "Chờ xử lý" or "Mới tiếp nhận");
            var activeTeams = Math.Min(9, Math.Max(5, handledCount + 3));
            var handledRate = reports.Count == 0 ? 0 : (int)Math.Round((double)handledCount / reports.Count * 100);
            var focusArea = reports.GroupBy(r => r.Area).OrderByDescending(group => group.Count()).ThenBy(group => group.Key).FirstOrDefault();

            _reportCountValue.Text = reports.Count.ToString();
            _reportCountDescription.Text = $"{processingCount} báo cáo đang cần theo dõi";

            _newsCountValue.Text = news.Count.ToString();
            _newsCountDescription.Text = $"{news.Take(3).Count()} bản tin mới nhất đang hiển thị";

            _urgentCountValue.Text = urgentCount.ToString();
            _urgentCountDescription.Text = $"{reports.Count(r => r.Status == "Đang xác minh")} ca đang xác minh";

            _teamCountValue.Text = activeTeams.ToString();
            _teamCountDescription.Text = $"{Math.Max(1, activeTeams - 2)} đội sẵn sàng điều phối";

            _handledRateValue.Text = $"{handledRate}%";
            _handledRateDescription.Text = $"{handledCount}/{reports.Count} báo cáo đã xử lý hoặc điều phối";

            _urgentSummaryValue.Text = $"{urgentCount} ca";
            _urgentSummaryDescription.Text = $"{Math.Max(0, urgentCount - 1)} ca khẩn đã có đội tiếp cận";

            _focusAreaValue.Text = focusArea?.Key ?? "Chưa có";
            _focusAreaDescription.Text = $"{focusArea?.Count() ?? 0} báo cáo tập trung tại khu vực này";

            _severityBreakdownLabel.Text =
                $"Mức độ: Khẩn {reports.Count(r => r.Severity == "Khẩn")} | Trung bình {reports.Count(r => r.Severity == "Trung bình")} | Thấp {reports.Count(r => r.Severity == "Thấp")}";
            _statusBreakdownLabel.Text =
                $"Trạng thái: Điều phối {reports.Count(r => r.Status == "Đã điều phối")} | Xác minh {reports.Count(r => r.Status == "Đang xác minh")} | Chờ {reports.Count(r => r.Status == "Chờ xử lý")} | Mới {reports.Count(r => r.Status == "Mới tiếp nhận")}";
            _updatedAtLabel.Text = $"Cập nhật lúc {DateTime.Now:HH:mm}";
        }

        private void PublishNews()
        {
            var newsItem = new NewsItem
            {
                Id = $"NEWS-{DateTime.Now:yyMMdd-HHmmss}",
                Title = _titleBox.Text.Trim(),
                Area = "Toàn thành phố",
                Content = _contentBox.Text.Trim(),
                PublishedAt = DateTime.Now
            };

            LocalDataStore.AddNews(newsItem);
            _statusBox.Text = $"Đã đăng {DateTime.Now:HH:mm}";
            RenderDashboard();
            MessageBox.Show($"Đã cập nhật tin tức:\n{newsItem.Title}\n\n{newsItem.Content}", "Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void AddIncomingReport()
        {
            var minute = DateTime.Now.Minute;
            var category = _categories[minute % _categories.Length];
            var severity = category == "Mất cắp tài sản" ? "Khẩn" : minute % 2 == 0 ? "Trung bình" : "Thấp";

            LocalDataStore.AddReport(new IncidentReport
            {
                Id = $"CASE-{DateTime.Now:yyMMdd-HHmmss}",
                Category = category,
                Severity = severity,
                Status = "Mới tiếp nhận",
                Area = _areas[minute % _areas.Length],
                Location = "10.7769, 106.7009",
                Description = $"Bản ghi mới được tạo tự động để mô phỏng dữ liệu thật cho admin lúc {DateTime.Now:HH:mm}.",
                Source = "Admin seed",
                ReportedAt = DateTime.Now
            });
        }

        private void ExportStatistics()
        {
            var reports = LocalDataStore.GetReports();
            var news = LocalDataStore.GetNews();
            var summary =
                $"Tổng báo cáo: {reports.Count}\n" +
                $"Khẩn cấp: {reports.Count(r => r.Severity == "Khẩn")}\n" +
                $"Đã điều phối hoặc hoàn tất: {reports.Count(r => r.Status is "Đã điều phối" or "Đã đóng")}\n" +
                $"Tin tức đã đăng: {news.Count}\n" +
                $"Khu vực nhiều báo cáo nhất: {reports.GroupBy(r => r.Area).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key ?? "Chưa có"}";

            MessageBox.Show(summary, "Thống kê admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static CardWidgets CreateOverviewCard(Control parent, string title, Rectangle bounds, Color accent)
        {
            var card = UiFactory.CreateCard(parent, bounds, Color.White, Color.FromArgb(203, 213, 225));
            UiFactory.CreateLabel(card, title, 18, 18, 10, FontStyle.Bold, Color.FromArgb(71, 85, 105));
            var valueLabel = UiFactory.CreateLabel(card, "0", 18, 46, 24, FontStyle.Bold, accent);
            var descriptionLabel = UiFactory.CreateLabel(card, string.Empty, 18, 84, 9.5f, FontStyle.Regular, Color.FromArgb(30, 41, 59));
            return new CardWidgets(valueLabel, descriptionLabel);
        }

        private static CardWidgets CreateMetricCard(Control parent, string title, Rectangle bounds, Color accent)
        {
            var card = UiFactory.CreateCard(parent, bounds, Color.FromArgb(249, 250, 251), Color.FromArgb(226, 232, 240));
            UiFactory.CreateLabel(card, title, 14, 12, 9.5f, FontStyle.Bold, Color.FromArgb(71, 85, 105));
            var valueLabel = UiFactory.CreateLabel(card, "--", 14, 34, 19, FontStyle.Bold, accent);
            var descriptionLabel = UiFactory.CreateLabel(card, string.Empty, 14, 60, 8.8f, FontStyle.Regular, Color.FromArgb(51, 65, 85));
            descriptionLabel.AutoSize = false;
            descriptionLabel.Size = new Size(bounds.Width - 28, 18);
            return new CardWidgets(valueLabel, descriptionLabel);
        }

        private sealed record CardWidgets(Label ValueLabel, Label DescriptionLabel);
    }
}
