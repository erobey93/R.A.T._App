

using RATAPP.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace RATAPP.Panels
{
    public partial class PairingsAndLittersPanel : Panel, INavigable
    {
        private TabControl tabControl;
        private TabPage pairingsTab;
        private TabPage littersTab;
        private TextBox searchBox;
        private ComboBox filterComboBox;
        private Button searchButton;
        private DataGridView pairingsGridView;
        private DataGridView littersGridView;
        private Button addButton;
        private Button updateButton;
        private Button deleteButton;

        public PairingsAndLittersPanel()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // Initialize TabControl
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12F, FontStyle.Regular)
            };

            // Initialize Pairings Tab
            pairingsTab = new TabPage("Pairings");
            InitializeTabPage(pairingsTab, out pairingsGridView);

            // Initialize Litters Tab
            littersTab = new TabPage("Litters");
            InitializeTabPage(littersTab, out littersGridView);

            tabControl.TabPages.Add(pairingsTab);
            tabControl.TabPages.Add(littersTab);

            this.Controls.Add(tabControl);

            // Initialize common controls
            InitializeCommonControls();
        }

        private void InitializeTabPage(TabPage tabPage, out DataGridView gridView)
        {
            gridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular)
            };

            tabPage.Controls.Add(gridView);
        }

        private void InitializeCommonControls()
        {
            // Search and Filter Panel
            Panel searchFilterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10)
            };

            searchBox = new TextBox
            {
                Width = 200,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(10, 15)
            };

            filterComboBox = new ComboBox
            {
                Width = 150,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(220, 15),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            filterComboBox.Items.AddRange(new object[] { "All", "Current", "Past", "Future", "Species", "Line", "Project" });
            filterComboBox.SelectedIndex = 0;

            searchButton = new Button
            {
                Text = "Search",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(380, 13),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            searchButton.FlatAppearance.BorderSize = 0;
            searchButton.Click += SearchButton_Click;

            searchFilterPanel.Controls.Add(searchBox);
            searchFilterPanel.Controls.Add(filterComboBox);
            searchFilterPanel.Controls.Add(searchButton);

            // Action Buttons Panel
            Panel actionButtonsPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10)
            };

            addButton = CreateActionButton("Add", 10);
            updateButton = CreateActionButton("Update", 120);
            deleteButton = CreateActionButton("Delete", 230);

            actionButtonsPanel.Controls.Add(addButton);
            actionButtonsPanel.Controls.Add(updateButton);
            actionButtonsPanel.Controls.Add(deleteButton);

            this.Controls.Add(searchFilterPanel);
            this.Controls.Add(actionButtonsPanel);
        }

        private Button CreateActionButton(string text, int x)
        {
            Button button = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(x, 13),
                Width = 100,
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += ActionButton_Click;
            return button;
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            // Implement search functionality
            MessageBox.Show($"Searching for: {searchBox.Text}\nFilter: {filterComboBox.SelectedItem}");
        }

        private void ActionButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            string action = clickedButton.Text;
            string currentTab = tabControl.SelectedTab.Text;

            MessageBox.Show($"{action} {currentTab}");
            // Implement add, update, or delete functionality based on the action and current tab
        }

        public Task RefreshDataAsync()
        {
            // Implement data refresh logic
            return Task.CompletedTask;
        }

        // Additional methods for data operations (Add, Update, Delete, Search, Filter) to be implemented
    }
}
//using RATAPP.Forms;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;

//namespace RATAPP.Panels
//{
//    public partial class PairingsAndLittersPanel : Panel, INavigable
//    {
//        private Button addPairingButton;
//        private Button updatePairingButton;
//        private Button deletePairingButton;
//        private Button addLitterButton;
//        private Button updateLitterButton;
//        private Button deleteLitterButton;
//        private DataGridView pairingsGridView;
//        private DataGridView littersGridView;
//        private Panel pairingsButtonsPanel;
//        private Panel littersButtonsPanel;
//        private Panel pairingsGridPanel;
//        private Panel littersGridPanel;

