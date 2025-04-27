using System;
using System.Drawing;
using System.Windows.Forms;

namespace RATAPP.Helpers
{
    /// <summary>
    /// Factory class for creating standardized UI components for the pairing form.
    /// Use this pattern for other forms that need similar component creation.
    /// </summary>
    public static class PairingFormComponentFactory
    {
        /// <summary>
        /// Creates a styled label with standard properties
        /// </summary>
        /// <param name="text">The label text</param>
        /// <param name="location">The location of the label</param>
        /// <returns>A configured Label control</returns>
        public static Label CreateLabel(string text, Point? location = null)
        {
            var label = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };

            if (location.HasValue)
            {
                label.Location = location.Value;
            }

            return label;
        }

        /// <summary>
        /// Creates a complete form section with label and input
        /// </summary>
        /// <param name="labelText">The label text</param>
        /// <param name="input">The input control</param>
        /// <returns>A panel containing the label and input</returns>
        public static Panel CreateFormField(string labelText, Control input)
        {
            var panel = new Panel
            {
                Height = 40,
                Dock = DockStyle.Top,
                Padding = new Padding(10)
            };

            var label = CreateLabel(labelText);
            label.Width = 120;
            label.Location = new Point(0, 10);

            input.Location = new Point(130, 5);
            input.Anchor = AnchorStyles.Left | AnchorStyles.Right;

            panel.Controls.AddRange(new Control[] { label, input });
            return panel;
        }

        /// <summary>
        /// Creates a styled tab page for pairing form
        /// </summary>
        /// <param name="title">The tab page title</param>
        /// <param name="content">Optional panel containing the tab's content</param>
        /// <returns>A configured TabPage</returns>
        public static TabPage CreatePairingTabPage(string title, Panel content = null)
        {
            var tabPage = new TabPage(title)
            {
                Padding = new Padding(20),
                BackColor = Color.White
            };

            if (content != null)
            {
                content.Dock = DockStyle.Fill;
                tabPage.Controls.Add(content);
            }

            return tabPage;
        }

        /// <summary>
        /// Creates a button panel with standard layout
        /// </summary>
        /// <param name="primaryButton">The primary action button</param>
        /// <param name="secondaryButton">The secondary action button</param>
        /// <returns>A panel containing the buttons</returns>
        public static Panel CreateButtonPanel(Button primaryButton, Button secondaryButton)
        {
            var panel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Bottom,
                Padding = new Padding(0, 10, 0, 0)
            };

            // Style and position buttons
            FormStyleHelper.ApplyButtonStyle(primaryButton, true);
            FormStyleHelper.ApplyButtonStyle(secondaryButton, false);

            primaryButton.Location = new Point(panel.Width / 2 - 160, 10);
            secondaryButton.Location = new Point(panel.Width / 2 + 10, 10);

            panel.Controls.AddRange(new Control[] { primaryButton, secondaryButton });
            return panel;
        }

        /// <summary>
        /// Creates a data grid for multiple pairings
        /// </summary>
        /// <returns>A configured DataGridView</returns>
        public static DataGridView CreatePairingsGrid()
        {
            var grid = new DataGridView();
            FormStyleHelper.ApplyDataGridViewStyle(grid);

            // Add standard columns
            grid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "PairingID", HeaderText = "Pairing ID" },
                new DataGridViewTextBoxColumn { Name = "Project", HeaderText = "Project" },
                new DataGridViewTextBoxColumn { Name = "Dam", HeaderText = "Dam" },
                new DataGridViewTextBoxColumn { Name = "Sire", HeaderText = "Sire" },
                new DataGridViewTextBoxColumn { Name = "PairingDate", HeaderText = "Pairing Date" },
                new DataGridViewButtonColumn 
                { 
                    Name = "Remove", 
                    HeaderText = "Remove", 
                    Text = "Remove", 
                    UseColumnTextForButtonValue = true 
                }
            });

            return grid;
        }

        /// <summary>
        /// Creates an information panel with standard styling
        /// </summary>
        /// <param name="title">The panel title</param>
        /// <param name="infoText">The information text to display</param>
        /// <returns>A configured GroupBox containing the information</returns>
        public static GroupBox CreateInfoPanel(string title, string infoText)
        {
            var infoPanel = new GroupBox();
            FormStyleHelper.ApplyGroupBoxStyle(infoPanel, title, 150);
            infoPanel.Dock = DockStyle.Bottom;

            var infoLabel = new Label
            {
                Text = infoText,
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(20, 30),
                Size = new Size(700, 100),
                AutoSize = false
            };

            infoPanel.Controls.Add(infoLabel);
            return infoPanel;
        }
    }
}
