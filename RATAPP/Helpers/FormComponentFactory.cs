using System.Drawing;
using System.Windows.Forms;

namespace RATAPP.Helpers
{
    /// <summary>
    /// Factory for creating common form components used in breeding-related forms
    /// </summary>
    public static class FormComponentFactory
    {
        public static TabPage CreateTabPage(string title)
        {
            return new TabPage(title);
        }

        public static GroupBox CreateFormSection(string title, DockStyle dockStyle, int height)
        {
            return new GroupBox
            {
                Text = title,
                Dock = dockStyle,
                Height = height,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Padding = new Padding(15)
            };
        }

        public static Panel CreateFormField(string labelText, Control control)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Margin = new Padding(0, 0, 0, 10)
            };

            var label = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, 10),
                AutoSize = true
            };

            control.Location = new Point(150, 5);
            control.Width = 300;

            panel.Controls.Add(label);
            panel.Controls.Add(control);

            return panel;
        }

        public static Panel CreateButtonPanel(Button primaryButton, Button secondaryButton = null)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(0, 10, 0, 0)
            };

            if (primaryButton != null)
            {
                FormStyleHelper.ApplyButtonStyle(primaryButton, true);
                primaryButton.Location = new Point(180, 10);
                panel.Controls.Add(primaryButton);
            }

            if (secondaryButton != null)
            {
                FormStyleHelper.ApplyButtonStyle(secondaryButton, false);
                secondaryButton.Location = new Point(340, 10);
                panel.Controls.Add(secondaryButton);
            }

            return panel;
        }

        public static GroupBox CreateInfoPanel(string title, string infoText)
        {
            var infoPanel = new GroupBox
            {
                Text = title,
                Dock = DockStyle.Bottom,
                Height = 150,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Padding = new Padding(15)
            };

            var infoLabel = new Label
            {
                Text = infoText,
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(20, 30),
                Size = new Size(700, 100)
            };

            infoPanel.Controls.Add(infoLabel);
            return infoPanel;
        }

        public static DataGridView CreateDataGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = true,
                Font = new Font("Segoe UI", 9),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(230, 230, 230),
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(247, 247, 247)
                }
            };

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            return grid;
        }

        public static Panel CreateHeaderPanel(string title)
        {
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(0, 120, 212)
            };

            var headerLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 12)
            };

            headerPanel.Controls.Add(headerLabel);
            return headerPanel;
        }
    }
}
