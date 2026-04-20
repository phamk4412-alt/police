using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WinFormsApp
{
    internal static class UiFactory
    {
        public static Panel CreateCanvas(Form form, Color startColor, Color endColor)
        {
            var canvas = new Panel { Dock = DockStyle.Fill };
            canvas.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = canvas.ClientRectangle;

                using var bg = new LinearGradientBrush(rect, startColor, endColor, LinearGradientMode.Vertical);
                g.FillRectangle(bg, rect);

                using var gridPen = new Pen(Color.FromArgb(22, 255, 255, 255), 1);
                for (int x = 0; x < rect.Width; x += 36)
                {
                    g.DrawLine(gridPen, x, 0, x, rect.Height);
                }

                for (int y = 0; y < rect.Height; y += 36)
                {
                    g.DrawLine(gridPen, 0, y, rect.Width, y);
                }
            };

            form.Controls.Add(canvas);
            return canvas;
        }

        public static Panel CreateCard(Control parent, Rectangle bounds, Color backColor, Color borderColor)
        {
            var panel = new Panel
            {
                Bounds = bounds,
                BackColor = backColor
            };

            panel.Paint += (s, e) =>
            {
                var rect = panel.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var pen = new Pen(borderColor, 2);
                e.Graphics.DrawRectangle(pen, rect);
            };

            parent.Controls.Add(panel);
            return panel;
        }

        public static Label CreateLabel(Control parent, string text, int x, int y, float size, FontStyle style, Color color, bool autoSize = true)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(x, y),
                Font = new Font("Segoe UI", size, style),
                ForeColor = color,
                BackColor = Color.Transparent,
                AutoSize = autoSize
            };

            parent.Controls.Add(label);
            return label;
        }

        public static Button CreateButton(Control parent, string text, Rectangle bounds, Color backColor, Color foreColor)
        {
            var button = new Button
            {
                Text = text,
                Bounds = bounds,
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = foreColor,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            parent.Controls.Add(button);
            return button;
        }

        public static ListView CreateListView(Control parent, Rectangle bounds, params (string header, int width)[] columns)
        {
            var list = new ListView
            {
                Bounds = bounds,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.FromArgb(15, 24, 42),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                HeaderStyle = ColumnHeaderStyle.Nonclickable
            };

            foreach (var column in columns)
            {
                list.Columns.Add(column.header, column.width);
            }

            parent.Controls.Add(list);
            return list;
        }

        public static TextBox CreateTextBox(Control parent, Rectangle bounds, string text = "", bool multiline = false)
        {
            var box = new TextBox
            {
                Bounds = bounds,
                Text = text,
                Multiline = multiline,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                BackColor = Color.FromArgb(245, 248, 255),
                ForeColor = Color.FromArgb(25, 35, 55),
                BorderStyle = BorderStyle.FixedSingle
            };

            parent.Controls.Add(box);
            return box;
        }
    }
}
