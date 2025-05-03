using System;
using System.Drawing;
using System.Windows.Forms;

namespace RATAPP.Helpers
{
    /// <summary>
    /// Centralizes form styling to maintain consistency across the application.
    /// Reuse this helper in any form that needs standard styling.
    /// </summary>
    public static class FormStyleHelper
    {
        // Define standard colors and fonts as constants for reuse
        private static readonly Color PRIMARY_COLOR = Color.FromArgb(0, 120, 212);
        private static readonly Color SECONDARY_COLOR = Color.FromArgb(240, 240, 240);
        private static readonly Color TEXT_COLOR = Color.FromArgb(60, 60, 60);
        private static readonly Font HEADER_FONT = new Font("Segoe UI", 20, FontStyle.Bold);
        private static readonly Font STANDARD_FONT = new Font("Segoe UI", 10);
        private static readonly Font BOLD_FONT = new Font("Segoe UI", 10, FontStyle.Bold);

        /// <summary>
        /// Applies standard header styling to any panel used as a form header
        /// </summary>
        /// <param name="header">The panel to style as a header</param>
        /// <param name="title">The title text to display in the header</param>
        public static void ApplyHeaderStyle(Panel header, string title)
        {
            // Standard header styling that can be reused across forms
            header.Dock = DockStyle.Top;
            header.Height = 60;
            header.BackColor = PRIMARY_COLOR;

            var headerLabel = new Label
            {
                Text = title,
                Font = HEADER_FONT,
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 12)
            };
            header.Controls.Add(headerLabel);
        }

        /// <summary>
        /// Applies consistent button styling based on primary/secondary role
        /// </summary>
        /// <param name="button">The button to style</param>
        /// <param name="isPrimary">True for primary action buttons, false for secondary actions</param>
        /// <param name="width">Optional custom width for the button</param>
        /// <param name="height">Optional custom height for the button</param>
        public static void ApplyButtonStyle(Button button, bool isPrimary, int width = 150, int height = 40)
        {
            button.Width = width;
            button.Height = height;
            button.Font = STANDARD_FONT;
            button.FlatStyle = FlatStyle.Flat;
            button.Cursor = Cursors.Hand;

            if (isPrimary)
            {
                button.BackColor = PRIMARY_COLOR;
                button.ForeColor = Color.White;
                button.FlatAppearance.BorderSize = 0;
            }
            else
            {
                button.BackColor = SECONDARY_COLOR;
                button.ForeColor = Color.Black;
                button.FlatAppearance.BorderSize = 1;
            }
        }

        /// <summary>
        /// Applies standard styling to ComboBox controls
        /// </summary>
        /// <param name="comboBox">The ComboBox to style</param>
        /// <param name="width">Optional custom width for the ComboBox</param>
        public static void ApplyComboBoxStyle(ComboBox comboBox, int width = 400)
        {
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.Font = STANDARD_FONT;
            comboBox.Width = width;
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.BackColor = Color.White;
        }

        /// <summary>
        /// Applies standard styling to GroupBox controls
        /// </summary>
        /// <param name="groupBox">The GroupBox to style</param>
        /// <param name="title">The title text for the GroupBox</param>
        /// <param name="height">Optional custom height for the GroupBox</param>
        public static void ApplyGroupBoxStyle(GroupBox groupBox, string title, int height = 300)
        {
            groupBox.Text = title;
            groupBox.Height = height;
            groupBox.Font = BOLD_FONT;
            groupBox.ForeColor = TEXT_COLOR;
            groupBox.Padding = new Padding(15);
        }

        /// <summary>
        /// Applies standard styling to DataGridView controls
        /// </summary>
        /// <param name="grid">The DataGridView to style</param>
        public static void ApplyDataGridViewStyle(DataGridView grid)
        {
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            grid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AllowUserToAddRows = false;
            grid.Font = STANDARD_FONT;
            grid.BackColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.RowHeadersVisible = false;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Color.FromArgb(230, 230, 230);

            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(247, 247, 247)
            };

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = PRIMARY_COLOR,
                ForeColor = Color.White,
                Font = BOLD_FONT
            };
        }

        /// <summary>
        /// Applies standard styling to TextBox controls
        /// </summary>
        /// <param name="textBox">The TextBox to style</param>
        /// <param name="width">Optional custom width for the TextBox</param>
        public static void ApplyTextBoxStyle(TextBox textBox, int width = 400)
        {
            textBox.Font = STANDARD_FONT;
            textBox.Width = width;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = Color.White;
        }
    }
}