//        public PairingsAndLittersPanel()
//        {
//            InitializeComponents();
//        }

//        private void InitializeComponents()
//        {
//            // Set the Dock style for the custom panel
//            this.Dock = DockStyle.Fill;
//            BackColor = Color.LightBlue;

//            // Initialize Pairings and Litters panels
//            InitializePairingsPanel();
//            InitializeLittersPanel();
//        }

//        private void InitializePairingsPanel()
//        {
//            // Create the panel for Pairings section
//            var pairingsPanel = new Panel
//            {
//                Dock = DockStyle.Top,
//                Height = 200
//            };

//            // Initialize Pairing Buttons Panel
//            pairingsButtonsPanel = new Panel
//            {
//                Dock = DockStyle.Top,
//                Height = 80
//            };
//            InitializePairingButtons();
//            pairingsPanel.Controls.Add(pairingsButtonsPanel);

//            // Initialize Pairings Grid Panel
//            pairingsGridPanel = new Panel
//            {
//                Dock = DockStyle.Fill
//            };
//            InitializePairingsGrid();
//            pairingsPanel.Controls.Add(pairingsGridPanel);

//            // Add pairings panel to the custom panel
//            this.Controls.Add(pairingsPanel);
//        }

//        private void InitializeLittersPanel()
//        {
//            // Create the panel for Litters section
//            var littersPanel = new Panel
//            {
//                Dock = DockStyle.Top,
//                Height = 200
//            };

//            // Initialize Litter Buttons Panel
//            littersButtonsPanel = new Panel
//            {
//                Dock = DockStyle.Top,
//                Height = 80
//            };
//            InitializeLitterButtons();
//            littersPanel.Controls.Add(littersButtonsPanel);

//            // Initialize Litters Grid Panel
//            littersGridPanel = new Panel
//            {
//                Dock = DockStyle.Fill
//            };
//            InitializeLittersGrid();
//            littersPanel.Controls.Add(littersGridPanel);

//            // Add litters panel to the custom panel
//            this.Controls.Add(littersPanel);
//        }

//        private void InitializePairingButtons()
//        {
//            // Add Pairing Button
//            addPairingButton = new Button
//            {
//                Text = "Add Pairing",
//                Font = new Font("Arial", 12F),
//                ForeColor = Color.White,
//                BackColor = Color.Navy,
//                Width = 200,
//                Height = 40,
//                Location = new Point(20, 20)
//            };
//            addPairingButton.Click += AddPairingButton_Click;

//            // Update Pairing Button
//            updatePairingButton = new Button
//            {
//                Text = "Update Pairing",
//                Font = new Font("Arial", 12F),
//                ForeColor = Color.White,
//                BackColor = Color.Navy,
//                Width = 200,
//                Height = 40,
//                Location = new Point(240, 20)
//            };
//            updatePairingButton.Click += UpdatePairingButton_Click;

//            // Delete Pairing Button
//            deletePairingButton = new Button
//            {
//                Text = "Delete Pairing",
//                Font = new Font("Arial", 12F),
//                ForeColor = Color.White,
//                BackColor = Color.Navy,
//                Width = 200,
//                Height = 40,
//                Location = new Point(460, 20)
//            };
//            deletePairingButton.Click += DeletePairingButton_Click;

//            // Add buttons to the pairingsButtonsPanel
//            pairingsButtonsPanel.Controls.Add(addPairingButton);
//            pairingsButtonsPanel.Controls.Add(updatePairingButton);
//            pairingsButtonsPanel.Controls.Add(deletePairingButton);
//        }

//        private void InitializeLitterButtons()
//        {
//            // Add Litter Button
//            addLitterButton = new Button
//            {
//                Text = "Add Litter",
//                Font = new Font("Arial", 12F),
//                ForeColor = Color.White,
//                BackColor = Color.Navy,
//                Width = 200,
//                Height = 40,
//                Location = new Point(20, 20)
//            };
//            addLitterButton.Click += AddLitterButton_Click;

