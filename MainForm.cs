using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFormsApp
{
    public class MainForm : Form
    {
        private Panel canvas = null!;
        private Label titleLabel = null!;
        private Label subtitleLabel = null!;
        private Label unitLabel = null!;
        private Button btnNhan = null!;
        private Button btnKhanh = null!;
        private Button btnKhai = null!;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "Ứng dụng cảnh sát";
            ClientSize = new Size(960, 640);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(8, 18, 34);

            canvas = new Panel
            {
                Dock = DockStyle.Fill
            };
            canvas.Paint += Canvas_Paint;
            Controls.Add(canvas);

            unitLabel = new Label
            {
                Text = "POLICE CONTROL",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(130, 182, 255),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(60, 44)
            };

            titleLabel = new Label
            {
                Text = "TRUNG TÂM ĐIỀU PHỐI",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(56, 72)
            };

            subtitleLabel = new Label
            {
                Text = "Chọn cán bộ để truy cập nhanh vào hồ sơ làm việc.",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(173, 189, 214),
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(60, 124)
            };

            btnNhan = CreateOfficerButton("Nhân", "Đơn vị tuần tra", Color.FromArgb(33, 150, 243), 60, 220);
            btnKhanh = CreateOfficerButton("Khánh", "Đơn vị chỉ huy", Color.FromArgb(255, 193, 7), 330, 220);
            btnKhai = CreateOfficerButton("Khải", "Đơn vị kỹ thuật", Color.FromArgb(76, 175, 80), 600, 220);

            btnNhan.Click += (s, e) => OpenPage(new NhanForm());
            btnKhanh.Click += (s, e) => OpenPage(new KhanhForm());
            btnKhai.Click += (s, e) => OpenPage(new KhaiForm());

            canvas.Controls.Add(unitLabel);
            canvas.Controls.Add(titleLabel);
            canvas.Controls.Add(subtitleLabel);
            canvas.Controls.Add(btnNhan);
            canvas.Controls.Add(btnKhanh);
            canvas.Controls.Add(btnKhai);
        }

        private Button CreateOfficerButton(string officerName, string officerRole, Color accentColor, int x, int y)
        {
            var button = new Button
            {
                Size = new Size(240, 260),
                Location = new Point(x, y),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(13, 30, 54),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Text = officerName + Environment.NewLine + Environment.NewLine + officerRole,
                Font = new Font("Segoe UI", 17, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            button.FlatAppearance.BorderColor = accentColor;
            button.FlatAppearance.BorderSize = 2;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, 42, 74);

            button.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var topBrush = new LinearGradientBrush(
                    new Rectangle(0, 0, button.Width, 6),
                    accentColor,
                    Color.Transparent,
                    LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(topBrush, 0, 0, button.Width, 6);
                }

                using (var badgePen = new Pen(Color.FromArgb(90, accentColor), 2))
                {
                    g.DrawEllipse(badgePen, 86, 30, 68, 68);
                    g.DrawLine(badgePen, 120, 48, 120, 80);
                    g.DrawLine(badgePen, 104, 64, 136, 64);
                }
            };

            return button;
        }

        private void Canvas_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = canvas.ClientRectangle;
            using (var brush = new LinearGradientBrush(
                rect,
                Color.FromArgb(6, 18, 36),
                Color.FromArgb(14, 34, 63),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, rect);
            }

            using (var gridPen = new Pen(Color.FromArgb(18, 120, 170, 220), 1))
            {
                for (int x = 0; x < rect.Width; x += 40)
                {
                    g.DrawLine(gridPen, x, 0, x, rect.Height);
                }

                for (int y = 0; y < rect.Height; y += 40)
                {
                    g.DrawLine(gridPen, 0, y, rect.Width, y);
                }
            }

            using (var haloPen = new Pen(Color.FromArgb(70, 255, 255, 255), 2))
            {
                g.DrawEllipse(haloPen, 700, -80, 300, 300);
                g.DrawEllipse(haloPen, -120, 460, 280, 280);
            }

            using (var footerBrush = new SolidBrush(Color.FromArgb(150, 170, 190)))
            using (var footerFont = new Font("Segoe UI", 9, FontStyle.Regular))
            {
                g.DrawString(
                    "Hệ thống nội bộ • Nhân • Khánh • Khải",
                    footerFont,
                    footerBrush,
                    60,
                    590);
            }
        }

        private void OpenPage(Form page)
        {
            page.Owner = this;
            page.StartPosition = FormStartPosition.CenterScreen;
            page.Show();
        }
    }
}
