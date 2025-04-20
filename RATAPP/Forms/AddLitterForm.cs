using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RATAPP.Forms;

namespace RATAPP.Forms
{
    public partial class AddLitterForm : Form
    {
        private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;
        private TabControl tabControl;
        private ComboBox damComboBox;
        private ComboBox sireComboBox;
        private TextBox litterIdTextBox;
        private ComboBox projectComboBox;
        private DateTimePicker litterDatePicker;
        private Button addButton;
        private Button cancelButton;
        private DataGridView multiplelittersGrid;
        private Button addToGridButton;
        private Button saveAllButton;
        private PictureBox loadingSpinner;

        public AddLitterForm(RATAPPLibrary.Data.DbContexts.RatAppDbContext context)
        {
            _context = context;
            InitializeComponents();
            LoadAnimals();
            LoadProjects();
        }

        private void InitializeComponents()
        {
            this.Text = "Add Litters";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Create header
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(0, 120, 212)
            };

            var headerLabel = new Label
            {
                Text = "Add Litter",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 12)
            };
            headerPanel.Controls.Add(headerLabel);

            this.Controls.Add(headerPanel);

            // Create container panel for tabControl
            var tabContainerPanel = new Panel
            {
                Location = new Point(0, headerPanel.Bottom),
                Width = this.ClientSize.Width, // Match form width
                Height = this.ClientSize.Height - headerPanel.Height // Calculate height
            };

