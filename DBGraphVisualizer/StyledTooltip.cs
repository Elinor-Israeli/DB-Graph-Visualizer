

using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DBGraphVisualizer
{
    /// <summary>
    /// A custom tooltip form that supports basic text styling using **bold italic** markers.
    /// </summary>
    public class StyledTooltip : Form
    {
        private string _text = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="StyledTooltip"/> class.
        /// </summary>

        public StyledTooltip()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = ColorTranslator.FromHtml("#fde8dc");
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.Padding = new Padding(8);
            this.DoubleBuffered = true;
        }

        /// <summary>
        /// Paints the styled tooltip text onto the form using basic markdown-style formatting.
        /// </summary>
        /// <param name="e">Paint event arguments.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            var regularFont = new Font("Segoe UI", 9, FontStyle.Regular);
            var boldItalicFont = new Font("Segoe UI", 9, FontStyle.Bold | FontStyle.Italic);
            var brush = new SolidBrush(Color.FromArgb(43, 87, 154));

            float x = 10f;
            float y = 10f;

            string[] parts = Regex.Split(_text, @"(\*\*.*?\*\*)");

            foreach (var part in parts)
            {
                string displayText = part;
                Font font = regularFont;

                if (part.StartsWith("**") && part.EndsWith("**"))
                {
                    displayText = part.Substring(2, part.Length - 4);
                    font = boldItalicFont;
                }

                g.DrawString(displayText, font, brush, x, y);
                x += g.MeasureString(displayText, regularFont).Width;
            }
        }
        /// <summary>
        /// Displays the tooltip at the specified screen location with styled content.
        /// </summary>
        /// <param name="text">The text to display. Use **...** to apply bold italic styling.</param>
        /// <param name="screenLocation">The location on the screen where the tooltip should appear.</param>

        public void ShowTooltip(string text, Point screenLocation)
        {
            _text = text;

            using var g = CreateGraphics();
            var regularFont = new Font("Segoe UI", 9, FontStyle.Regular);
            var boldItalicFont = new Font("Segoe UI", 9, FontStyle.Bold | FontStyle.Italic);

            float width = 20f;
            float height = 0f;

            string[] parts = Regex.Split(_text, @"(\*\*.*?\*\*)");

            foreach (var part in parts)
            {
                string displayText = part;
                Font font = regularFont;

                if (part.StartsWith("**") && part.EndsWith("**"))
                {
                    displayText = part.Substring(2, part.Length - 4);
                    font = boldItalicFont;
                }

                var size = g.MeasureString(displayText, font);
                width += size.Width;
                height = Math.Max(height, size.Height);
            }

            this.Size = new Size((int)Math.Ceiling(width), (int)Math.Ceiling(height + 20));
            this.Location = screenLocation;
            this.Invalidate();
            this.Show();
        }

        public void HideTooltip()
        {
            this.Hide();
        }
    }
}



