using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text.pdf;
using RATAPP.Forms;

namespace RATAPP.Panels
{
    public partial class FinancialPanel : ResponsivePanel
    {
        private RATAppBaseForm _parentForm;
        private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;
        private TabControl tabControl;
        private ComboBox timeFrameComboBox;
        private ComboBox yearComboBox;
        private ComboBox monthComboBox;
        private ComboBox categoryComboBox;
        private Button applyFilterButton;
        private Button exportButton;
        private PictureBox loadingSpinner;

        // Data grids for each tab
        private DataGridView profitLossGrid;
        private DataGridView budgetGrid;
        private DataGridView overviewGrid;

        public FinancialPanel(RATAppBaseForm parentForm, RATAPPLibrary.Data.DbContexts.RatAppDbContext context) : base(parentForm)
        {
            _parentForm = parentForm;
            _context = context;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // Create header panel
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(20, 10, 20, 0)
            };

            var titleLabel = new Label
            {
                Text = "Financial Management",
                Font = new Font("Segoe UI", 25, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 212),
                AutoSize = true,
                Location = new Point(0, 5)
            };
            headerPanel.Controls.Add(titleLabel);

            this.Controls.Add(headerPanel);

            // Create filter panel
            InitializeFilterControls();

            // Create tab control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };

            // Initialize tabs
            InitializeProfitLossTab();
            InitializeBudgetTab();
            InitializeOverviewTab();

            this.Controls.Add(tabControl);

            // Initialize loading spinner
            InitializeLoadingSpinner();

            // Load initial data
            LoadData();
        }

        private void InitializeFilterControls()
        {
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(20, 10, 20, 10),
            };

            // Time frame filter (Month, Quarter, Year, All)
            var timeFrameLabel = new Label
            {
                Text = "Time Frame:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(0, 20)
            };
            filterPanel.Controls.Add(timeFrameLabel);

            timeFrameComboBox = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Width = 120,
                Location = new Point(80, 17),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            timeFrameComboBox.Items.AddRange(new string[] { "Month", "Quarter", "Year", "All" });
            timeFrameComboBox.SelectedIndex = 0;
            timeFrameComboBox.SelectedIndexChanged += TimeFrameComboBox_SelectedIndexChanged;
            filterPanel.Controls.Add(timeFrameComboBox);

            // Year filter
            var yearLabel = new Label
            {
                Text = "Year:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(220, 20)
            };
            filterPanel.Controls.Add(yearLabel);

            yearComboBox = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Width = 80,
                Location = new Point(260, 17),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            // Add current year and 4 previous years
            int currentYear = DateTime.Now.Year;
            for (int i = 0; i < 5; i++)
            {
                yearComboBox.Items.Add((currentYear - i).ToString());
            }
            yearComboBox.SelectedIndex = 0;
            filterPanel.Controls.Add(yearComboBox);

            // Month filter
            var monthLabel = new Label
            {
                Text = "Month:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(360, 20)
            };
            filterPanel.Controls.Add(monthLabel);

            monthComboBox = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Width = 100,
                Location = new Point(410, 17),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            monthComboBox.Items.AddRange(new string[] {
                "January", "February", "March", "April", "May", "June",
                "July", "August", "September", "October", "November", "December"
            });
            monthComboBox.SelectedIndex = DateTime.Now.Month - 1;
            filterPanel.Controls.Add(monthComboBox);

            // Category filter
            var categoryLabel = new Label
            {
                Text = "Category:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(530, 20)
            };
            filterPanel.Controls.Add(categoryLabel);

            categoryComboBox = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Width = 150,
                Location = new Point(595, 17),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            categoryComboBox.Items.AddRange(new string[] {
                "All Categories", "Animals", "Food", "Bedding", "Cages", "Medications", "Veterinary", "Supplies", "Other"
            });
            categoryComboBox.SelectedIndex = 0;
            filterPanel.Controls.Add(categoryComboBox);

