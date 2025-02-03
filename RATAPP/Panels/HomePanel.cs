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
        private DataGridView dataDisplayArea;

        public HomePanel(string username, string role)
        {
            _username = username;
            _role = role;
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
            dataDisplayArea.Rows.Add("Rat1", "001", "Rat", "Male", "01/01/2023", "A/A");
            dataDisplayArea.Rows.Add("Rat2", "002", "Rat", "Female", "02/01/2023", "A/B");
            dataDisplayArea.Rows.Add("Mouse1", "003", "Mouse", "Male", "03/01/2023", "B/B");

            Controls.Add(dataDisplayArea);

            // Add a button column for individual animal pages
            DataGridViewButtonColumn buttonColumn = new DataGridViewButtonColumn
            {
                HeaderText = "Individual Animal Page",
                Text = "Go to Page",
                UseColumnTextForButtonValue = true
            };
            dataDisplayArea.Columns.Add(buttonColumn);
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
