using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFormsApp
{
    public class KhaiForm : Form
    {
        public KhaiForm()
        {
            Text = "Cán bộ Khải";
            ClientSize = new Size(720, 500);
            BackColor = Color.FromArgb(9, 28, 15);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var canvas = new Panel { Dock = DockStyle.Fill };
            canvas.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = canvas.ClientRectangle;

                using (var bg = new LinearGradientBrush(rect, Color.FromArgb(14, 45, 22), Color.FromArgb(8, 20, 14), LinearGradientMode.ForwardDiagonal))
                {
                    g.FillRectangle(bg, rect);
                }

                using (var accent = new Pen(Color.FromArgb(140, 76, 175, 80), 3))
                {
                    g.DrawEllipse(accent, 475, 40, 170, 170);
                    g.DrawPolygon(accent, new[]
                    {
                        new Point(510, 260),
                        new Point(650, 260),
                        new Point(580, 160)
                    });
                }

                DrawText(g, "ĐỘI KỸ THUẬT", 42, 55, 11, FontStyle.Bold, Color.FromArgb(157, 231, 160));
                DrawText(g, "Khải", 40, 145, 32, FontStyle.Bold, Color.White);
                DrawText(g, "Đơn vị kỹ thuật", 42, 198, 13, FontStyle.Regular, Color.FromArgb(182, 224, 184));

                DrawInfo(g, "Mã đơn vị", "TECH-03", 42, 255);
                DrawInfo(g, "Trạng thái", "Đang trực hệ thống", 42, 295);
                DrawInfo(g, "Chuyên trách", "Giám sát dữ liệu và thiết bị", 42, 335);
                DrawInfo(g, "Liên lạc", "Kênh nội bộ số 5", 42, 375);
            };

            var backButton = new Button
            {
                Text = "Quay lại",
                Size = new Size(120, 40),
                Location = new Point(42, 430),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            backButton.FlatAppearance.BorderSize = 0;
            backButton.Click += (s, e) => Close();

            Controls.Add(canvas);
            canvas.Controls.Add(backButton);
        }

        private void DrawInfo(Graphics g, string label, string value, int x, int y)
        {
            DrawText(g, label, x, y, 10, FontStyle.Bold, Color.FromArgb(157, 231, 160));
            DrawText(g, value, x + 160, y, 10, FontStyle.Regular, Color.White);
        }

        private void DrawText(Graphics g, string text, int x, int y, float fontSize, FontStyle style, Color color)
        {
            using (var font = new Font("Segoe UI", fontSize, style))
            using (var brush = new SolidBrush(color))
            {
                g.DrawString(text, font, brush, x, y);
            }
        }
    }
}
