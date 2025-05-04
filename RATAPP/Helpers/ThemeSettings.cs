using System;
using System.Drawing;
using System.Windows.Forms;

namespace RATAPP.Helpers
{
    /// <summary>
    /// Contains theme settings for the application.
    /// </summary>
    public class ThemeSettings
    {
        // Basic colors
        public Color BackgroundColor { get; set; }
        public Color TextColor { get; set; }
        public Color BorderColor { get; set; }

        // Control-specific colors
        public Color ButtonColor { get; set; }
        public Color ButtonTextColor { get; set; }
        public Color TextBoxColor { get; set; }
        public Color HeaderColor { get; set; }
        public Color HeaderTextColor { get; set; }
        public Color CellColor { get; set; }
        public Color SelectionColor { get; set; }
        public Color SelectionTextColor { get; set; }

        // Border styles
        public BorderStyle BorderStyle { get; set; }

        /// <summary>
        /// Creates a default light theme.
        /// </summary>
        public static ThemeSettings DefaultLight()
        {
            return new ThemeSettings
            {
                BackgroundColor = Color.White,
                TextColor = Color.Black,
                BorderColor = Color.FromArgb(200, 200, 200),
                ButtonColor = Color.FromArgb(0, 120, 212),
                ButtonTextColor = Color.White,
                TextBoxColor = Color.White,
                HeaderColor = Color.FromArgb(240, 240, 240),
                HeaderTextColor = Color.Black,
                CellColor = Color.White,
                SelectionColor = Color.FromArgb(0, 120, 212),
                SelectionTextColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        /// <summary>
        /// Creates a default dark theme.
        /// </summary>
        public static ThemeSettings DefaultDark()
        {
            return new ThemeSettings
            {
                BackgroundColor = Color.FromArgb(30, 30, 30),
                TextColor = Color.White,
                BorderColor = Color.FromArgb(60, 60, 60),
                ButtonColor = Color.FromArgb(0, 120, 212),
                ButtonTextColor = Color.White,
                TextBoxColor = Color.FromArgb(45, 45, 45),
                HeaderColor = Color.FromArgb(50, 50, 50),
                HeaderTextColor = Color.White,
                CellColor = Color.FromArgb(40, 40, 40),
                SelectionColor = Color.FromArgb(0, 120, 212),
                SelectionTextColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
        }
    }
}
