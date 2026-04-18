using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFormsApp
{
    public class NhanForm : Form
    {
        public NhanForm()
        {
            Text = "Cán bộ Nhân";
            ClientSize = new Size(720, 500);
            BackColor = Color.FromArgb(8, 20, 38);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var canvas = new Panel { Dock = DockStyle.Fill };
            canvas.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = canvas.ClientRectangle;

                using (var bg = new LinearGradientBrush(rect, Color.FromArgb(11, 28, 54), Color.FromArgb(4, 16, 34), LinearGradientMode.ForwardDiagonal))
                {
                    g.FillRectangle(bg, rect);
                }

                using (var accent = new Pen(Color.FromArgb(120, 33, 150, 243), 3))
                {
                    g.DrawEllipse(accent, 470, 40, 180, 180);
                    g.DrawRectangle(accent, 40, 40, 230, 90);
                }

                DrawText(g, "HỒ SƠ CÁN BỘ", 42, 55, 11, FontStyle.Bold, Color.FromArgb(125, 182, 255));
                DrawText(g, "Nhân", 40, 145, 32, FontStyle.Bold, Color.White);
                DrawText(g, "Đơn vị tuần tra", 42, 198, 13, FontStyle.Regular, Color.FromArgb(175, 198, 226));

                DrawInfo(g, "Mã đơn vị", "PATROL-01", 42, 255);
                DrawInfo(g, "Trạng thái", "Sẵn sàng nhận nhiệm vụ", 42, 295);
                DrawInfo(g, "Khu vực", "Phân khu trung tâm", 42, 335);
                DrawInfo(g, "Liên lạc", "Kênh nội bộ số 3", 42, 375);
            };

            var backButton = new Button
            {
                Text = "Quay lại",
                Size = new Size(120, 40),
                Location = new Point(42, 430),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(33, 150, 243),
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
            DrawText(g, label, x, y, 10, FontStyle.Bold, Color.FromArgb(125, 182, 255));
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
