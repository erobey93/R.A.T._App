using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RATAPP.Forms;

namespace RATAPP.Forms
{
    public partial class CalendarForm : Form
    {
        private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;
        private MonthCalendar calendar;
        private DataGridView alertsGrid;
        private Panel addAlertPanel;
        private Panel upcomingEventsPanel;
        private TabControl tabControl;

        public CalendarForm(RATAPPLibrary.Data.DbContexts.RatAppDbContext context)
        {
            _context = context;
            InitializeComponents();
            LoadAlerts();
            LoadUpcomingEvents();
        }

        private void InitializeComponents()
        {
            this.Text = "RAT APP Calendar";
            this.Size = new Size(1000, 700);
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
                Text = "Calendar & Alerts",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 12)
            };
            headerPanel.Controls.Add(headerLabel);

            this.Controls.Add(headerPanel);

            // Create tab control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };

            // Calendar Tab
            var calendarTab = new TabPage("Calendar View");
            InitializeCalendarTab(calendarTab);
            tabControl.TabPages.Add(calendarTab);

            // Alerts Tab
            var alertsTab = new TabPage("Manage Alerts");
            InitializeAlertsTab(alertsTab);
            tabControl.TabPages.Add(alertsTab);

            // Upcoming Events Tab
            var eventsTab = new TabPage("Upcoming Events");
            InitializeEventsTab(eventsTab);
            tabControl.TabPages.Add(eventsTab);

