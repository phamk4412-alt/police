using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WinFormsApp
{
    public class AdminForm : Form
    {
        private readonly List<AdminReport> _reports = new()
        {
            new("Quận 1", "Khẩn", "Đã điều phối", "08:42"),
            new("Quận 3", "Trung bình", "Đang xác minh", "09:15"),
            new("Thủ Đức", "Khẩn", "Chờ xử lý", "09:28"),
            new("Bình Thạnh", "Thấp", "Đã đóng", "10:06"),
            new("Quận 7", "Khẩn", "Đang xác minh", "10:22"),
            new("Gò Vấp", "Trung bình", "Đã điều phối", "10:41")
        };

        private int _publishedNewsCount = 36;
        private readonly string[] _areas = { "Quận 1", "Quận 3", "Thủ Đức", "Bình Thạnh", "Quận 7", "Gò Vấp", "Tân Bình" };
        private readonly string[] _severities = { "Khẩn", "Trung bình", "Thấp" };

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

            var canvas = UiFactory.CreateCanvas(this, Color.FromArgb(255, 255, 255), Color.FromArgb(243, 247, 252));

            UiFactory.CreateLabel(canvas, "ADMIN DASHBOARD", 38, 28, 11, FontStyle.Bold, Color.FromArgb(8, 145, 178));
            UiFactory.CreateLabel(canvas, "Quản trị báo cáo, thống kê và cập nhật tin tức", 34, 58, 24, FontStyle.Bold, Color.FromArgb(15, 23, 42));
            UiFactory.CreateLabel(canvas, "Theo dõi dữ liệu hiện trường theo thời gian thực ngày 20/04/2026", 38, 100, 10, FontStyle.Regular, Color.FromArgb(100, 116, 139));

            var reportCard = CreateOverviewCard(canvas, "Báo cáo hôm nay", new Rectangle(38, 150, 250, 120), Color.FromArgb(0, 172, 193));
            _reportCountValue = reportCard.ValueLabel;
            _reportCountDescription = reportCard.DescriptionLabel;

            var newsCard = CreateOverviewCard(canvas, "Tin tức đã đăng", new Rectangle(310, 150, 250, 120), Color.FromArgb(52, 152, 219));
            _newsCountValue = newsCard.ValueLabel;
            _newsCountDescription = newsCard.DescriptionLabel;

            var urgentCard = CreateOverviewCard(canvas, "Ca khẩn cấp", new Rectangle(582, 150, 250, 120), Color.FromArgb(255, 111, 97));
            _urgentCountValue = urgentCard.ValueLabel;
            _urgentCountDescription = urgentCard.DescriptionLabel;

            var teamCard = CreateOverviewCard(canvas, "Đội phản ứng", new Rectangle(854, 150, 250, 120), Color.FromArgb(46, 204, 113));
            _teamCountValue = teamCard.ValueLabel;
            _teamCountDescription = teamCard.DescriptionLabel;

            var reportPanel = UiFactory.CreateCard(canvas, new Rectangle(38, 300, 520, 390), Color.White, Color.FromArgb(210, 223, 236));
            UiFactory.CreateLabel(reportPanel, "Báo cáo hiện trường", 22, 18, 14, FontStyle.Bold, Color.FromArgb(15, 23, 42));
            UiFactory.CreateLabel(reportPanel, "Khu vực", 22, 58, 9.5f, FontStyle.Bold, Color.FromArgb(100, 116, 139));
            UiFactory.CreateLabel(reportPanel, "Mức độ", 172, 58, 9.5f, FontStyle.Bold, Color.FromArgb(100, 116, 139));
            UiFactory.CreateLabel(reportPanel, "Trạng thái", 282, 58, 9.5f, FontStyle.Bold, Color.FromArgb(100, 116, 139));
            UiFactory.CreateLabel(reportPanel, "Thời gian", 402, 58, 9.5f, FontStyle.Bold, Color.FromArgb(100, 116, 139));

            _reportList = UiFactory.CreateListView(reportPanel, new Rectangle(22, 84, 476, 236),
                ("Khu vực", 150), ("Mức độ", 110), ("Trạng thái", 120), ("Thời gian", 96));
            _reportList.BackColor = Color.FromArgb(248, 250, 252);
            _reportList.ForeColor = Color.FromArgb(15, 23, 42);

            var exportButton = UiFactory.CreateButton(reportPanel, "Xuất báo cáo", new Rectangle(22, 336, 140, 38), Color.FromArgb(0, 172, 193), Color.White);
            exportButton.Click += (s, e) => ExportStatistics();

            var refreshButton = UiFactory.CreateButton(reportPanel, "Làm mới dữ liệu", new Rectangle(176, 336, 150, 38), Color.FromArgb(226, 232, 240), Color.FromArgb(15, 23, 42));
            refreshButton.Click += (s, e) =>
            {
                AddIncomingReport();
                RenderDashboard();
            };

            _updatedAtLabel = UiFactory.CreateLabel(reportPanel, string.Empty, 352, 346, 9.5f, FontStyle.Regular, Color.FromArgb(100, 116, 139));

            var statisticsPanel = UiFactory.CreateCard(canvas, new Rectangle(582, 300, 250, 390), Color.White, Color.FromArgb(210, 223, 236));
            UiFactory.CreateLabel(statisticsPanel, "Thống kê nhanh", 22, 18, 14, FontStyle.Bold, Color.FromArgb(15, 23, 42));

            var handledRateCard = CreateMetricCard(statisticsPanel, "Tỷ lệ xử lý", new Rectangle(22, 58, 206, 84), Color.FromArgb(34, 197, 94));
            _handledRateValue = handledRateCard.ValueLabel;
            _handledRateDescription = handledRateCard.DescriptionLabel;

            var urgentSummaryCard = CreateMetricCard(statisticsPanel, "Mức khẩn", new Rectangle(22, 152, 206, 84), Color.FromArgb(255, 111, 97));
            _urgentSummaryValue = urgentSummaryCard.ValueLabel;
            _urgentSummaryDescription = urgentSummaryCard.DescriptionLabel;

            var focusAreaCard = CreateMetricCard(statisticsPanel, "Khu vực nổi bật", new Rectangle(22, 246, 206, 84), Color.FromArgb(59, 130, 246));
            _focusAreaValue = focusAreaCard.ValueLabel;
            _focusAreaDescription = focusAreaCard.DescriptionLabel;

            _severityBreakdownLabel = UiFactory.CreateLabel(statisticsPanel, string.Empty, 22, 344, 9.5f, FontStyle.Regular, Color.FromArgb(71, 85, 105));
            _severityBreakdownLabel.AutoSize = false;
            _severityBreakdownLabel.Size = new Size(206, 20);

            _statusBreakdownLabel = UiFactory.CreateLabel(statisticsPanel, string.Empty, 22, 364, 9.5f, FontStyle.Regular, Color.FromArgb(71, 85, 105));
            _statusBreakdownLabel.AutoSize = false;
            _statusBreakdownLabel.Size = new Size(206, 20);

            var publishPanel = UiFactory.CreateCard(canvas, new Rectangle(856, 300, 306, 390), Color.White, Color.FromArgb(210, 223, 236));
            UiFactory.CreateLabel(publishPanel, "Cập nhật tin tức", 22, 18, 14, FontStyle.Bold, Color.FromArgb(15, 23, 42));
            UiFactory.CreateLabel(publishPanel, "Tiêu đề", 22, 58, 9.5f, FontStyle.Bold, Color.FromArgb(100, 116, 139));
            _titleBox = UiFactory.CreateTextBox(publishPanel, new Rectangle(22, 80, 262, 34), "Cảnh báo giao thông khu vực trung tâm");
            UiFactory.CreateLabel(publishPanel, "Nội dung", 22, 128, 9.5f, FontStyle.Bold, Color.FromArgb(100, 116, 139));
            _contentBox = UiFactory.CreateTextBox(publishPanel, new Rectangle(22, 150, 262, 146),
                "Cập nhật tình hình ùn tắc và hướng dẫn người dân chọn lộ trình thay thế an toàn.",
                multiline: true);
            UiFactory.CreateLabel(publishPanel, "Trạng thái đăng", 22, 316, 9.5f, FontStyle.Bold, Color.FromArgb(100, 116, 139));
            _statusBox = UiFactory.CreateTextBox(publishPanel, new Rectangle(22, 338, 118, 34), "Chờ xuất bản");
            var publishButton = UiFactory.CreateButton(publishPanel, "Đăng tin", new Rectangle(154, 334, 130, 40), Color.FromArgb(255, 111, 97), Color.White);
            publishButton.Click += (s, e) =>
            {
                _publishedNewsCount++;
                _statusBox.Text = $"Đã đăng {DateTime.Now:HH:mm}";
                RenderDashboard();
                MessageBox.Show($"Đã cập nhật tin tức:\n{_titleBox.Text}\n\n{_contentBox.Text}", "Admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            var backButton = UiFactory.CreateButton(canvas, "Quay lại", new Rectangle(38, 708, 120, 36), Color.FromArgb(226, 232, 240), Color.FromArgb(15, 23, 42));
            backButton.Click += (s, e) => Close();
            var logoutButton = UiFactory.CreateButton(canvas, "Đăng xuất", new Rectangle(174, 708, 120, 36), Color.FromArgb(220, 53, 69), Color.White);
            logoutButton.Click += (s, e) => SessionNavigator.Logout();

            RenderDashboard();
        }

        private void RenderDashboard()
        {
            _reportList.Items.Clear();
            foreach (var report in _reports.OrderByDescending(r => ParseTime(r.Time)))
            {
                _reportList.Items.Add(new ListViewItem(new[] { report.Area, report.Severity, report.Status, report.Time }));
            }

            var urgentCount = _reports.Count(r => r.Severity == "Khẩn");
            var handledCount = _reports.Count(r => r.Status is "Đã điều phối" or "Đã đóng");
            var processingCount = _reports.Count(r => r.Status is "Đang xác minh" or "Chờ xử lý");
            var activeTeams = Math.Min(9, Math.Max(5, handledCount + 3));
            var handledRate = _reports.Count == 0 ? 0 : (int)Math.Round((double)handledCount / _reports.Count * 100);
            var focusArea = _reports
                .GroupBy(r => r.Area)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key)
                .FirstOrDefault();
            var focusAreaName = focusArea?.Key ?? "Chưa có";
            var focusAreaCount = focusArea?.Count() ?? 0;

            _reportCountValue.Text = _reports.Count.ToString();
            _reportCountDescription.Text = $"{processingCount} báo cáo đang cần theo dõi";

            _newsCountValue.Text = _publishedNewsCount.ToString();
            _newsCountDescription.Text = $"{Math.Max(1, _publishedNewsCount / 12)} bản tin nổi bật trong ngày";

            _urgentCountValue.Text = urgentCount.ToString();
            _urgentCountDescription.Text = $"{_reports.Count(r => r.Status == "Đang xác minh")} ca đang xác minh";

            _teamCountValue.Text = activeTeams.ToString();
            _teamCountDescription.Text = $"{Math.Max(1, activeTeams - 2)} đội sẵn sàng điều phối";

            _handledRateValue.Text = $"{handledRate}%";
            _handledRateDescription.Text = $"{handledCount}/{_reports.Count} báo cáo đã xử lý hoặc điều phối";

            _urgentSummaryValue.Text = $"{urgentCount} ca";
            _urgentSummaryDescription.Text = $"{Math.Max(0, urgentCount - 1)} ca khẩn đã có đội tiếp cận";

            _focusAreaValue.Text = focusAreaName;
            _focusAreaDescription.Text = $"{focusAreaCount} báo cáo tập trung tại khu vực này";

            _severityBreakdownLabel.Text =
                $"Mức độ: Khẩn {_reports.Count(r => r.Severity == "Khẩn")} | Trung bình {_reports.Count(r => r.Severity == "Trung bình")} | Thấp {_reports.Count(r => r.Severity == "Thấp")}";
            _statusBreakdownLabel.Text =
                $"Trạng thái: Điều phối {_reports.Count(r => r.Status == "Đã điều phối")} | Xác minh {_reports.Count(r => r.Status == "Đang xác minh")} | Chờ {_reports.Count(r => r.Status == "Chờ xử lý")} | Đóng {_reports.Count(r => r.Status == "Đã đóng")}";
            _updatedAtLabel.Text = $"Cập nhật lúc {DateTime.Now:HH:mm}";
        }

        private void AddIncomingReport()
        {
            var minute = DateTime.Now.Minute;
            var area = _areas[minute % _areas.Length];
            var severity = _severities[minute % _severities.Length];
            var status = severity == "Khẩn" ? "Đang xác minh" : "Chờ xử lý";
            var time = DateTime.Now.ToString("HH:mm");

            _reports.Add(new AdminReport(area, severity, status, time));
        }

        private void ExportStatistics()
        {
            var summary =
                $"Tổng báo cáo: {_reports.Count}\n" +
                $"Khẩn cấp: {_reports.Count(r => r.Severity == "Khẩn")}\n" +
                $"Đã điều phối hoặc hoàn tất: {_reports.Count(r => r.Status is "Đã điều phối" or "Đã đóng")}\n" +
                $"Tin tức đã đăng: {_publishedNewsCount}\n" +
                $"Khu vực nhiều báo cáo nhất: {_reports.GroupBy(r => r.Area).OrderByDescending(g => g.Count()).First().Key}";

            MessageBox.Show(summary, "Thống kê admin", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static DateTime ParseTime(string time)
        {
            return DateTime.TryParse(time, out var parsed) ? parsed : DateTime.MinValue;
        }

        private static CardWidgets CreateOverviewCard(Control parent, string title, Rectangle bounds, Color accent)
        {
            var card = UiFactory.CreateCard(parent, bounds, Color.White, Color.FromArgb(220, accent));
            UiFactory.CreateLabel(card, title, 18, 18, 10, FontStyle.Bold, Color.FromArgb(100, 116, 139));
            var valueLabel = UiFactory.CreateLabel(card, "0", 18, 46, 24, FontStyle.Bold, accent);
            var descriptionLabel = UiFactory.CreateLabel(card, string.Empty, 18, 84, 9.5f, FontStyle.Regular, Color.FromArgb(30, 41, 59));
            return new CardWidgets(valueLabel, descriptionLabel);
        }

        private static CardWidgets CreateMetricCard(Control parent, string title, Rectangle bounds, Color accent)
        {
            var card = UiFactory.CreateCard(parent, bounds, Color.FromArgb(248, 250, 252), Color.FromArgb(225, 232, 240));
            UiFactory.CreateLabel(card, title, 14, 12, 9.5f, FontStyle.Bold, Color.FromArgb(100, 116, 139));
            var valueLabel = UiFactory.CreateLabel(card, "--", 14, 34, 19, FontStyle.Bold, accent);
            var descriptionLabel = UiFactory.CreateLabel(card, string.Empty, 14, 60, 8.8f, FontStyle.Regular, Color.FromArgb(51, 65, 85));
            descriptionLabel.AutoSize = false;
            descriptionLabel.Size = new Size(bounds.Width - 28, 18);
            return new CardWidgets(valueLabel, descriptionLabel);
        }

        private sealed record AdminReport(string Area, string Severity, string Status, string Time);

        private sealed record CardWidgets(Label ValueLabel, Label DescriptionLabel);
    }
}
