using RATAPP.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace RATAPP.Panels
{
    public partial class HomePanel : Panel
    {
        private string _username;
        private string _role;

        private ComboBox speciesComboBox;
        private ComboBox sexComboBox;
        private TextBox searchBar;
        private Button searchButton;
        private Button addButton;
        private DataGridView dataDisplayArea;

        private RATAppBaseForm _parentForm;  // Reference to parent form (RATAppBaseForm) this tightly couples things, but it is the easiest way to use panels
        private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;

        public HomePanel(RATAppBaseForm parentForm, RATAPPLibrary.Data.DbContexts.RatAppDbContext context, string username, string role)
        {
            _parentForm = parentForm;
            _username = username;
            _role = role;
            _context = context;
            InitializePanel();
        }

        private void InitializePanel()
        {
            // Set the panel size to match the container (RATAppBaseForm)
            Dock = DockStyle.Fill;  // Ensure it fills the entire form
            BackColor = Color.LightBlue;

            // Set the title of the panel
            var usernameLabel = new Label
            {
                Text = $"Welcome, {_username}!",
                Font = new Font("Arial", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            Controls.Add(usernameLabel);

            // Home Page Specific UI (Override Base Class functionality if needed)

            // Add a label to display role (remove this later)
            var roleLabel = new Label
            {
                Text = $"Your role: {_role}",
                Font = new Font("Arial", 12, FontStyle.Regular),
                AutoSize = true,
                Location = new Point(20, 60)
            };
            Controls.Add(roleLabel);

            // Initialize other components in the panel
            InitializeSpeciesToggleButton();
            InitializeSexToggleButton();
            InitializeSearchBar();
            InitializeSearchButton();
            InitializeAddButton();
            InitializeDataDisplayArea();
        }

        private void InitializeSpeciesToggleButton()
        {
            speciesComboBox = new ComboBox
            {
                Font = new Font("Arial", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Navy,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 200,
                Height = 40,
                Location = new Point(20, 100)
            };

            speciesComboBox.Items.Add("All Species");
            speciesComboBox.Items.Add("Rats");
            speciesComboBox.Items.Add("Mice");
            speciesComboBox.SelectedIndex = 0;

            Controls.Add(speciesComboBox);
        }

        private void InitializeSexToggleButton()
        {
            sexComboBox = new ComboBox
            {
                Font = new Font("Arial", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Navy,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 150,
                Height = 40,
                Location = new Point(240, 100)
            };

            sexComboBox.Items.Add("All Sexes");
            sexComboBox.Items.Add("Male");
            sexComboBox.Items.Add("Female");
            sexComboBox.SelectedIndex = 0;

            Controls.Add(sexComboBox);
        }

        private void InitializeSearchBar()
        {
            searchBar = new TextBox
            {
                Font = new Font("Arial", 12F),
                Width = 400,
                Height = 30,
                Location = new Point(420, 100),
                PlaceholderText = "Search by Animal Name, ID, etc..."
            };

            Controls.Add(searchBar);
        }

        private void InitializeSearchButton()
        {
            searchButton = new Button
            {
                Text = "Search",
                Font = new Font("Arial", 12F),
                Width = 100,
                Height = 30,
                Location = new Point(830, 100)
            };

            searchButton.Click += SearchButton_Click;

            Controls.Add(searchButton);
        }

        private void InitializeAddButton()
        {
            addButton = new Button
            {
                Text = "Add New Animal",
                Font = new Font("Arial", 12F),
                Width = 100,
                Height = 30,
                Location = new Point(1030, 100)
            };

            addButton.Click += addButton_Click;

            Controls.Add(addButton);
        }

        private void InitializeDataDisplayArea()
        {
            int topPanelHeight = 150;

            dataDisplayArea = new DataGridView
            {
                Location = new Point(0, topPanelHeight),
                Width = 1000,
                Height = 400,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single,
                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders
            };

            // Add columns
            dataDisplayArea.Columns.Add("AnimalName", "Animal Name");
            dataDisplayArea.Columns.Add("AnimalID", "Animal ID");
            dataDisplayArea.Columns.Add("Species", "Species");
            dataDisplayArea.Columns.Add("Sex", "Sex");
            dataDisplayArea.Columns.Add("DOB", "DOB");
            dataDisplayArea.Columns.Add("Genotype", "Genotype");

            // Add some rows for testing
            //dataDisplayArea.Rows.Add("Rat1", "001", "Rat", "Male", "01/01/2023", "A/A");
            //dataDisplayArea.Rows.Add("Rat2", "002", "Rat", "Female", "02/01/2023", "A/B");
            //dataDisplayArea.Rows.Add("Mouse1", "003", "Mouse", "Male", "03/01/2023", "B/B");

            Controls.Add(dataDisplayArea);

            // Add a button column for individual animal pages
            DataGridViewButtonColumn buttonColumn = new DataGridViewButtonColumn
            {
                Name = "Individual Animal Page",  // Give it a name for easy reference
                HeaderText = "Individual Animal Page",
                Text = "Go to Page",
                UseColumnTextForButtonValue = true
            };
            dataDisplayArea.Columns.Add(buttonColumn);
            // Add an event handler for button clicks in the DataGridView
            dataDisplayArea.CellContentClick += DataGridView_CellContentClick;
        }

        private void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataDisplayArea.Columns["Individual Animal Page"].Index && e.RowIndex >= 0 && dataDisplayArea.Rows[e.RowIndex].Cells["AnimalID"].Value != null)
            {
                string animalName = dataDisplayArea.Rows[e.RowIndex].Cells["AnimalName"].Value.ToString();
                string animalID = dataDisplayArea.Rows[e.RowIndex].Cells["AnimalID"].Value.ToString(); //TODO fix this logic with string vs int 
                string species = dataDisplayArea.Rows[e.RowIndex].Cells["Species"].Value.ToString();
                string sex = dataDisplayArea.Rows[e.RowIndex].Cells["Sex"].Value.ToString();
                string dob = dataDisplayArea.Rows[e.RowIndex].Cells["DOB"].Value.ToString();
                string genotype = dataDisplayArea.Rows[e.RowIndex].Cells["Genotype"].Value.ToString();

                // Create the AnimalDetailsPanel and show it using the parent form's ShowPanel method
                var animalDetailsPanel = new AnimalPanel(_parentForm, _context, animalName, int.Parse(animalID));//new AnimalPanel(animalName, animalID, species, sex, dob, genotype); TODO need to actually pass in the details for the associated animal 
                _parentForm.ShowPanel(animalDetailsPanel);   // Use the ShowPanel method from the parent form
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            //open up a window 
            //window should contain sections for adding a new animal
            //or, could just take to an empty animal page
            // have to have some way to distinguish between adding a new animal and editing an existing animal
            //maybe if the animal id is empty, then it is a new animal
            //that check would have to be done in the animal panel
            //go to animal panel, but pass in an empty animal id
            var animalPanel = new AnimalPanel(_parentForm, _context, "", 0); // Pass in an empty animal ID for a new animal
            _parentForm.ShowPanel(animalPanel);
        }
        private void SearchButton_Click(object sender, EventArgs e)
        {
            // Get selected filter values
            string speciesFilter = speciesComboBox.SelectedItem.ToString();
            string sexFilter = sexComboBox.SelectedItem.ToString();
            string searchTerm = searchBar.Text.ToLower();

            // Filter rows first by Species
            var speciesFilteredRows = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in dataDisplayArea.Rows)
            {
                bool isVisible = true;

                // Filter by species
                if (speciesFilter != "All Species" && row.Cells["Species"].Value.ToString() != speciesFilter)
                {
                    isVisible = false;
                }

                // Only add to the filtered list if it matches the species filter
                if (isVisible)
                {
                    speciesFilteredRows.Add(row);
                }
            }

            // Now apply further filtering and sorting by Sex or Group
            var sexFilteredRows = new List<DataGridViewRow>();
            foreach (var row in speciesFilteredRows)
            {
                bool isSexVisible = true;

                // Filter by sex/group
                if (sexFilter != "All Sexes" && row.Cells["Sex"].Value.ToString() != sexFilter)
                {
                    isSexVisible = false;
                }

                // Apply the search filter
                if (!row.Cells["AnimalName"].Value.ToString().ToLower().Contains(searchTerm) &&
                    !row.Cells["AnimalID"].Value.ToString().ToLower().Contains(searchTerm))
                {
                    isSexVisible = false;
                }

                // If the row passes all filters, add it to the visible list
                if (isSexVisible)
                {
                    sexFilteredRows.Add(row);
                }
            }

            // Finally, update DataGridView rows based on filtering
            dataDisplayArea.Rows.Clear();  // Clear existing rows
            foreach (var row in sexFilteredRows)
            {
                dataDisplayArea.Rows.Add(row);
            }
        }
    }
}