//            // Update Litter Button
//            updateLitterButton = new Button
//            {
//                Text = "Update Litter",
//                Font = new Font("Arial", 12F),
//                ForeColor = Color.White,
//                BackColor = Color.Navy,
//                Width = 200,
//                Height = 40,
//                Location = new Point(240, 20)
//            };
//            updateLitterButton.Click += UpdateLitterButton_Click;

//            // Delete Litter Button
//            deleteLitterButton = new Button
//            {
//                Text = "Delete Litter",
//                Font = new Font("Arial", 12F),
//                ForeColor = Color.White,
//                BackColor = Color.Navy,
//                Width = 200,
//                Height = 40,
//                Location = new Point(460, 20)
//            };
//            deleteLitterButton.Click += DeleteLitterButton_Click;

//            // Add buttons to the littersButtonsPanel
//            littersButtonsPanel.Controls.Add(addLitterButton);
//            littersButtonsPanel.Controls.Add(updateLitterButton);
//            littersButtonsPanel.Controls.Add(deleteLitterButton);
//        }

//        private void InitializePairingsGrid()
//        {
//            // Pairings DataGridView
//            pairingsGridView = new DataGridView
//            {
//                Location = new Point(20, 20),
//                Size = new Size(pairingsGridPanel.Width - 40, 200),
//                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
//                Columns =
//                {
//                    new DataGridViewTextBoxColumn { HeaderText = "Pairing ID", Name = "PairingID" },
//                    new DataGridViewTextBoxColumn { HeaderText = "Male Animal", Name = "MaleAnimal" },
//                    new DataGridViewTextBoxColumn { HeaderText = "Female Animal", Name = "FemaleAnimal" },
//                    new DataGridViewTextBoxColumn { HeaderText = "Breeding Date", Name = "BreedingDate" }
//                }
//            };

//            // Add the DataGrid to the pairingsGridPanel
//            pairingsGridPanel.Controls.Add(pairingsGridView);
//        }

//        private void InitializeLittersGrid()
//        {
//            // Litters DataGridView
//            littersGridView = new DataGridView
//            {
//                Location = new Point(20, 20),
//                Size = new Size(littersGridPanel.Width - 40, 200),
//                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
//                Columns =
//                {
//                    new DataGridViewTextBoxColumn { HeaderText = "Litter ID", Name = "LitterID" },
//                    new DataGridViewTextBoxColumn { HeaderText = "Dam", Name = "Dam" },
//                    new DataGridViewTextBoxColumn { HeaderText = "Sire", Name = "Sire" },
//                    new DataGridViewTextBoxColumn { HeaderText = "Litter Date", Name = "LitterDate" }
//                }
//            };

//            // Add the DataGrid to the littersGridPanel
//            littersGridPanel.Controls.Add(littersGridView);
//        }

//        // Button Click Event Handlers (Just placeholders for now)
//        private void AddPairingButton_Click(object sender, EventArgs e)
//        {
//            MessageBox.Show("Add Pairing functionality will be implemented.");
//        }

//        private void UpdatePairingButton_Click(object sender, EventArgs e)
//        {
//            MessageBox.Show("Update Pairing functionality will be implemented.");
//        }

//        private void DeletePairingButton_Click(object sender, EventArgs e)
//        {
//            MessageBox.Show("Delete Pairing functionality will be implemented.");
//        }

//        private void AddLitterButton_Click(object sender, EventArgs e)
//        {
//            MessageBox.Show("Add Litter functionality will be implemented.");
//        }

//        private void UpdateLitterButton_Click(object sender, EventArgs e)
//        {
//            MessageBox.Show("Update Litter functionality will be implemented.");
//        }

//        private void DeleteLitterButton_Click(object sender, EventArgs e)
//        {
//            MessageBox.Show("Delete Litter functionality will be implemented.");
//        }

//        public Task RefreshDataAsync()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}