            // Create tab control with modern styling
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill, // Dock to fill the container panel
                Font = new Font("Segoe UI", 10),
                Padding = new Point(12, 4)
            };

            // Single litter Tab
            var singlelitterTab = new TabPage("Single litter");
            InitializeSinglelitterTab(singlelitterTab);
            tabControl.TabPages.Add(singlelitterTab);

            // Multiple litters Tab
            var multiplelittersTab = new TabPage("Multiple litters");
            InitializeMultiplelittersTab(multiplelittersTab);
            tabControl.TabPages.Add(multiplelittersTab);

            // Add tabControl to the container panel, then add the panel to the form
            tabContainerPanel.Controls.Add(tabControl);
            this.Controls.Add(tabContainerPanel);

            // Initialize loading spinner
            InitializeLoadingSpinner();
        }

        private void InitializeSinglelitterTab(TabPage tab)
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Create a GroupBox for better visual organization
            var litterGroup = new GroupBox
            {
                Text = "litter Information",
                Dock = DockStyle.Top,
                Height = 300,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Padding = new Padding(15)
            };

            var formPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(10),
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 30F),
                    new ColumnStyle(SizeType.Percent, 70F)
                }
            };

            // Dam (Female)
            var damLabel = new Label
            {
                Text = "Dam (Female):",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(damLabel, 0, 0);

            damComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            formPanel.Controls.Add(damComboBox, 1, 0);

            // Sire (Male)
            var sireLabel = new Label
            {
                Text = "Sire (Male):",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(sireLabel, 0, 1);

            sireComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            formPanel.Controls.Add(sireComboBox, 1, 1);

            // litter ID
            var litterIdLabel = new Label
            {
                Text = "litter ID:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(litterIdLabel, 0, 2);

            litterIdTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            formPanel.Controls.Add(litterIdTextBox, 1, 2);

            // Project
            var projectLabel = new Label
            {
                Text = "Project:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(projectLabel, 0, 3);

            projectComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            formPanel.Controls.Add(projectComboBox, 1, 3);

            // litter Date
            var litterDateLabel = new Label
            {
                Text = "litter Date:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(litterDateLabel, 0, 4);

            litterDatePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300
            };
            formPanel.Controls.Add(litterDatePicker, 1, 4);

            // Buttons
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            addButton = new Button
            {
                Text = "Add Litter",
                Font = new Font("Segoe UI", 10),
                Width = 150,
                Height = 40,
                Location = new Point(180, 10),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += AddButton_Click;
            buttonPanel.Controls.Add(addButton);

            cancelButton = new Button
            {
                Text = "Cancel",
                Font = new Font("Segoe UI", 10),
                Width = 150,
                Height = 40,
                Location = new Point(340, 10),
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            cancelButton.FlatAppearance.BorderSize = 1;
            cancelButton.Click += CancelButton_Click;
            buttonPanel.Controls.Add(cancelButton);

            formPanel.Controls.Add(buttonPanel, 1, 5);

            litterGroup.Controls.Add(formPanel);
            mainPanel.Controls.Add(litterGroup);

            // Add information panel
            var infoPanel = new GroupBox
            {
                Text = "Information",
                Dock = DockStyle.Bottom,
                Height = 150,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Padding = new Padding(15)
            };

            var infoText = new Label
            {
                Text = "• litter ID should be unique for each litter\n" +
                       "• Ensure the dam and sire are of appropriate age for breeding\n" +
                       "• The litter date will be used to calculate expected litter dates\n" +
                       "• You can view all litters in the Breeding History section",
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(20, 30),
                Size = new Size(700, 100)
            };
            infoPanel.Controls.Add(infoText);

            mainPanel.Controls.Add(infoPanel);

            tab.Controls.Add(mainPanel);
        }

        private void InitializeMultiplelittersTab(TabPage tab)
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Create input panel (same as single litter but with "Add to List" button)
            var inputGroup = new GroupBox
            {
                Text = "New litter",
                Dock = DockStyle.Top,
                Height = 250,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Padding = new Padding(15)
            };

            var formPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(10),
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 30F),
                    new ColumnStyle(SizeType.Percent, 70F)
                }
            };

            // Dam (Female)
            var damLabel = new Label
            {
                Text = "Dam (Female):",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(damLabel, 0, 0);

            var multiDamComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            formPanel.Controls.Add(multiDamComboBox, 1, 0);

            // Sire (Male)
            var sireLabel = new Label
            {
                Text = "Sire (Male):",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(sireLabel, 0, 1);

            var multiSireComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            formPanel.Controls.Add(multiSireComboBox, 1, 1);

            // litter ID
            var litterIdLabel = new Label
            {
                Text = "litter ID:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(litterIdLabel, 0, 2);

            var multilitterIdTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            formPanel.Controls.Add(multilitterIdTextBox, 1, 2);

            // Project
            var projectLabel = new Label
            {
                Text = "Project:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(projectLabel, 0, 3);

            var multiProjectComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            formPanel.Controls.Add(multiProjectComboBox, 1, 3);

            // litter Date
            var litterDateLabel = new Label
            {
                Text = "litter Date:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(litterDateLabel, 0, 4);

            var multilitterDatePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300
            };
            formPanel.Controls.Add(multilitterDatePicker, 1, 4);

            // Add to Grid button
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            addToGridButton = new Button
            {
                Text = "Add to List",
                Font = new Font("Segoe UI", 10),
                Width = 150,
                Height = 40,
                Location = new Point(180, 10),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            addToGridButton.FlatAppearance.BorderSize = 0;
            addToGridButton.Click += (s, e) =>
            {
                // Show loading spinner
                loadingSpinner.Visible = true;
                this.Refresh();

                try
                {
                    // Validate inputs
                    if (multiDamComboBox.SelectedIndex == -1 || multiSireComboBox.SelectedIndex == -1 ||
                        string.IsNullOrWhiteSpace(multilitterIdTextBox.Text) || multiProjectComboBox.SelectedIndex == -1)
                    {
                        MessageBox.Show("Please fill in all required fields", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Add to grid
                    multiplelittersGrid.Rows.Add(
                        multiDamComboBox.SelectedItem.ToString(),
                        multiSireComboBox.SelectedItem.ToString(),
                        multilitterIdTextBox.Text,
                        multiProjectComboBox.SelectedItem.ToString(),
                        multilitterDatePicker.Value.ToShortDateString()
                    );

                    // Clear inputs for next entry
                    multilitterIdTextBox.Text = "";

                    // Simulate a brief delay for the operation
                    System.Threading.Thread.Sleep(300);
                }
                finally
                {
                    // Hide loading spinner
                    loadingSpinner.Visible = false;
                    this.Refresh();
                }
            };
            buttonPanel.Controls.Add(addToGridButton);

            formPanel.Controls.Add(buttonPanel, 1, 5);

            inputGroup.Controls.Add(formPanel);
            mainPanel.Controls.Add(inputGroup);

            // Create grid for multiple litters
            var gridGroup = new GroupBox
            {
                Text = "litters to Add",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Padding = new Padding(15)
            };

            multiplelittersGrid = new DataGridView
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

            multiplelittersGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            multiplelittersGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Dam", HeaderText = "Dam" },
                new DataGridViewTextBoxColumn { Name = "Sire", HeaderText = "Sire" },
                new DataGridViewTextBoxColumn { Name = "litterID", HeaderText = "litter ID" },
                new DataGridViewTextBoxColumn { Name = "Project", HeaderText = "Project" },
                new DataGridViewTextBoxColumn { Name = "litterDate", HeaderText = "litter Date" },
                new DataGridViewButtonColumn { Name = "Remove", HeaderText = "Remove", Text = "Remove", UseColumnTextForButtonValue = true }
            });

            multiplelittersGrid.CellContentClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == multiplelittersGrid.Columns["Remove"].Index)
                {
                    // Show loading spinner
                    loadingSpinner.Visible = true;
                    this.Refresh();

                    try
                    {
                        // Remove the row
                        multiplelittersGrid.Rows.RemoveAt(e.RowIndex);

                        // Simulate a brief delay for the operation
                        System.Threading.Thread.Sleep(300);
                    }
                    finally
                    {
                        // Hide loading spinner
                        loadingSpinner.Visible = false;
                        this.Refresh();
                    }
                }
            };

            gridGroup.Controls.Add(multiplelittersGrid);

            // Add Save All button
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(0, 10, 0, 0)
            };

            saveAllButton = new Button
            {
                Text = "Save All litters",
                Font = new Font("Segoe UI", 10),
                Width = 150,
                Height = 40,
                Location = new Point(300, 10),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            saveAllButton.FlatAppearance.BorderSize = 0;
            saveAllButton.Click += SaveAllButton_Click;
            bottomPanel.Controls.Add(saveAllButton);

            var cancelMultiButton = new Button
            {
                Text = "Cancel",
                Font = new Font("Segoe UI", 10),
                Width = 150,
                Height = 40,
                Location = new Point(460, 10),
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            cancelMultiButton.FlatAppearance.BorderSize = 1;
            cancelMultiButton.Click += CancelButton_Click;
            bottomPanel.Controls.Add(cancelMultiButton);

            mainPanel.Controls.Add(bottomPanel);
            mainPanel.Controls.Add(gridGroup);

            tab.Controls.Add(mainPanel);

            // Copy the items from the single litter tab
            multiDamComboBox.Items.Clear();
            multiSireComboBox.Items.Clear();
            multiProjectComboBox.Items.Clear();

            // We'll populate these in the LoadAnimals and LoadProjects methods
        }

        private void InitializeLoadingSpinner()
        {
            loadingSpinner = new PictureBox
            {
                Size = new Size(50, 50),
                Image = Image.FromFile("C:\\Users\\earob\\source\\repos\\RATAPP\\RATAPPLibrary\\RATAPP\\Resources\\Loading_2.gif"),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false
            };
            this.Controls.Add(loadingSpinner);
            this.Resize += (s, e) => CenterLoadingSpinner();
        }

        private void CenterLoadingSpinner()
        {
            loadingSpinner.Location = new Point(
                (ClientSize.Width - loadingSpinner.Width) / 2,
                (ClientSize.Height - loadingSpinner.Height) / 2
            );
        }

        private void LoadAnimals()
        {
            // Show loading spinner
            loadingSpinner.Visible = true;
            this.Refresh();

            try
            {
                // TODO: Load actual animals from database
                // For now, add sample data

                // Add female rats to dam combo box
                damComboBox.Items.Clear();
                damComboBox.Items.AddRange(new string[] {
                    "F001 - Daisy (Female)",
                    "F002 - Luna (Female)",
                    "F003 - Bella (Female)",
                    "F004 - Molly (Female)",
                    "F005 - Coco (Female)"
                });

                // Add male rats to sire combo box
                sireComboBox.Items.Clear();
                sireComboBox.Items.AddRange(new string[] {
                    "M001 - Max (Male)",
                    "M002 - Charlie (Male)",
                    "M003 - Buddy (Male)",
                    "M004 - Rocky (Male)",
                    "M005 - Duke (Male)"
                });

                // Also populate the multiple litters tab combo boxes
                if (tabControl.TabPages.Count > 1)
                {
                    var multiDamComboBox = tabControl.TabPages[1].Controls[0].Controls[0].Controls[0].Controls[1] as ComboBox;
                    var multiSireComboBox = tabControl.TabPages[1].Controls[0].Controls[0].Controls[0].Controls[3] as ComboBox;

                    if (multiDamComboBox != null && multiSireComboBox != null)
                    {
                        multiDamComboBox.Items.Clear();
                        multiDamComboBox.Items.AddRange(new string[] {
                            "F001 - Daisy (Female)",
                            "F002 - Luna (Female)",
                            "F003 - Bella (Female)",
                            "F004 - Molly (Female)",
                            "F005 - Coco (Female)"
                        });

                        multiSireComboBox.Items.Clear();
                        multiSireComboBox.Items.AddRange(new string[] {
                            "M001 - Max (Male)",
                            "M002 - Charlie (Male)",
                            "M003 - Buddy (Male)",
                            "M004 - Rocky (Male)",
                            "M005 - Duke (Male)"
                        });
                    }
                }

                // Simulate a brief delay for the operation
                System.Threading.Thread.Sleep(500);
            }
            finally
            {
                // Hide loading spinner
                loadingSpinner.Visible = false;
                this.Refresh();
            }
        }

        private void LoadProjects()
        {
            // Show loading spinner
            loadingSpinner.Visible = true;
            this.Refresh();

            try
            {
                // TODO: Load actual projects from database
                // For now, add sample data
                projectComboBox.Items.Clear();
                projectComboBox.Items.AddRange(new string[] {
                    "Breeding Program A",
                    "Research Project B",
                    "Color Study C",
                    "Behavior Study D",
                    "General Breeding"
                });

                // Also populate the multiple litters tab project combo box
                if (tabControl.TabPages.Count > 1)
                {
                    var multiProjectComboBox = tabControl.TabPages[1].Controls[0].Controls[0].Controls[0].Controls[7] as ComboBox;

                    if (multiProjectComboBox != null)
                    {
                        multiProjectComboBox.Items.Clear();
                        multiProjectComboBox.Items.AddRange(new string[] {
                            "Breeding Program A",
                            "Research Project B",
                            "Color Study C",
                            "Behavior Study D",
                            "General Breeding"
                        });
                    }
                }

                // Simulate a brief delay for the operation
                System.Threading.Thread.Sleep(500);
            }
            finally
            {
                // Hide loading spinner
                loadingSpinner.Visible = false;
                this.Refresh();
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            // Show loading spinner
            loadingSpinner.Visible = true;
            this.Refresh();

            try
            {
                // Validate inputs
                if (damComboBox.SelectedIndex == -1 || sireComboBox.SelectedIndex == -1 ||
                    string.IsNullOrWhiteSpace(litterIdTextBox.Text) || projectComboBox.SelectedIndex == -1)
                {
                    MessageBox.Show("Please fill in all required fields", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // TODO: Save litter to database

                // Simulate processing delay
                System.Threading.Thread.Sleep(1000);

                MessageBox.Show($"litter added successfully!\n\nDam: {damComboBox.SelectedItem}\nSire: {sireComboBox.SelectedItem}\nlitter ID: {litterIdTextBox.Text}\nProject: {projectComboBox.SelectedItem}\nlitter Date: {litterDatePicker.Value.ToShortDateString()}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear form for next entry
                litterIdTextBox.Text = "";
                damComboBox.SelectedIndex = -1;
                sireComboBox.SelectedIndex = -1;
                projectComboBox.SelectedIndex = -1;
                litterDatePicker.Value = DateTime.Today;
            }
            finally
            {
                // Hide loading spinner
                loadingSpinner.Visible = false;
                this.Refresh();
            }
        }

        private void SaveAllButton_Click(object sender, EventArgs e)
        {
            // Show loading spinner
            loadingSpinner.Visible = true;
            this.Refresh();

            try
            {
                if (multiplelittersGrid.Rows.Count == 0)
                {
                    MessageBox.Show("Please add at least one litter to the list", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // TODO: Save all litters to database

                // Simulate processing delay
                System.Threading.Thread.Sleep(1500);

                MessageBox.Show($"Successfully added {multiplelittersGrid.Rows.Count} litters!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear grid for next batch
                multiplelittersGrid.Rows.Clear();
            }
            finally
            {
                // Hide loading spinner
                loadingSpinner.Visible = false;
                this.Refresh();
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to cancel? Any unsaved data will be lost.",
                "Confirm Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close();
            }
        }
    }
}