            // Apply filter button
            applyFilterButton = new Button
            {
                Text = "Apply Filters",
                Font = new Font("Segoe UI", 10),
                Width = 100,
                Height = 30,
                Location = new Point(765, 15),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            applyFilterButton.FlatAppearance.BorderSize = 0;
            applyFilterButton.Click += ApplyFilterButton_Click;
            filterPanel.Controls.Add(applyFilterButton);

            // Export button
            exportButton = new Button
            {
                Text = "Export",
                Font = new Font("Segoe UI", 10),
                Width = 80,
                Height = 30,
                Location = new Point(880, 15),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            exportButton.FlatAppearance.BorderSize = 0;
            exportButton.Click += ExportButton_Click;
            filterPanel.Controls.Add(exportButton);

            this.Controls.Add(filterPanel);
        }

        private void InitializeProfitLossTab()
        {
            var profitLossTab = new TabPage("Profit & Loss");
            // split container = splits the panel into 2 based on settings like orientation, min size of each panel, etc 
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                Width = 800,
                Height = 800,
                Panel1MinSize = 400,
                Panel2MinSize = 200
            };

            this.Controls.Add(splitContainer);

            // Set SplitterDistance after the splitContainer has been sized
            //this will fix the issue with the SplitterDistance not being "between" the mins 
            //splitContainer.SplitterDistance = 200;

            // profit & loss grid
            profitLossGrid = CreateDataGridView();

            // Add profit & loss specific columns
            profitLossGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Date" },
                new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "Type" },
                new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Category" },
                new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description" },
                new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "Amount ($)" },
                new DataGridViewButtonColumn { Name = "Edit", HeaderText = "Edit", Text = "Edit", UseColumnTextForButtonValue = true },
                new DataGridViewButtonColumn { Name = "Delete", HeaderText = "Delete", Text = "Delete", UseColumnTextForButtonValue = true }
            });

            profitLossGrid.CellContentClick += ProfitLossGrid_CellContentClick;

            // panel 1 grid 
            splitContainer.Panel1.Controls.Add(profitLossGrid);

            // make an entry form
            var entryPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var entryLabel = new Label
            {
                Text = "Add New Entry",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 212),
                Dock = DockStyle.Top,
                Height = 30
            };
            entryPanel.Controls.Add(entryLabel);

            // make an entry form 
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

            // Type i.e. Income/Expense
            formPanel.Controls.Add(new Label
            {
                Text = "Type:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            }, 0, 0);

            var typeCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 200
            };
            typeCombo.Items.AddRange(new string[] { "Income", "Expense" });
            typeCombo.SelectedIndex = 0;
            formPanel.Controls.Add(typeCombo, 1, 0);

            // Date
            formPanel.Controls.Add(new Label
            {
                Text = "Date:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            }, 0, 1);

            var datePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 200
            };
            formPanel.Controls.Add(datePicker, 1, 1);

            // Category
            formPanel.Controls.Add(new Label
            {
                Text = "Category:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            }, 0, 2);

            var entryCategoryCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 200
            };
            entryCategoryCombo.Items.AddRange(new string[] {
                "Animals", "Food", "Bedding", "Cages", "Medications", "Veterinary", "Supplies", "Other"
            });
            entryCategoryCombo.SelectedIndex = 0;
            formPanel.Controls.Add(entryCategoryCombo, 1, 2);

            // Description
            formPanel.Controls.Add(new Label
            {
                Text = "Description:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            }, 0, 3);

            var descriptionBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300
            };
            formPanel.Controls.Add(descriptionBox, 1, 3);

            // Amount
            formPanel.Controls.Add(new Label
            {
                Text = "Amount ($):",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            }, 0, 4);

            var amountBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 200
            };
            formPanel.Controls.Add(amountBox, 1, 4);

            // Add button
            var addButton = new Button
            {
                Text = "Add Entry",
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Right,
                Width = 120,
                Height = 35
            };
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += (s, e) =>
            {
                // TODO: Add entry to database
                MessageBox.Show("Entry added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Add to grid for demo purposes
                string entryType = typeCombo.SelectedItem.ToString();
                string date = datePicker.Value.ToShortDateString();
                string category = entryCategoryCombo.SelectedItem.ToString();
                string description = descriptionBox.Text;
                string amount = amountBox.Text;

                if (!string.IsNullOrWhiteSpace(description) && !string.IsNullOrWhiteSpace(amount))
                {
                    profitLossGrid.Rows.Add(date, entryType, category, description, amount);

                    // Clear form
                    descriptionBox.Text = "";
                    amountBox.Text = "";
                }
                else
                {
                    MessageBox.Show("Please fill in all fields", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            buttonPanel.Controls.Add(addButton);
            formPanel.Controls.Add(buttonPanel, 1, 5);

            entryPanel.Controls.Add(formPanel);

            // Add entry panel to panel 2
            splitContainer.Panel2.Controls.Add(entryPanel);

            // Add split container to tab
            profitLossTab.Controls.Add(splitContainer);

            // Add tab to tab control
            tabControl.TabPages.Add(profitLossTab);
        }

        private void InitializeBudgetTab()
        {
            var budgetTab = new TabPage("Budget");

            // Create a split container for the grid and entry form
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                Width = 600,
                Height = 600,
                SplitterDistance = 500,
                Panel1MinSize = 400,
                Panel2MinSize = 200
            };

            // Create budget grid
            budgetGrid = CreateDataGridView();

            // Add budget-specific columns
            budgetGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Period", HeaderText = "Period" },
                new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Category" },
                new DataGridViewTextBoxColumn { Name = "Allocated", HeaderText = "Allocated ($)" },
                new DataGridViewTextBoxColumn { Name = "Spent", HeaderText = "Spent ($)" },
                new DataGridViewTextBoxColumn { Name = "Remaining", HeaderText = "Remaining ($)" },
                new DataGridViewTextBoxColumn { Name = "PercentUsed", HeaderText = "% Used" },
                new DataGridViewButtonColumn { Name = "Edit", HeaderText = "Edit", Text = "Edit", UseColumnTextForButtonValue = true },
                new DataGridViewButtonColumn { Name = "Delete", HeaderText = "Delete", Text = "Delete", UseColumnTextForButtonValue = true }
            });

            budgetGrid.CellContentClick += BudgetGrid_CellContentClick;

            // Add grid to panel 1
            splitContainer.Panel1.Controls.Add(budgetGrid);

            // Create entry form in panel 2
            var entryPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var entryLabel = new Label
            {
                Text = "Add Budget Item",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 212),
                Dock = DockStyle.Top,
                Height = 30
            };
            entryPanel.Controls.Add(entryLabel);

            // Create entry form
            var formPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(10),
                ColumnStyles = {
                    new ColumnStyle(SizeType.Percent, 30F),
                    new ColumnStyle(SizeType.Percent, 70F)
                }
            };

            // Period
            formPanel.Controls.Add(new Label
            {
                Text = "Period:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            }, 0, 0);

            var periodCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 200
            };
            periodCombo.Items.AddRange(new string[] {
                "January 2025", "February 2025", "March 2025", "April 2025",
                "Q1 2025", "Q2 2025", "2025 Annual"
            });
            periodCombo.SelectedIndex = 0;
            formPanel.Controls.Add(periodCombo, 1, 0);

            // Category
            formPanel.Controls.Add(new Label
            {
                Text = "Category:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            }, 0, 1);

            var budgetCategoryCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 200
            };
            budgetCategoryCombo.Items.AddRange(new string[] {
                "Animals", "Food", "Bedding", "Cages", "Medications", "Veterinary", "Supplies", "Other"
            });
            budgetCategoryCombo.SelectedIndex = 0;
            formPanel.Controls.Add(budgetCategoryCombo, 1, 1);

            // Allocated Amount
            formPanel.Controls.Add(new Label
            {
                Text = "Allocated ($):",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            }, 0, 2);

            var allocatedBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 200
            };
            formPanel.Controls.Add(allocatedBox, 1, 2);

            // Notes
            formPanel.Controls.Add(new Label
            {
                Text = "Notes:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            }, 0, 3);

            var notesBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                Multiline = true,
                Height = 60
            };
            formPanel.Controls.Add(notesBox, 1, 3);

            // Add button
            var addButton = new Button
            {
                Text = "Add Budget Item",
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Right,
                Width = 120,
                Height = 35
            };
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += (s, e) =>
            {
                // TODO: Add budget item to database
                MessageBox.Show("Budget item added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Add to grid for demo purposes
                string period = periodCombo.SelectedItem.ToString();
                string category = budgetCategoryCombo.SelectedItem.ToString();
                string allocated = allocatedBox.Text;

                if (!string.IsNullOrWhiteSpace(allocated))
                {
                    // For demo, spent is 0 and remaining is the full allocated amount
                    double allocatedAmount = double.Parse(allocated);
                    string spent = "0.00";
                    string remaining = allocated;
                    string percentUsed = "0.00%";

                    budgetGrid.Rows.Add(period, category, allocated, spent, remaining, percentUsed);

                    // Clear form
                    allocatedBox.Text = "";
                    notesBox.Text = "";
                }
                else
                {
                    MessageBox.Show("Please enter an allocated amount", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            buttonPanel.Controls.Add(addButton);
            formPanel.Controls.Add(buttonPanel, 1, 4);

            entryPanel.Controls.Add(formPanel);

            // Add entry panel to panel 2
            splitContainer.Panel2.Controls.Add(entryPanel);

            // Add split container to tab
            budgetTab.Controls.Add(splitContainer);

            // Add tab to tab control
            tabControl.TabPages.Add(budgetTab);
        }

        private void InitializeOverviewTab()
        {
            var overviewTab = new TabPage("Overview");

            // Create a panel to hold all overview elements
            var overviewPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Create summary cards at the top
            var summaryPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 120,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(0, 0, 0, 20)
            };

            // Income summary card
            var incomeCard = CreateSummaryCard("Total Income", "$8,750.00", "+12.5% vs. last period", Color.FromArgb(0, 120, 212));
            summaryPanel.Controls.Add(incomeCard);

            // Expenses summary card
            var expensesCard = CreateSummaryCard("Total Expenses", "$3,250.75", "+5.2% vs. last period", Color.FromArgb(220, 53, 69));
            summaryPanel.Controls.Add(expensesCard);

            // Profit summary card
            var profitCard = CreateSummaryCard("Net Profit", "$5,499.25", "+17.8% vs. last period", Color.FromArgb(40, 167, 69));
            summaryPanel.Controls.Add(profitCard);

            // Budget summary card
            var budgetCard = CreateSummaryCard("Budget Utilization", "65.02%", "34.98% remaining", Color.FromArgb(255, 193, 7));
            summaryPanel.Controls.Add(budgetCard);

            overviewPanel.Controls.Add(summaryPanel);

            // Create overview grid
            overviewGrid = CreateDataGridView();
            overviewGrid.Dock = DockStyle.Fill;

            // Add overview-specific columns
            overviewGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Period", HeaderText = "Period" },
                new DataGridViewTextBoxColumn { Name = "Income", HeaderText = "Income ($)" },
                new DataGridViewTextBoxColumn { Name = "Expenses", HeaderText = "Expenses ($)" },
                new DataGridViewTextBoxColumn { Name = "Profit", HeaderText = "Profit ($)" },
                new DataGridViewTextBoxColumn { Name = "ProfitMargin", HeaderText = "Profit Margin (%)" },
                new DataGridViewTextBoxColumn { Name = "BudgetUtilization", HeaderText = "Budget Utilization (%)" }
            });

            overviewPanel.Controls.Add(overviewGrid);

            // Create chart placeholder
            var chartPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 200,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(245, 245, 245),
                Margin = new Padding(0, 20, 0, 0)
            };

            var chartLabel = new Label
            {
                Text = "Financial Overview Chart",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 212),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 30
            };
            chartPanel.Controls.Add(chartLabel);

            var chartPlaceholder = new Label
            {
                Text = "Chart visualization would appear here.\nShowing income, expenses, and profit trends over time.",
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill
            };
            chartPanel.Controls.Add(chartPlaceholder);

            overviewPanel.Controls.Add(chartPanel);

            // Add overview panel to tab
            overviewTab.Controls.Add(overviewPanel);

            // Add tab to tab control
            tabControl.TabPages.Add(overviewTab);
        }

        private Panel CreateSummaryCard(string title, string value, string subtitle, Color accentColor)
        {
            var card = new Panel
            {
                Margin = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Height = 100,
                Dock = DockStyle.Fill
            };

            // Add colored accent bar at the top
            var accentBar = new Panel
            {
                BackColor = accentColor,
                Height = 5,
                Dock = DockStyle.Top
            };
            card.Controls.Add(accentBar);

            // Add title
            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(10, 10),
                AutoSize = true
            };
            card.Controls.Add(titleLabel);

            // Add value
            var valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(10, 30),
                AutoSize = true
            };
            card.Controls.Add(valueLabel);

            // Add subtitle
            var subtitleLabel = new Label
            {
                Text = subtitle,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(10, 65),
                AutoSize = true
            };
            card.Controls.Add(subtitleLabel);

            return card;
        }

        private DataGridView CreateDataGridView()
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
                AllowUserToDeleteRows = false,
                Font = new Font("Segoe UI", 9),
                BackgroundColor = Color.White,
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

        private void LoadData()
        {
            // TODO: Load actual data from database
            // For now, add sample data

            // Profit & Loss data
            profitLossGrid.Rows.Clear();
            profitLossGrid.Rows.Add("3/1/2025", "Income", "Animals", "Rat - Fancy (Male)", "$225.00");
            profitLossGrid.Rows.Add("3/5/2025", "Income", "Animals", "Mouse - Fancy (Female)", "$200.00");
            profitLossGrid.Rows.Add("3/10/2025", "Income", "Animals", "Rat - Dumbo (Female)", "$200.00");
            profitLossGrid.Rows.Add("3/15/2025", "Income", "Supplies", "Cage - Large", "$240.00");
            profitLossGrid.Rows.Add("3/2/2025", "Expense", "Medications", "Antibiotics", "$150.00");
            profitLossGrid.Rows.Add("3/8/2025", "Expense", "Food", "Premium Rat Food (5kg)", "$75.50");
            profitLossGrid.Rows.Add("3/12/2025", "Expense", "Bedding", "Aspen Shavings (20L)", "$45.25");

            // Budget data
            budgetGrid.Rows.Clear();
            budgetGrid.Rows.Add("March 2025", "Animals", "$1,200.00", "$850.25", "$349.75", "70.85%");
            budgetGrid.Rows.Add("March 2025", "Food", "$1,500.00", "$1,100.50", "$399.50", "73.37%");
            budgetGrid.Rows.Add("March 2025", "Bedding", "$600.00", "$450.00", "$150.00", "75.00%");
            budgetGrid.Rows.Add("March 2025", "Cages", "$800.00", "$300.00", "$500.00", "37.50%");
            budgetGrid.Rows.Add("March 2025", "Medications", "$400.00", "$250.00", "$150.00", "62.50%");
            budgetGrid.Rows.Add("Q1 2025", "Animals", "$3,500.00", "$2,450.75", "$1,049.25", "70.02%");
            budgetGrid.Rows.Add("Q1 2025", "Food", "$4,200.00", "$3,100.50", "$1,099.50", "73.82%");

            // Overview data
            overviewGrid.Rows.Clear();
            overviewGrid.Rows.Add("January 2025", "$7,500.00", "$3,000.00", "$4,500.00", "60.00%", "60.00%");
            overviewGrid.Rows.Add("February 2025", "$8,200.00", "$3,100.00", "$5,100.00", "62.20%", "62.00%");
            overviewGrid.Rows.Add("March 2025", "$8,750.00", "$3,250.75", "$5,499.25", "62.85%", "65.02%");
            overviewGrid.Rows.Add("Q1 2025", "$24,450.00", "$9,350.75", "$15,099.25", "61.76%", "62.34%");
            overviewGrid.Rows.Add("2024 (Total)", "$85,000.00", "$35,000.00", "$50,000.00", "58.82%", "70.00%");
        }

        private void TimeFrameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Enable/disable month dropdown based on time frame selection
            string selectedTimeFrame = timeFrameComboBox.SelectedItem.ToString();
            monthComboBox.Enabled = selectedTimeFrame == "Month";
        }

        private void ProfitLossGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == profitLossGrid.Columns["Edit"].Index)
            {
                // TODO: Edit entry
                MessageBox.Show("Edit entry functionality would go here", "Edit Entry", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (e.ColumnIndex == profitLossGrid.Columns["Delete"].Index)
            {
                // TODO: Delete entry
                if (MessageBox.Show("Are you sure you want to delete this entry?", "Confirm Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    MessageBox.Show("Entry deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    profitLossGrid.Rows.RemoveAt(e.RowIndex);
                }
            }
        }

        private void BudgetGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == budgetGrid.Columns["Edit"].Index)
            {
                // TODO: Edit budget item
                MessageBox.Show("Edit budget item functionality would go here", "Edit Budget Item", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (e.ColumnIndex == budgetGrid.Columns["Delete"].Index)
            {
                // TODO: Delete budget item
                if (MessageBox.Show("Are you sure you want to delete this budget item?", "Confirm Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    MessageBox.Show("Budget item deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    budgetGrid.Rows.RemoveAt(e.RowIndex);
                }
            }
        }

        private void ApplyFilterButton_Click(object sender, EventArgs e)
        {
            try
            {
                loadingSpinner.Visible = true;
                CenterLoadingSpinner();
                this.Refresh();

                // TODO: Apply filters to data
                string timeFrame = timeFrameComboBox.SelectedItem.ToString();
                string year = yearComboBox.SelectedItem.ToString();
                string month = monthComboBox.SelectedItem.ToString();
                string category = categoryComboBox.SelectedItem.ToString();

                // Simulate loading delay
                System.Threading.Thread.Sleep(1000);

                // Reload data with filters
                LoadData();

                loadingSpinner.Visible = false;
                this.Refresh();

                MessageBox.Show($"Filters applied: {timeFrame}, {year}, {month}, {category}", "Filters Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                loadingSpinner.Visible = false;
                this.Refresh();
            }
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            // TODO: Implement export functionality
            MessageBox.Show("Export functionality would go here.\nThis would export the current view to Excel or CSV.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public async Task RefreshDataAsync()
        {
            try
            {
                loadingSpinner.Visible = true;
                CenterLoadingSpinner();
                this.Refresh();

                // TODO: Implement actual data refresh from database
                await Task.Delay(1000); // Simulated delay

                LoadData();

                loadingSpinner.Visible = false;
                this.Refresh();

                MessageBox.Show("Financial data refreshed successfully", "Refresh Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                loadingSpinner.Visible = false;
                this.Refresh();
            }
        }
    }
}
//using System;
//using System.Drawing;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using RATAPP.Forms;

//namespace RATAPP.Panels
//{
//    public partial class FinancialPanel : Panel, INavigable
//    {
//        private RATAppBaseForm _parentForm;
//        private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;
//        private TabControl tabControl;
//        private ComboBox timeFrameComboBox;
//        private ComboBox yearComboBox;
//        private ComboBox monthComboBox;
//        private ComboBox categoryComboBox;
//        private Button applyFilterButton;
//        private Button exportButton;
//        private PictureBox loadingSpinner;

//        // Data grids for each tab
//        private DataGridView budgetGrid;
//        private DataGridView profitGrid;
//        private DataGridView lossGrid;
//        private DataGridView trendsGrid;

//        public FinancialPanel(RATAppBaseForm parentForm, RATAPPLibrary.Data.DbContexts.RatAppDbContext context)
//        {
//            _parentForm = parentForm;
//            _context = context;
//            InitializeComponents();
//        }

//        private void InitializeComponents()
//        {
//            this.Dock = DockStyle.Fill;
//            this.BackColor = Color.White;

//            // Create header panel
//            var headerPanel = new Panel
//            {
//                Dock = DockStyle.Top,
//                Height = 60,
//                Padding = new Padding(20, 10, 20, 0)
//            };

//            var titleLabel = new Label
//            {
//                Text = "Financial Management",
//                Font = new Font("Segoe UI", 25, FontStyle.Bold),
//                ForeColor = Color.FromArgb(0, 120, 212),
//                AutoSize = true,
//                Location = new Point(0, 5)
//            };
//            headerPanel.Controls.Add(titleLabel);

//            this.Controls.Add(headerPanel);

//            // Create filter panel
//            InitializeFilterControls();

//            // Create tab control
//            tabControl = new TabControl
//            {
//                Dock = DockStyle.Fill,
//                Font = new Font("Segoe UI", 10)
//            };

//            // Initialize tabs
//            InitializeBudgetTab();
//            InitializeProfitTab();
//            InitializeLossTab();
//            InitializeTrendsTab();

//            this.Controls.Add(tabControl);

//            // Initialize loading spinner
//            InitializeLoadingSpinner();

//            // Load initial data
//            LoadData();
//        }

//        private void InitializeFilterControls()
//        {
//            var filterPanel = new Panel
//            {
//                Dock = DockStyle.Top,
//                Height = 60,
//                Padding = new Padding(20, 10, 20, 10),
//            };

//            // Time frame filter (Month, Quarter, Year, All)
//            var timeFrameLabel = new Label
//            {
//                Text = "Time Frame:",
//                Font = new Font("Segoe UI", 10),
//                AutoSize = true,
//                Location = new Point(0, 20)
//            };
//            filterPanel.Controls.Add(timeFrameLabel);

//            timeFrameComboBox = new ComboBox
//            {
//                Font = new Font("Segoe UI", 10),
//                Width = 120,
//                Location = new Point(80, 17),
//                DropDownStyle = ComboBoxStyle.DropDownList
//            };
//            timeFrameComboBox.Items.AddRange(new string[] { "Month", "Quarter", "Year", "All" });
//            timeFrameComboBox.SelectedIndex = 0;
//            timeFrameComboBox.SelectedIndexChanged += TimeFrameComboBox_SelectedIndexChanged;
//            filterPanel.Controls.Add(timeFrameComboBox);

//            // Year filter
//            var yearLabel = new Label
//            {
//                Text = "Year:",
//                Font = new Font("Segoe UI", 10),
//                AutoSize = true,
//                Location = new Point(220, 20)
//            };
//            filterPanel.Controls.Add(yearLabel);

//            yearComboBox = new ComboBox
//            {
//                Font = new Font("Segoe UI", 10),
//                Width = 80,
//                Location = new Point(260, 17),
//                DropDownStyle = ComboBoxStyle.DropDownList
//            };
//            // Add current year and 4 previous years
//            int currentYear = DateTime.Now.Year;
//            for (int i = 0; i < 5; i++)
//            {
//                yearComboBox.Items.Add((currentYear - i).ToString());
//            }
//            yearComboBox.SelectedIndex = 0;
//            filterPanel.Controls.Add(yearComboBox);

//            // Month filter
//            var monthLabel = new Label
//            {
//                Text = "Month:",
//                Font = new Font("Segoe UI", 10),
//                AutoSize = true,
//                Location = new Point(360, 20)
//            };
//            filterPanel.Controls.Add(monthLabel);

//            monthComboBox = new ComboBox
//            {
//                Font = new Font("Segoe UI", 10),
//                Width = 100,
//                Location = new Point(410, 17),
//                DropDownStyle = ComboBoxStyle.DropDownList
//            };
//            monthComboBox.Items.AddRange(new string[] {
//                "January", "February", "March", "April", "May", "June",
//                "July", "August", "September", "October", "November", "December"
//            });
//            monthComboBox.SelectedIndex = DateTime.Now.Month - 1;
//            filterPanel.Controls.Add(monthComboBox);

//            // Category filter
//            var categoryLabel = new Label
//            {
//                Text = "Category:",
//                Font = new Font("Segoe UI", 10),
//                AutoSize = true,
//                Location = new Point(530, 20)
//            };
//            filterPanel.Controls.Add(categoryLabel);

//            categoryComboBox = new ComboBox
//            {
//                Font = new Font("Segoe UI", 10),
//                Width = 150,
//                Location = new Point(595, 17),
//                DropDownStyle = ComboBoxStyle.DropDownList
//            };
//            categoryComboBox.Items.AddRange(new string[] {
//                "All Categories", "Animals", "Food", "Bedding", "Cages", "Medications", "Veterinary", "Supplies", "Other"
//            });
//            categoryComboBox.SelectedIndex = 0;
//            filterPanel.Controls.Add(categoryComboBox);

//            // Apply filter button
//            applyFilterButton = new Button
//            {
//                Text = "Apply Filters",
//                Font = new Font("Segoe UI", 10),
//                Width = 100,
//                Height = 30,
//                Location = new Point(765, 15),
//                BackColor = Color.FromArgb(0, 120, 212),
//                ForeColor = Color.White,
//                FlatStyle = FlatStyle.Flat
//            };
//            applyFilterButton.FlatAppearance.BorderSize = 0;
//            applyFilterButton.Click += ApplyFilterButton_Click;
//            filterPanel.Controls.Add(applyFilterButton);

//            // Export button
//            exportButton = new Button
//            {
//                Text = "Export",
//                Font = new Font("Segoe UI", 10),
//                Width = 80,
//                Height = 30,
//                Location = new Point(880, 15),
//                BackColor = Color.FromArgb(0, 120, 212),
//                ForeColor = Color.White,
//                FlatStyle = FlatStyle.Flat
//            };
//            exportButton.FlatAppearance.BorderSize = 0;
//            exportButton.Click += ExportButton_Click;
//            filterPanel.Controls.Add(exportButton);

//            this.Controls.Add(filterPanel);
//        }

//        private void InitializeBudgetTab()
//        {
//            var budgetTab = new TabPage("Budget");

//            // Create budget grid
//            budgetGrid = CreateDataGridView();

//            // Add budget-specific columns
//            budgetGrid.Columns.AddRange(new DataGridViewColumn[]
//            {
//                new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Category" },
//                new DataGridViewTextBoxColumn { Name = "Allocated", HeaderText = "Allocated ($)" },
//                new DataGridViewTextBoxColumn { Name = "Spent", HeaderText = "Spent ($)" },
//                new DataGridViewTextBoxColumn { Name = "Remaining", HeaderText = "Remaining ($)" },
//                new DataGridViewTextBoxColumn { Name = "PercentUsed", HeaderText = "% Used" }
//            });

//            // Add summary panel at the bottom
//            var summaryPanel = CreateSummaryPanel("Budget Summary", new string[] {
//                "Total Budget: $5,000.00",
//                "Total Spent: $3,250.75",
//                "Remaining: $1,749.25",
//                "Budget Utilization: 65.02%"
//            });

//            // Create a container panel
//            var containerPanel = new Panel
//            {
//                Dock = DockStyle.Fill,
//                Padding = new Padding(10)
//            };

//            // Add controls to the container
//            containerPanel.Controls.Add(budgetGrid);
//            containerPanel.Controls.Add(summaryPanel);

//            // Add container to tab
//            budgetTab.Controls.Add(containerPanel);

//            // Add tab to tab control
//            tabControl.TabPages.Add(budgetTab);
//        }

//        private void InitializeProfitTab()
//        {
//            var profitTab = new TabPage("Profit");

//            // Create profit grid
//            profitGrid = CreateDataGridView();

//            // Add profit-specific columns
//            profitGrid.Columns.AddRange(new DataGridViewColumn[]
//            {
//                new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Date" },
//                new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Category" },
//                new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description" },
//                new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Quantity" },
//                new DataGridViewTextBoxColumn { Name = "UnitPrice", HeaderText = "Unit Price ($)" },
//                new DataGridViewTextBoxColumn { Name = "TotalAmount", HeaderText = "Total Amount ($)" }
//            });

//            // Add summary panel at the bottom
//            var summaryPanel = CreateSummaryPanel("Profit Summary", new string[] {
//                "Total Revenue: $8,750.00",
//                "Total Cost: $3,250.75",
//                "Net Profit: $5,499.25",
//                "Profit Margin: 62.85%"
//            });

//            // Create a container panel
//            var containerPanel = new Panel
//            {
//                Dock = DockStyle.Fill,
//                Padding = new Padding(10)
//            };

//            // Add controls to the container
//            containerPanel.Controls.Add(profitGrid);
//            containerPanel.Controls.Add(summaryPanel);

//            // Add container to tab
//            profitTab.Controls.Add(containerPanel);

//            // Add tab to tab control
//            tabControl.TabPages.Add(profitTab);
//        }

//        private void InitializeLossTab()
//        {
//            var lossTab = new TabPage("Loss");

//            // Create loss grid
//            lossGrid = CreateDataGridView();

//            // Add loss-specific columns
//            lossGrid.Columns.AddRange(new DataGridViewColumn[]
//            {
//                new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Date" },
//                new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Category" },
//                new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description" },
//                new DataGridViewTextBoxColumn { Name = "Reason", HeaderText = "Reason" },
//                new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "Amount ($)" }
//            });

//            // Add summary panel at the bottom
//            var summaryPanel = CreateSummaryPanel("Loss Summary", new string[] {
//                "Total Losses: $1,250.50",
//                "Biggest Loss Category: Medications",
//                "Loss as % of Revenue: 14.29%",
//                "Loss Trend: Decreasing"
//            });

//            // Create a container panel
//            var containerPanel = new Panel
//            {
//                Dock = DockStyle.Fill,
//                Padding = new Padding(10)
//            };

//            // Add controls to the container
//            containerPanel.Controls.Add(lossGrid);
//            containerPanel.Controls.Add(summaryPanel);

//            // Add container to tab
//            lossTab.Controls.Add(containerPanel);

//            // Add tab to tab control
//            tabControl.TabPages.Add(lossTab);
//        }

//        private void InitializeTrendsTab()
//        {
//            var trendsTab = new TabPage("Trends");

//            // Create trends grid
//            trendsGrid = CreateDataGridView();

//            // Add trends-specific columns
//            trendsGrid.Columns.AddRange(new DataGridViewColumn[]
//            {
//                new DataGridViewTextBoxColumn { Name = "Period", HeaderText = "Period" },
//                new DataGridViewTextBoxColumn { Name = "Revenue", HeaderText = "Revenue ($)" },
//                new DataGridViewTextBoxColumn { Name = "Expenses", HeaderText = "Expenses ($)" },
//                new DataGridViewTextBoxColumn { Name = "Profit", HeaderText = "Profit ($)" },
//                new DataGridViewTextBoxColumn { Name = "GrowthRate", HeaderText = "Growth Rate (%)" }
//            });

//            // Add chart panel
//            var chartPanel = new Panel
//            {
//                Dock = DockStyle.Bottom,
//                Height = 200,
//                BorderStyle = BorderStyle.FixedSingle,
//                BackColor = Color.FromArgb(245, 245, 245)
//            };

//            var chartLabel = new Label
//            {
//                Text = "Financial Trends Chart (Placeholder)",
//                Font = new Font("Segoe UI", 12, FontStyle.Bold),
//                ForeColor = Color.FromArgb(0, 120, 212),
//                TextAlign = ContentAlignment.MiddleCenter,
//                Dock = DockStyle.Top,
//                Height = 30
//            };
//            chartPanel.Controls.Add(chartLabel);

//            var chartPlaceholder = new Label
//            {
//                Text = "Chart visualization would appear here.\nShowing revenue, expenses, and profit trends over time.",
//                Font = new Font("Segoe UI", 10),
//                TextAlign = ContentAlignment.MiddleCenter,
//                Dock = DockStyle.Fill
//            };
//            chartPanel.Controls.Add(chartPlaceholder);

//            // Create a container panel
//            var containerPanel = new Panel
//            {
//                Dock = DockStyle.Fill,
//                Padding = new Padding(10)
//            };

//            // Add controls to the container
//            containerPanel.Controls.Add(trendsGrid);
//            containerPanel.Controls.Add(chartPanel);

//            // Add container to tab
//            trendsTab.Controls.Add(containerPanel);

//            // Add tab to tab control
//            tabControl.TabPages.Add(trendsTab);
//        }

//        private DataGridView CreateDataGridView()
//        {
//            var grid = new DataGridView
//            {
//                Dock = DockStyle.Fill,
//                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
//                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
//                RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
//                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders,
//                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
//                AllowUserToAddRows = false,
//                AllowUserToDeleteRows = false,
//                ReadOnly = true,
//                Font = new Font("Segoe UI", 9),
//                BackgroundColor = Color.White,
//                BorderStyle = BorderStyle.None,
//                RowHeadersVisible = false,
//                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
//                GridColor = Color.FromArgb(230, 230, 230),
//                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
//                {
//                    BackColor = Color.FromArgb(247, 247, 247)
//                }
//            };

//            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
//            {
//                BackColor = Color.FromArgb(0, 120, 212),
//                ForeColor = Color.White,
//                Font = new Font("Segoe UI", 10, FontStyle.Bold)
//            };

//            return grid;
//        }

//        private Panel CreateSummaryPanel(string title, string[] summaryItems)
//        {
//            var panel = new Panel
//            {
//                Dock = DockStyle.Bottom,
//                Height = 100,
//                BorderStyle = BorderStyle.FixedSingle,
//                BackColor = Color.FromArgb(245, 245, 245)
//            };

//            var titleLabel = new Label
//            {
//                Text = title,
//                Font = new Font("Segoe UI", 12, FontStyle.Bold),
//                ForeColor = Color.FromArgb(0, 120, 212),
//                Location = new Point(10, 10),
//                AutoSize = true
//            };
//            panel.Controls.Add(titleLabel);

//            // Add summary items
//            int y = 40;
//            foreach (var item in summaryItems)
//            {
//                var itemLabel = new Label
//                {
//                    Text = item,
//                    Font = new Font("Segoe UI", 10),
//                    Location = new Point(20, y),
//                    AutoSize = true
//                };
//                panel.Controls.Add(itemLabel);
//                y += 20;
//            }

//            return panel;
//        }

//        private void InitializeLoadingSpinner()
//        {
//            loadingSpinner = new PictureBox
//            {
//                Size = new Size(50, 50),
//                Image = Image.FromFile("C:\\Users\\earob\\source\\repos\\RATAPP\\RATAPPLibrary\\RATAPP\\Resources\\Loading_2.gif"),
//                SizeMode = PictureBoxSizeMode.StretchImage,
//                Visible = false
//            };
//            this.Controls.Add(loadingSpinner);
//            this.Resize += (s, e) => CenterLoadingSpinner();
//        }

//        private void CenterLoadingSpinner()
//        {
//            loadingSpinner.Location = new Point(
//                (ClientSize.Width - loadingSpinner.Width) / 2,
//                (ClientSize.Height - loadingSpinner.Height) / 2
//            );
//        }

//        private void LoadData()
//        {
//            // TODO: Load actual data from database
//            // For now, add sample data

//            // Budget data
//            budgetGrid.Rows.Clear();
//            budgetGrid.Rows.Add("Animals", "$1,200.00", "$850.25", "$349.75", "70.85%");
//            budgetGrid.Rows.Add("Food", "$1,500.00", "$1,100.50", "$399.50", "73.37%");
//            budgetGrid.Rows.Add("Bedding", "$600.00", "$450.00", "$150.00", "75.00%");
//            budgetGrid.Rows.Add("Cages", "$800.00", "$300.00", "$500.00", "37.50%");
//            budgetGrid.Rows.Add("Medications", "$400.00", "$250.00", "$150.00", "62.50%");
//            budgetGrid.Rows.Add("Veterinary", "$300.00", "$200.00", "$100.00", "66.67%");
//            budgetGrid.Rows.Add("Supplies", "$200.00", "$100.00", "$100.00", "50.00%");

//            // Profit data
//            profitGrid.Rows.Clear();
//            profitGrid.Rows.Add("3/1/2025", "Animals", "Rat - Fancy (Male)", 5, "$45.00", "$225.00");
//            profitGrid.Rows.Add("3/5/2025", "Animals", "Mouse - Fancy (Female)", 8, "$25.00", "$200.00");
//            profitGrid.Rows.Add("3/10/2025", "Animals", "Rat - Dumbo (Female)", 4, "$50.00", "$200.00");
//            profitGrid.Rows.Add("3/15/2025", "Supplies", "Cage - Large", 2, "$120.00", "$240.00");
//            profitGrid.Rows.Add("3/20/2025", "Food", "Premium Rat Food (5kg)", 10, "$30.00", "$300.00");
//            profitGrid.Rows.Add("3/25/2025", "Animals", "Rat - Rex (Male)", 3, "$55.00", "$165.00");

//            // Loss data
//            lossGrid.Rows.Clear();
//            lossGrid.Rows.Add("3/2/2025", "Medications", "Antibiotics - Expired", "Inventory Management", "$150.00");
//            lossGrid.Rows.Add("3/8/2025", "Food", "Damaged Food Packages", "Shipping Damage", "$75.50");
//            lossGrid.Rows.Add("3/12/2025", "Animals", "Lost Sale - Customer Cancellation", "Customer Service", "$225.00");
//            lossGrid.Rows.Add("3/18/2025", "Supplies", "Broken Cages", "Handling Error", "$200.00");
//            lossGrid.Rows.Add("3/22/2025", "Bedding", "Mold Contamination", "Storage Issue", "$100.00");

//            // Trends data
//            trendsGrid.Rows.Clear();
//            trendsGrid.Rows.Add("January 2025", "$7,500.00", "$3,000.00", "$4,500.00", "");
//            trendsGrid.Rows.Add("February 2025", "$8,200.00", "$3,100.00", "$5,100.00", "13.33%");
//            trendsGrid.Rows.Add("March 2025", "$8,750.00", "$3,250.75", "$5,499.25", "7.83%");
//            trendsGrid.Rows.Add("Q1 2025", "$24,450.00", "$9,350.75", "$15,099.25", "");
//            trendsGrid.Rows.Add("2024 (Total)", "$85,000.00", "$35,000.00", "$50,000.00", "");
//        }

//        private void TimeFrameComboBox_SelectedIndexChanged(object sender, EventArgs e)
//        {
//            // Enable/disable month dropdown based on time frame selection
//            string selectedTimeFrame = timeFrameComboBox.SelectedItem.ToString();
//            monthComboBox.Enabled = selectedTimeFrame == "Month";
//        }

//        private void ApplyFilterButton_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                loadingSpinner.Visible = true;
//                CenterLoadingSpinner();
//                this.Refresh();

//                // TODO: Apply filters to data
//                string timeFrame = timeFrameComboBox.SelectedItem.ToString();
//                string year = yearComboBox.SelectedItem.ToString();
//                string month = monthComboBox.SelectedItem.ToString();
//                string category = categoryComboBox.SelectedItem.ToString();

//                // Simulate loading delay
//                System.Threading.Thread.Sleep(1000);

//                // Reload data with filters
//                LoadData();

//                loadingSpinner.Visible = false;
//                this.Refresh();

//                MessageBox.Show($"Filters applied: {timeFrame}, {year}, {month}, {category}", "Filters Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//            finally
//            {
//                loadingSpinner.Visible = false;
//                this.Refresh();
//            }
//        }

//        private void ExportButton_Click(object sender, EventArgs e)
//        {
//            // TODO: Implement export functionality
//            MessageBox.Show("Export functionality would go here.\nThis would export the current view to Excel or CSV.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
//        }

//        public async Task RefreshDataAsync()
//        {
//            try
//            {
//                loadingSpinner.Visible = true;
//                CenterLoadingSpinner();
//                this.Refresh();

//                // TODO: Implement actual data refresh from database
//                await Task.Delay(1000); // Simulated delay

//                LoadData();

//                loadingSpinner.Visible = false;
//                this.Refresh();

//                MessageBox.Show("Financial data refreshed successfully", "Refresh Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//            finally
//            {
//                loadingSpinner.Visible = false;
//                this.Refresh();
//            }
//        }
//    }
//}