            this.Controls.Add(tabControl);
        }

        private void InitializeCalendarTab(TabPage tab)
        {
            // Split panel for calendar and alerts
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 600
            };

            // Calendar panel (left side)
            calendar = new MonthCalendar
            {
                CalendarDimensions = new Size(2, 2),
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 20),
                MaxSelectionCount = 1,
                ShowTodayCircle = true,
                ShowWeekNumbers = false
            };
            calendar.DateSelected += Calendar_DateSelected;

            var calendarPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };
            calendarPanel.Controls.Add(calendar);

            // Today's alerts panel (right side)
            var todayAlertsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var todayLabel = new Label
            {
                Text = "Alerts for Selected Date",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 212),
                Dock = DockStyle.Top,
                Height = 30
            };
            todayAlertsPanel.Controls.Add(todayLabel);

            var todayAlertsList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                Font = new Font("Segoe UI", 9)
            };
            todayAlertsList.Columns.Add("Time", 80);
            todayAlertsList.Columns.Add("Description", 300);
            todayAlertsList.Columns.Add("Type", 100);

            // Add sample data
            var item1 = new ListViewItem(new string[] { "9:00 AM", "Health check for Whiskers", "Health" });
            var item2 = new ListViewItem(new string[] { "2:00 PM", "Medication for Mickey", "Medication" });
            var item3 = new ListViewItem(new string[] { "5:00 PM", "Litter #1234 weaning", "Age Event" });

            todayAlertsList.Items.Add(item1);
            todayAlertsList.Items.Add(item2);
            todayAlertsList.Items.Add(item3);

            todayAlertsPanel.Controls.Add(todayAlertsList);

            // Add quick alert button
            var addQuickAlertButton = new Button
            {
                Text = "Add Alert for Selected Date",
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            addQuickAlertButton.FlatAppearance.BorderSize = 0;
            addQuickAlertButton.Click += AddQuickAlert_Click;
            todayAlertsPanel.Controls.Add(addQuickAlertButton);

            splitContainer.Panel1.Controls.Add(calendarPanel);
            splitContainer.Panel2.Controls.Add(todayAlertsPanel);

            tab.Controls.Add(splitContainer);
        }

        private void InitializeAlertsTab(TabPage tab)
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Split panel for alerts list and add alert form
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 400
            };

            // Alerts grid (top)
            alertsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true
            };

            alertsGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Date" },
                new DataGridViewTextBoxColumn { Name = "Time", HeaderText = "Time" },
                new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description" },
                new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "Type" },
                new DataGridViewTextBoxColumn { Name = "AnimalID", HeaderText = "Animal ID" },
                new DataGridViewButtonColumn { Name = "Edit", HeaderText = "Edit", Text = "Edit", UseColumnTextForButtonValue = true },
                new DataGridViewButtonColumn { Name = "Delete", HeaderText = "Delete", Text = "Delete", UseColumnTextForButtonValue = true }
            });

            alertsGrid.CellContentClick += AlertsGrid_CellContentClick;

            var alertsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 0, 10)
            };

            var alertsLabel = new Label
            {
                Text = "All Alerts",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 212),
                Dock = DockStyle.Top,
                Height = 30
            };
            alertsPanel.Controls.Add(alertsLabel);
            alertsPanel.Controls.Add(alertsGrid);

            // Add alert form (bottom)
            addAlertPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 10, 0, 0)
            };

            var addAlertLabel = new Label
            {
                Text = "Add New Alert",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 212),
                Dock = DockStyle.Top,
                Height = 30
            };
            addAlertPanel.Controls.Add(addAlertLabel);

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

            // Date picker
            formPanel.Controls.Add(new Label
            {
                Text = "Date:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            }, 0, 0);

            var datePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 200
            };
            formPanel.Controls.Add(datePicker, 1, 0);

            // Time picker
            formPanel.Controls.Add(new Label
            {
                Text = "Time:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            }, 0, 1);

            var timePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 200
            };
            formPanel.Controls.Add(timePicker, 1, 1);

            // Description
            formPanel.Controls.Add(new Label
            {
                Text = "Description:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            }, 0, 2);

            var descriptionBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300
            };
            formPanel.Controls.Add(descriptionBox, 1, 2);

            // Alert Type
            formPanel.Controls.Add(new Label
            {
                Text = "Alert Type:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            }, 0, 3);

            var typeCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 200
            };
            typeCombo.Items.AddRange(new string[] { "Health", "Medication", "Feeding", "Cleaning", "Breeding", "Other" });
            typeCombo.SelectedIndex = 0;
            formPanel.Controls.Add(typeCombo, 1, 3);

            // Animal ID (optional)
            formPanel.Controls.Add(new Label
            {
                Text = "Animal ID (optional):",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            }, 0, 4);

            var animalIdBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 200
            };
            formPanel.Controls.Add(animalIdBox, 1, 4);

            // Add button
            var addButton = new Button
            {
                Text = "Add Alert",
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Right,
                Width = 120,
                Height = 35
            };
            addButton.FlatAppearance.BorderSize = 0;
            addButton.Click += AddAlert_Click;

            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill
            };
            buttonPanel.Controls.Add(addButton);
            formPanel.Controls.Add(buttonPanel, 1, 5);

            addAlertPanel.Controls.Add(formPanel);

            splitContainer.Panel1.Controls.Add(alertsPanel);
            splitContainer.Panel2.Controls.Add(addAlertPanel);

            mainPanel.Controls.Add(splitContainer);
            tab.Controls.Add(mainPanel);
        }

        private void InitializeEventsTab(TabPage tab)
        {
            upcomingEventsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            var eventsLabel = new Label
            {
                Text = "Upcoming Age-Based Events",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 212),
                Dock = DockStyle.Top,
                Height = 30
            };
            upcomingEventsPanel.Controls.Add(eventsLabel);

            var eventsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true
            };

            eventsGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Date" },
                new DataGridViewTextBoxColumn { Name = "AnimalID", HeaderText = "Animal/Litter ID" },
                new DataGridViewTextBoxColumn { Name = "Event", HeaderText = "Event" },
                new DataGridViewTextBoxColumn { Name = "Age", HeaderText = "Age" },
                new DataGridViewButtonColumn { Name = "Details", HeaderText = "Details", Text = "View", UseColumnTextForButtonValue = true },
                new DataGridViewButtonColumn { Name = "AddAlert", HeaderText = "Add Alert", Text = "Add Alert", UseColumnTextForButtonValue = true }
            });

            eventsGrid.CellContentClick += EventsGrid_CellContentClick;

            // Add filter controls
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Padding = new Padding(0, 0, 0, 10)
            };

            var filterLabel = new Label
            {
                Text = "Filter by:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(0, 15),
                AutoSize = true
            };
            filterPanel.Controls.Add(filterLabel);

            var filterCombo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Location = new Point(70, 12),
                Width = 150
            };
            filterCombo.Items.AddRange(new string[] { "All Events", "Weaning (4 weeks)", "Ready for Adoption (8 weeks)", "Breeding Age" });
            filterCombo.SelectedIndex = 0;
            filterCombo.SelectedIndexChanged += FilterCombo_SelectedIndexChanged;
            filterPanel.Controls.Add(filterCombo);

            var refreshButton = new Button
            {
                Text = "Refresh",
                Font = new Font("Segoe UI", 10),
                Location = new Point(240, 10),
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Click += RefreshEvents_Click;
            filterPanel.Controls.Add(refreshButton);

            upcomingEventsPanel.Controls.Add(filterPanel);
            upcomingEventsPanel.Controls.Add(eventsGrid);

            tab.Controls.Add(upcomingEventsPanel);
        }

        private void LoadAlerts()
        {
            // TODO: Load alerts from database
            // For now, add sample data
            alertsGrid.Rows.Clear();

            alertsGrid.Rows.Add("3/16/2025", "9:00 AM", "Health check for Whiskers", "Health", "1001");
            alertsGrid.Rows.Add("3/16/2025", "2:00 PM", "Medication for Mickey", "Medication", "1002");
            alertsGrid.Rows.Add("3/17/2025", "10:00 AM", "Cage cleaning", "Cleaning", "");
            alertsGrid.Rows.Add("3/18/2025", "3:00 PM", "Vet appointment", "Health", "1003");
            alertsGrid.Rows.Add("3/20/2025", "9:00 AM", "Food delivery", "Feeding", "");
        }

        private void LoadUpcomingEvents()
        {
            // TODO: Calculate and load upcoming age-based events from database
            // For now, add sample data
            var eventsGrid = (DataGridView)upcomingEventsPanel.Controls[upcomingEventsPanel.Controls.Count - 1];
            eventsGrid.Rows.Clear();

            eventsGrid.Rows.Add("3/20/2025", "L1001", "Weaning", "4 weeks");
            eventsGrid.Rows.Add("3/25/2025", "L1002", "Ready for Adoption", "8 weeks");
            eventsGrid.Rows.Add("4/01/2025", "1005", "Breeding Age", "12 weeks");
            eventsGrid.Rows.Add("4/10/2025", "L1003", "Weaning", "4 weeks");
            eventsGrid.Rows.Add("4/15/2025", "L1004", "Ready for Adoption", "8 weeks");
        }

        private void Calendar_DateSelected(object sender, DateRangeEventArgs e)
        {
            // TODO: Load alerts for the selected date
            MessageBox.Show($"Loading alerts for {e.Start.ToShortDateString()}", "Date Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void AddQuickAlert_Click(object sender, EventArgs e)
        {
            // TODO: Show quick add alert dialog for the selected date
            string selectedDate = calendar.SelectionStart.ToShortDateString();
            MessageBox.Show($"Add alert for {selectedDate}", "Quick Add Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Switch to the Alerts tab
            tabControl.SelectedIndex = 1;
        }

        private void AddAlert_Click(object sender, EventArgs e)
        {
            // TODO: Add alert to database
            MessageBox.Show("Alert added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadAlerts();
        }

        private void AlertsGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == alertsGrid.Columns["Edit"].Index)
            {
                // TODO: Edit alert
                MessageBox.Show("Edit alert functionality would go here", "Edit Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (e.ColumnIndex == alertsGrid.Columns["Delete"].Index)
            {
                // TODO: Delete alert
                if (MessageBox.Show("Are you sure you want to delete this alert?", "Confirm Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    MessageBox.Show("Alert deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAlerts();
                }
            }
        }

        private void EventsGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var grid = (DataGridView)upcomingEventsPanel.Controls[upcomingEventsPanel.Controls.Count - 1];

            if (e.ColumnIndex == grid.Columns["Details"].Index)
            {
                // TODO: View animal/litter details
                string id = grid.Rows[e.RowIndex].Cells["AnimalID"].Value.ToString();
                MessageBox.Show($"View details for {id}", "View Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (e.ColumnIndex == grid.Columns["AddAlert"].Index)
            {
                // TODO: Add alert for this event
                string id = grid.Rows[e.RowIndex].Cells["AnimalID"].Value.ToString();
                string eventType = grid.Rows[e.RowIndex].Cells["Event"].Value.ToString();
                string date = grid.Rows[e.RowIndex].Cells["Date"].Value.ToString();

                MessageBox.Show($"Add alert for {eventType} of {id} on {date}", "Add Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Switch to the Alerts tab
                tabControl.SelectedIndex = 1;
            }
        }

        private void FilterCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO: Filter events based on selection
            string filter = ((ComboBox)sender).SelectedItem.ToString();
            MessageBox.Show($"Filtering events by: {filter}", "Filter Events", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadUpcomingEvents();
        }

        private void RefreshEvents_Click(object sender, EventArgs e)
        {
            // TODO: Refresh events from database
            MessageBox.Show("Refreshing events...", "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadUpcomingEvents();
        }
    }
}
