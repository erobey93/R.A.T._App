using System;
using System.Drawing;
using System.Windows.Forms;

namespace RATAPP.Helpers
{
    public static class ResponsiveFormComponentFactory
    {
        // Create a responsive panel that automatically adjusts its layout
        public static Panel CreateResponsivePanel(DockStyle dockStyle = DockStyle.Fill)
        {
            var panel = new Panel
            {
                Dock = dockStyle,
                AutoSize = false,
                AutoScroll = true
            };

            return panel;
        }

        // Create a responsive tab control
        public static TabControl CreateResponsiveTabControl()
        {
            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                Padding = new Point(12, 4),
                SizeMode = TabSizeMode.FillToRight
            };

            return tabControl;
        }

        // Create a responsive tab page
        public static TabPage CreateResponsiveTabPage(string text)
        {
            var tabPage = new TabPage(text)
            {
                Padding = new Padding(10),
                UseVisualStyleBackColor = true
            };

            return tabPage;
        }

        // Create a responsive data grid view
        public static DataGridView CreateResponsiveDataGridView()
        {
            var dataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
            };

            return dataGridView;
        }

        // Create a responsive form section with a title
        public static Panel CreateResponsiveFormSection(string title, DockStyle dockStyle = DockStyle.Top, int height = 0)
        {
            var panel = new Panel
            {
                Dock = dockStyle,
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };

            if (height > 0)
            {
                panel.Height = height;
            }

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                AutoSize = true,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 0, 0, 10)
            };

            panel.Controls.Add(titleLabel);

            return panel;
        }

        // Create a responsive form field with label and control
        public static Panel CreateResponsiveFormField(string labelText, Control control)
        {
            var panel = new TableLayoutPanel
            {
                RowCount = 2,
                ColumnCount = 1,
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(0, 0, 0, 10)
            };

            var label = new Label
            {
                Text = labelText,
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                Dock = DockStyle.Fill
            };

            panel.Controls.Add(label, 0, 0);

            // Create a container for the control to ensure it fills the width
            var controlContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                Margin = new Padding(0, 3, 0, 0)
            };

            control.Dock = DockStyle.Fill;
            controlContainer.Controls.Add(control);

            panel.Controls.Add(controlContainer, 0, 1);

            // Set row heights
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            return panel;
        }

        // Create a responsive button panel for form actions
        public static Panel CreateResponsiveButtonPanel(params Button[] buttons)
        {
            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Dock = DockStyle.Bottom,
                Padding = new Padding(0, 10, 0, 0)
            };

            foreach (var button in buttons)
            {
                if (button != null)
                {
                    button.AutoSize = true;
                    button.Margin = new Padding(0, 0, 10, 0);
                    panel.Controls.Add(button);
                }
            }

            return panel;
        }

        // Create a responsive header panel
        public static Panel CreateResponsiveHeaderPanel(string title)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(0, 120, 212)
            };

            var titleLabel = new Label
            {
                Text = title,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 10)
            };

            panel.Controls.Add(titleLabel);

            return panel;
        }

        // Create a responsive info panel
        public static Panel CreateResponsiveInfoPanel(string title, string content)
        {
            var panel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10),
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 10)
            };

            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Dock = DockStyle.Top
            };

            var contentLabel = new Label
            {
                Text = content,
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                Dock = DockStyle.Fill,
                MaximumSize = new Size(800, 0)
            };

            panel.Controls.Add(contentLabel);
            panel.Controls.Add(titleLabel);

            return panel;
        }
    }
}
