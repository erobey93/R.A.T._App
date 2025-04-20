using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using RATAPP.Forms;
using RATAPPLibrary.Data;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;

namespace RATAPP.Panels
{
    public partial class AdopterManagementPanel : Panel, INavigable
    {
        private RatAppDbContext _context;
        private string _currentUsername;
        private string _userRole;
        private RATAppBaseForm _baseForm;

        // UI Components
        private TabControl tabControl;
        private DataGridView waitlistGrid;
        private DataGridView reservationsGrid;
        private DataGridView pastAdoptersGrid;
        private TextBox searchBox;
        private ComboBox filterComboBox;
        private Button refreshButton;
        private Button addButton;
        private Button editButton;
        private Button deleteButton;
        private RichTextBox notesTextBox;
        private Label notesLabel;

        public static async Task<AdopterManagementPanel> CreateAsync(RATAppBaseForm baseForm, RatAppDbContext context)
        {
            var panel = new AdopterManagementPanel(baseForm, context);
            await panel.LoadDataAsync();
            return panel;
        }

        public AdopterManagementPanel(RATAppBaseForm baseForm, RatAppDbContext context)
        {
            _baseForm = baseForm;
            _context = context;
            //_currentUsername = username;
            //_userRole = role;

            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(240, 240, 240);

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Page Title
            var titleLabel = new Label
            {
                Text = "Adopter Management",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64),
                Location = new Point(20, 20),
                Size = new Size(300, 40),
                AutoSize = true
            };

            // Search and Filter Controls
            searchBox = new TextBox
            {
                Location = new Point(20, 70),
                Size = new Size(250, 30),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "Search adopters..."
            };

            filterComboBox = new ComboBox
            {
                Location = new Point(280, 70),
                Size = new Size(150, 30),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            filterComboBox.Items.AddRange(new object[] { "All", "Name", "Email", "Phone", "Animal Type" });
            filterComboBox.SelectedIndex = 0;

            refreshButton = CreateButton("Refresh", 440, 70, 100, Color.FromArgb(0, 120, 215));
            refreshButton.Click += async (s, e) => await LoadDataAsync();

            addButton = CreateButton("Add New", 550, 70, 100, Color.FromArgb(0, 150, 136));
            addButton.Click += (s, e) => AddNewAdopter();

            // Tab Control for different sections
            tabControl = new TabControl
            {
                Location = new Point(20, 110),
                Size = new Size(this.Width - 40, this.Height - 230),
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            // Create tabs
            var waitlistTab = new TabPage("Waitlist");
            var reservationsTab = new TabPage("Reservations");
            var pastAdoptersTab = new TabPage("Past Adopters");

            // Create DataGridViews for each tab
            waitlistGrid = CreateDataGridView();
            reservationsGrid = CreateDataGridView();
            pastAdoptersGrid = CreateDataGridView();

            waitlistTab.Controls.Add(waitlistGrid);
            reservationsTab.Controls.Add(reservationsGrid);
            pastAdoptersTab.Controls.Add(pastAdoptersGrid);

            tabControl.TabPages.Add(waitlistTab);
            tabControl.TabPages.Add(reservationsTab);
            tabControl.TabPages.Add(pastAdoptersTab);

            // Notes section
            notesLabel = new Label
            {
                Text = "Notes:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, this.Height - 110),
                Size = new Size(100, 25),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            notesTextBox = new RichTextBox
            {
                Location = new Point(20, this.Height - 85),
                Size = new Size(this.Width - 250, 75),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            editButton = CreateButton("Edit", this.Width - 220, this.Height - 85, 90, Color.FromArgb(255, 152, 0));
            editButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            editButton.Click += (s, e) => EditSelectedAdopter();

            deleteButton = CreateButton("Delete", this.Width - 120, this.Height - 85, 90, Color.FromArgb(244, 67, 54));
            deleteButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            deleteButton.Click += (s, e) => DeleteSelectedAdopter();

            // Add all controls to the panel
            this.Controls.Add(titleLabel);
            this.Controls.Add(searchBox);
            this.Controls.Add(filterComboBox);
            this.Controls.Add(refreshButton);
            this.Controls.Add(addButton);
            this.Controls.Add(tabControl);
            this.Controls.Add(notesLabel);
            this.Controls.Add(notesTextBox);
            this.Controls.Add(editButton);
            this.Controls.Add(deleteButton);

            // Add event handlers
            searchBox.TextChanged += async (s, e) => await ApplySearchFilter();
            tabControl.SelectedIndexChanged += (s, e) => UpdateNotesForSelectedAdopter();
            waitlistGrid.SelectionChanged += (s, e) => UpdateNotesForSelectedAdopter();
            reservationsGrid.SelectionChanged += (s, e) => UpdateNotesForSelectedAdopter();
            pastAdoptersGrid.SelectionChanged += (s, e) => UpdateNotesForSelectedAdopter();
        }

        private Button CreateButton(string text, int x, int y, int width, Color color)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 30),
                Font = new Font("Segoe UI", 10),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        private DataGridView CreateDataGridView()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9)
            };

            grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    ViewAdopterDetails(grid.Rows[e.RowIndex]);
                }
            };

            return grid;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                // Load waitlist data TODO this is backend logic, put in the library 
                //var waitlistAdopters = await _context.Adopters
                //    .Where(a => a.Status == "Waitlist")
                //    .Select(a => new {
                //        a.Id,
                //        a.FirstName,
                //        a.LastName,
                //        a.Email,
                //        a.Phone,
                //        AnimalType = a.PreferredAnimalType,
                //        a.DateAdded,
                //        a.Notes
                //    })
                //    .ToListAsync();

                //// Load reservations data
                //var reservationsAdopters = await _context.Adopters
                //    .Where(a => a.Status == "Reserved")
                //    .Select(a => new {
                //        a.Id,
                //        a.FirstName,
                //        a.LastName,
                //        a.Email,
                //        a.Phone,
                //        AnimalType = a.PreferredAnimalType,
                //        AnimalName = a.ReservedAnimal.Name,
                //        ReservationDate = a.DateAdded,
                //        a.Notes
                //    })
                //    .ToListAsync();

                //// Load past adopters data
                //var pastAdopters = await _context.Adopters
                //    .Where(a => a.Status == "Adopted")
                //    .Select(a => new {
                //        a.Id,
                //        a.FirstName,
                //        a.LastName,
                //        a.Email,
                //        a.Phone,
                //        AnimalName = a.AdoptedAnimals.Select(aa => aa.Name).FirstOrDefault(),
                //        AdoptionDate = a.AdoptedAnimals.Select(aa => aa.AdoptionDate).FirstOrDefault(),
                //        a.Notes
                //    })
                //    .ToListAsync();

                //// Update the DataGridViews
                //waitlistGrid.DataSource = waitlistAdopters;
                //reservationsGrid.DataSource = reservationsAdopters;
                //pastAdoptersGrid.DataSource = pastAdopters;

                // Format the grids
                FormatDataGridView(waitlistGrid);
                FormatDataGridView(reservationsGrid);
                FormatDataGridView(pastAdoptersGrid);

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatDataGridView(DataGridView grid)
        {
            if (grid.Columns.Contains("Id"))
                grid.Columns["Id"].Visible = false;

            if (grid.Columns.Contains("Notes"))
                grid.Columns["Notes"].Visible = false;

            // Format date columns
            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column.Name.Contains("Date"))
                {
                    column.DefaultCellStyle.Format = "MM/dd/yyyy";
                }
            }

            // Set column headers
            if (grid.Columns.Contains("FirstName"))
                grid.Columns["FirstName"].HeaderText = "First Name";

            if (grid.Columns.Contains("LastName"))
                grid.Columns["LastName"].HeaderText = "Last Name";

            if (grid.Columns.Contains("AnimalType"))
                grid.Columns["AnimalType"].HeaderText = "Animal Type";

            if (grid.Columns.Contains("AnimalName"))
                grid.Columns["AnimalName"].HeaderText = "Animal Name";

            if (grid.Columns.Contains("DateAdded"))
                grid.Columns["DateAdded"].HeaderText = "Date Added";

            if (grid.Columns.Contains("ReservationDate"))
                grid.Columns["ReservationDate"].HeaderText = "Reservation Date";

            if (grid.Columns.Contains("AdoptionDate"))
                grid.Columns["AdoptionDate"].HeaderText = "Adoption Date";
        }

        private async Task ApplySearchFilter()
        {
            string searchText = searchBox.Text.ToLower();
            string filterType = filterComboBox.SelectedItem.ToString();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                await LoadDataAsync();
                return;
            }

            try
            {
                // Apply filter to each grid based on the selected tab
                switch (tabControl.SelectedIndex)
                {
                    case 0: // Waitlist
                        FilterDataGridView(waitlistGrid, searchText, filterType);
                        break;
                    case 1: // Reservations
                        FilterDataGridView(reservationsGrid, searchText, filterType);
                        break;
                    case 2: // Past Adopters
                        FilterDataGridView(pastAdoptersGrid, searchText, filterType);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying filter: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterDataGridView(DataGridView grid, string searchText, string filterType)
        {
            if (grid.DataSource is IEnumerable<dynamic> source)
            {
                var filteredList = source.Where(item =>
                {
                    if (filterType == "All")
                    {
                        // Search in all string properties
                        foreach (var prop in item.GetType().GetProperties())
                        {
                            if (prop.PropertyType == typeof(string) &&
                                prop.GetValue(item)?.ToString().ToLower().Contains(searchText) == true)
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                    else
                    {
                        // Search in specific property based on filter
                        string propertyName = filterType switch
                        {
                            "Name" => "FirstName", // Could also check LastName
                            "Email" => "Email",
                            "Phone" => "Phone",
                            "Animal Type" => "AnimalType",
                            _ => ""
                        };

                        if (string.IsNullOrEmpty(propertyName))
                            return false;

                        var prop = item.GetType().GetProperty(propertyName);
                        if (prop != null && prop.GetValue(item)?.ToString().ToLower().Contains(searchText) == true)
                        {
                            return true;
                        }

                        // Special case for Name to check both first and last name
                        if (propertyName == "FirstName")
                        {
                            var lastNameProp = item.GetType().GetProperty("LastName");
                            if (lastNameProp != null && lastNameProp.GetValue(item)?.ToString().ToLower().Contains(searchText) == true)
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                }).ToList();

                grid.DataSource = filteredList;
            }
        }

        private void UpdateNotesForSelectedAdopter()
        {
            DataGridView currentGrid = null;

            switch (tabControl.SelectedIndex)
            {
                case 0:
                    currentGrid = waitlistGrid;
                    break;
                case 1:
                    currentGrid = reservationsGrid;
                    break;
                case 2:
                    currentGrid = pastAdoptersGrid;
                    break;
            }

            if (currentGrid != null && currentGrid.SelectedRows.Count > 0)
            {
                DataGridViewRow row = currentGrid.SelectedRows[0];
                if (row.Cells["Notes"].Value != null)
                {
                    notesTextBox.Text = row.Cells["Notes"].Value.ToString();
                }
                else
                {
                    notesTextBox.Text = "";
                }
            }
            else
            {
                notesTextBox.Text = "";
            }
        }

        private void AddNewAdopter()
        {
            // Open a form to add a new adopter
            // This would be implemented in a separate form class
            MessageBox.Show("Add New Adopter functionality would open a new form here.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void EditSelectedAdopter()
        {
            DataGridView currentGrid = GetCurrentGrid();
            if (currentGrid != null && currentGrid.SelectedRows.Count > 0)
            {
                int adopterId = Convert.ToInt32(currentGrid.SelectedRows[0].Cells["Id"].Value);

                // For now, just update the notes
                UpdateAdopterNotes(adopterId, notesTextBox.Text);
            }
            else
            {
                MessageBox.Show("Please select an adopter to edit.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DeleteSelectedAdopter()
        {
            DataGridView currentGrid = GetCurrentGrid();
            if (currentGrid != null && currentGrid.SelectedRows.Count > 0)
            {
                int adopterId = Convert.ToInt32(currentGrid.SelectedRows[0].Cells["Id"].Value);
                string firstName = currentGrid.SelectedRows[0].Cells["FirstName"].Value.ToString();
                string lastName = currentGrid.SelectedRows[0].Cells["LastName"].Value.ToString();

                DialogResult result = MessageBox.Show(
                    $"Are you sure you want to delete {firstName} {lastName} from the system?",
                    "Confirm Deletion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    DeleteAdopter(adopterId);
                }
            }
            else
            {
                MessageBox.Show("Please select an adopter to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ViewAdopterDetails(DataGridViewRow row)
        {
            if (row != null)
            {
                int adopterId = Convert.ToInt32(row.Cells["Id"].Value);
                string firstName = row.Cells["FirstName"].Value.ToString();
                string lastName = row.Cells["LastName"].Value.ToString();

                // This would open a detailed view of the adopter
                MessageBox.Show($"Viewing details for {firstName} {lastName} (ID: {adopterId})", "Adopter Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private DataGridView GetCurrentGrid()
        {
            return tabControl.SelectedIndex switch
            {
                0 => waitlistGrid,
                1 => reservationsGrid,
                2 => pastAdoptersGrid,
                _ => null
            };
        }

        private async void UpdateAdopterNotes(int adopterId, string notes)
        {
            try
            {
                //var adopter = await _context.Adopters.FindAsync(adopterId); TODO this is backend logic, put in the library
                //if (adopter != null)
                //{
                //    adopter.Notes = notes;
                //    await _context.SaveChangesAsync();
                //    MessageBox.Show("Notes updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating notes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void DeleteAdopter(int adopterId)
        {
            //try TODO this is backend logic, put in the library
            //{
            //    var adopter = await _context.Adopters.FindAsync(adopterId);
            //    if (adopter != null)
            //    {
            //        _context.Adopters.Remove(adopter);
            //        await _context.SaveChangesAsync();
            //        await LoadDataAsync();
            //        MessageBox.Show("Adopter deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Error deleting adopter: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
        }

        public Task RefreshDataAsync()
        {
            throw new NotImplementedException();
        }
    }
}