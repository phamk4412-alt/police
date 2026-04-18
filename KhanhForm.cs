using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFormsApp
{
    public class KhanhForm : Form
    {
        public KhanhForm()
        {
            Text = "Cán bộ Khánh";
            ClientSize = new Size(720, 500);
            BackColor = Color.FromArgb(26, 24, 8);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var canvas = new Panel { Dock = DockStyle.Fill };
            canvas.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = canvas.ClientRectangle;

                using (var bg = new LinearGradientBrush(rect, Color.FromArgb(45, 37, 8), Color.FromArgb(19, 18, 7), LinearGradientMode.ForwardDiagonal))
                {
                    g.FillRectangle(bg, rect);
                }

                using (var accent = new Pen(Color.FromArgb(150, 255, 193, 7), 3))
                {
                    g.DrawRectangle(accent, 455, 55, 190, 140);
                    g.DrawLine(accent, 455, 225, 645, 225);
                    g.DrawLine(accent, 550, 55, 550, 195);
                }

                DrawText(g, "TRUNG TÂM CHỈ HUY", 42, 55, 11, FontStyle.Bold, Color.FromArgb(255, 220, 120));
                DrawText(g, "Khánh", 40, 145, 32, FontStyle.Bold, Color.White);
                DrawText(g, "Đơn vị chỉ huy", 42, 198, 13, FontStyle.Regular, Color.FromArgb(236, 222, 171));

                DrawInfo(g, "Mã đơn vị", "COMMAND-02", 42, 255);
                DrawInfo(g, "Trạng thái", "Đang giám sát hiện trường", 42, 295);
                DrawInfo(g, "Chuyên trách", "Điều phối lực lượng", 42, 335);
                DrawInfo(g, "Liên lạc", "Kênh nội bộ số 1", 42, 375);
            };

            var backButton = new Button
            {
                Text = "Quay lại",
                Size = new Size(120, 40),
                Location = new Point(42, 430),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.FromArgb(34, 28, 8),
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
            DrawText(g, label, x, y, 10, FontStyle.Bold, Color.FromArgb(255, 220, 120));
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
