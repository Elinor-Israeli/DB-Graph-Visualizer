using System;
using System.Drawing;
using System.Reflection.Emit;
using System.Windows.Forms;

public class StyledTooltip : Form
{
    private Label label;

    public StyledTooltip()
    {
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = ColorTranslator.FromHtml("#fde8dc"); 
        this.StartPosition = FormStartPosition.Manual;
        this.ShowInTaskbar = false;
        this.TopMost = true;
        this.Padding = new Padding(8);
        this.AutoSize = true;
        this.AutoSizeMode = AutoSizeMode.GrowAndShrink;

        label = new Label
        {
            AutoSize = true,
            ForeColor = System.Drawing.Color.FromArgb(51, 51, 51),
            Font = new System.Drawing.Font("Segoe UI", 9, FontStyle.Italic),
            BackColor = System.Drawing.Color.Transparent
        };

        this.Controls.Add(label);
    }

    public void ShowTooltip(string text, Point screenLocation)
    {
        label.Text = text;
        this.Location = screenLocation;
        this.Show();
    }

    public void HideTooltip()
    {
        this.Hide();
    }
}
