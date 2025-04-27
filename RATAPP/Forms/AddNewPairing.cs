using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using iTextSharp.text;
using RATAPP.Forms;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Services;
using Font = System.Drawing.Font;
using Image = System.Drawing.Image;

namespace RATAPP.Forms
{
    public partial class AddPairingForm : Form
    {
        private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;
        private BreedingService _breedingService;
        private SpeciesService _speciesService;
        private ProjectService _projectService; 
        private AnimalService _animalService; 
        private Pairing pairObj;

        private AnimalDto _damDto;
        private AnimalDto _sireDto;
        private List<AnimalDto> _dams;
        private List <AnimalDto> _sires;
        private Project selectedProject;
        private string speciesCommonNames; //FIXME remove 

        private List<Species> allSpecies; 
        private Species pairingSpecies;

        private TabControl tabControl;
        private ComboBox damComboBox;
        private ComboBox sireComboBox;
        private ComboBox speciesComboBox;
        private TextBox pairingIdTextBox;
        private ComboBox projectComboBox;
        private DateTimePicker pairingDatePicker;
        private Button addButton;
        private Button cancelButton;
        private DataGridView multiplePairingsGrid;
        private Button addToGridButton;
        private Button saveAllButton;
        private Button importButton;
        private PictureBox loadingSpinner;

        

        public AddPairingForm(RATAPPLibrary.Data.DbContexts.RatAppDbContext context)
        {
            _context = context;
            _breedingService = new BreedingService(context);
            _speciesService = new SpeciesService(context);
            _animalService = new AnimalService(context);
            _projectService = new ProjectService(context);

            InitializeComponents();
            LoadAnimalsAsync();
            LoadProjects();
        }

        private void InitializeComponents()
        {
            this.Text = "Add Pairing";
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
                Text = "Add Pairing",
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

            // Single Pairing Tab
            var singlePairingTab = new TabPage("Single Pairing");
            InitializeSinglePairingTab(singlePairingTab);
            tabControl.TabPages.Add(singlePairingTab);

            // Multiple Pairings Tab
            var multiplePairingsTab = new TabPage("Multiple Pairings");
            InitializeMultiplePairingsTab(multiplePairingsTab);
            tabControl.TabPages.Add(multiplePairingsTab);

            // Add tabControl to the container panel, then add the panel to the form
            tabContainerPanel.Controls.Add(tabControl);
            this.Controls.Add(tabContainerPanel);

            // Initialize loading spinner
            InitializeLoadingSpinner();
        }

        private void InitializeSinglePairingTab(TabPage tab)
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Create a GroupBox for better visual organization
            var pairingGroup = new GroupBox
            {
                Text = "Pairing Information",
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
            // Species
            var speciesLabel = new Label
            {
                Text = "Species:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(speciesLabel, 0, 0);

            speciesComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            formPanel.Controls.Add(speciesComboBox, 1, 0);
            speciesComboBox.Items.Add("Rat");
            speciesComboBox.SelectedIndex = 0;

            // Pairing ID
            var pairingIdLabel = new Label
            {
                Text = "Pairing ID:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(pairingIdLabel, 0, 1);

            pairingIdTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            formPanel.Controls.Add(pairingIdTextBox, 1, 1); 

            // Project
            var projectLabel = new Label
            {
                Text = "Project:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(projectLabel, 0, 2); 

            projectComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            formPanel.Controls.Add(projectComboBox, 1, 2); 

            // Dam (Female)
            var damLabel = new Label
            {
                Text = "Dam (Female):",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(damLabel, 0, 3); 

            damComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            formPanel.Controls.Add(damComboBox, 1, 3); 

            // Sire (Male)
            var sireLabel = new Label
            {
                Text = "Sire (Male):",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(sireLabel, 0, 4);

            sireComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            formPanel.Controls.Add(sireComboBox, 1, 4); 

            // Pairing Date
            var pairingDateLabel = new Label
            {
                Text = "Pairing Date:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(pairingDateLabel, 0, 5); 

            pairingDatePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300
            };
            formPanel.Controls.Add(pairingDatePicker, 1, 5); 

            // Buttons
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            addButton = new Button
            {
                Text = "Add Pairing",
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

            pairingGroup.Controls.Add(formPanel);
            mainPanel.Controls.Add(pairingGroup);

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
                Text = "• Pairing ID should be unique for each pairing\n" +
                       "• Ensure the dam and sire are of appropriate age for breeding\n" +
                       "• The pairing date will be used to calculate expected litter dates\n" +
                       "• You can view all pairings in the Breeding History section",
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(20, 30),
                Size = new Size(700, 100)
            };
            infoPanel.Controls.Add(infoText);

            mainPanel.Controls.Add(infoPanel);

            tab.Controls.Add(mainPanel);
        }

        //TODO add species 
        private void InitializeMultiplePairingsTab(TabPage tab)
        {
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Create input panel (same as single pairing but with "Add to List" button)
            var inputGroup = new GroupBox
            {
                Text = "New Pairing",
                Dock = DockStyle.Top,
                Height = 275,
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

            // Pairing ID
            var pairingIdLabel = new Label
            {
                Text = "Pairing ID:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(pairingIdLabel, 0, 0);

            var multiPairingIdTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            formPanel.Controls.Add(multiPairingIdTextBox, 1, 0);

            // Project
            var projectLabel = new Label
            {
                Text = "Project:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(projectLabel, 0, 1);

            var multiProjectComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            formPanel.Controls.Add(multiProjectComboBox, 1, 1);

            // Dam (Female)
            var damLabel = new Label
            {
                Text = "Dam (Female):",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(damLabel, 0, 2);

            var multiDamComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            formPanel.Controls.Add(multiDamComboBox, 1, 2);

            // Sire (Male)
            var sireLabel = new Label
            {
                Text = "Sire (Male):",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(sireLabel, 0, 3);

            var multiSireComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White
            };
            formPanel.Controls.Add(multiSireComboBox, 1, 3);

            // Pairing Date
            var pairingDateLabel = new Label
            {
                Text = "Pairing Date:",
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            formPanel.Controls.Add(pairingDateLabel, 0, 4);

            var multiPairingDatePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10),
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 300
            };
            formPanel.Controls.Add(multiPairingDatePicker, 1, 4);

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
                        string.IsNullOrWhiteSpace(multiPairingIdTextBox.Text) || multiProjectComboBox.SelectedIndex == -1)
                    {
                        MessageBox.Show("Please fill in all required fields", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Add to grid
                    multiplePairingsGrid.Rows.Add(
                        multiDamComboBox.SelectedItem.ToString(),
                        multiSireComboBox.SelectedItem.ToString(),
                        multiPairingIdTextBox.Text,
                        multiProjectComboBox.SelectedItem.ToString(),
                        multiPairingDatePicker.Value.ToShortDateString()
                    );

                    // Clear inputs for next entry
                    multiPairingIdTextBox.Text = "";

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

            // Create grid for multiple pairings
            var gridGroup = new GroupBox
            {
                Text = "Pairings to Add",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                Padding = new Padding(15)
            };

            multiplePairingsGrid = new DataGridView
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

            multiplePairingsGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            multiplePairingsGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "PairingID", HeaderText = "Pairing ID" },
                new DataGridViewTextBoxColumn { Name = "Project", HeaderText = "Project" },
                new DataGridViewTextBoxColumn { Name = "Dam", HeaderText = "Dam" },
                new DataGridViewTextBoxColumn { Name = "Sire", HeaderText = "Sire" },
                new DataGridViewTextBoxColumn { Name = "PairingDate", HeaderText = "Pairing Date" },
                new DataGridViewButtonColumn { Name = "Remove", HeaderText = "Remove", Text = "Remove", UseColumnTextForButtonValue = true }
            });

            multiplePairingsGrid.CellContentClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == multiplePairingsGrid.Columns["Remove"].Index)
                {
                    // Show loading spinner
                    loadingSpinner.Visible = true;
                    this.Refresh();

                    try
                    {
                        // Remove the row
                        multiplePairingsGrid.Rows.RemoveAt(e.RowIndex);

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

            gridGroup.Controls.Add(multiplePairingsGrid);

            // Add Save All button
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(0, 10, 0, 0)
            };

            saveAllButton = new Button
            {
                Text = "Save All Pairings",
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

            importButton = new Button
            {
                Text = "Import Data",
                Font = new Font("Segoe UI", 10),
                Width = 150,
                Height = 40,
                Location = new Point(140, 10),
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            importButton.FlatAppearance.BorderSize = 0;
            importButton.Click += ImportButton_Click;
            bottomPanel.Controls.Add(importButton);

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

            // Copy the items from the single pairing tab
            multiDamComboBox.Items.Clear();
            multiSireComboBox.Items.Clear();
            multiProjectComboBox.Items.Clear();

            // We'll populate these in the LoadAnimals and LoadProjects methods
        }
        //find species name 
        private string GetSpeciesCommonName()
        {
            Species selectedSpecies = GetAnimalSpeciesFromComboBoxSelection();
            string speciesCommonName = selectedSpecies.CommonName;

            return speciesCommonName;
        }

        //find species object
        private Species GetAnimalSpeciesFromComboBoxSelection()
        {
            string speciesCommonName = null;

            //first, check if the combo box has a selected item
            if (speciesComboBox.SelectedItem != null)
            {
                speciesCommonName = speciesComboBox.SelectedItem.ToString();
                //match the common name from our species list
                foreach (var specie in allSpecies) 
                {
                    if (speciesCommonName == specie.CommonName)
                    {
                        //set our pair species to the matched species
                        return specie;
                    }
                }
                // If the loop completes without a match, it means the selected item
                // there is something wrong, throw an exception
                throw new Exception("species common name not found in species list"); //this shouldn't be possible, but just in case 
            }
           
            else
            {
                //return unknown, use to tell the user that they need to fill out the species field first
                MessageBox.Show("Please fill out the species field first", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null; //FIXME shouldn't be returning null, but okay for now 
            }
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

        //selected index changed for species combo box
        //find the species in our species list by matching common name
        //return the 
        private async Task LoadAnimalsAsync()
        {
            //FIXME this is not correct but just for testing 
            var species = await _speciesService.GetAllSpeciesAsync();
            allSpecies = species.ToList(); 

            // Show loading spinner
            loadingSpinner.Visible = true;
            this.Refresh();

            try
            {
                // Add female rats to dam combo box
                // Populate dam and get the list
                List<AnimalDto> dams = await DropdownHelper.PopulateAnimalDropdownAsync(
                damComboBox,
                GetSpeciesCommonName,//get species from the user's selection 
                    _animalService.GetAnimalInfoBySexAndSpecies,
                    _animalService.GetAnimalsBySex,
                    "Female"
                );
                _dams = dams ?? new List<AnimalDto>(); // Store the returned list in your form-level _dams variable FIXME trying to avoid form level variables like this 
                
                // Populate sire and get the list
                List<AnimalDto> sires = await DropdownHelper.PopulateAnimalDropdownAsync(
                damComboBox,
                GetSpeciesCommonName,//get species from the user's selection 
                    _animalService.GetAnimalInfoBySexAndSpecies,
                    _animalService.GetAnimalsBySex,
                    "Male"
                );
                _sires = sires ?? new List<AnimalDto>(); // Store the returned list in your form-level _sires variable  FIXME trying to avoid form level variables like this 

                // Also populate the multiple pairings tab combo boxes
                if (tabControl.TabPages.Count > 1)
                {
                    var multiDamComboBox = tabControl.TabPages[1].Controls[0].Controls[0].Controls[0].Controls[1] as ComboBox;
                    var multiSireComboBox = tabControl.TabPages[1].Controls[0].Controls[0].Controls[0].Controls[3] as ComboBox;

                    if (multiDamComboBox != null && multiSireComboBox != null)
                    {
                        multiDamComboBox.Items.Clear();
                        multiDamComboBox.Items.AddRange(new string[] {
                          
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


        private async void LoadProjects()
        {
            // Show loading spinner
            loadingSpinner.Visible = true;
            this.Refresh();

            try
            {
                List<Project> projects = await DropdownHelper.PopulateProjectsDropdownAsync(
                    projectComboBox, _projectService.GetAllProjectsAsync //FIXME should be getting projects by species 
                    );

                // Also populate the multiple pairings tab project combo box
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

        //wrapper to get species 
        private async Task<List<Species>> GetSpecies()
        {
            List<Species> species = (List<Species>)await _speciesService.GetAllSpeciesAsync();

            return species; 
        }

        private async Task<List<Species>> LoadSpecies()
        {
            // Show loading spinner
            loadingSpinner.Visible = true;
            this.Refresh();

            try
            {
               
                //store the returned species
                allSpecies = await DropdownHelper.PopulateSpeciesDropdownAsync(speciesComboBox, GetSpecies);

                if (tabControl.TabPages.Count > 1)
                {
                    var multiSpeciesComboBox = tabControl.TabPages[1].Controls[0].Controls[0].Controls[0].Controls[7] as ComboBox;

                    if (speciesComboBox != null)
                    {
                        await DropdownHelper.PopulateSpeciesDropdownAsync(speciesComboBox, GetSpecies); //FIXME probably shouldn't be calling this twice right after the other row 
                    }
                }
            }
            finally
            {
                // Hide loading spinner
                loadingSpinner.Visible = false;
                this.Refresh();
            }

            return allSpecies;
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
                    string.IsNullOrWhiteSpace(pairingIdTextBox.Text) || projectComboBox.SelectedIndex == -1)
                {
                    MessageBox.Show("Please fill in all required fields", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Save pairing to database
                _breedingService.CreatePairingAsync(pairingIdTextBox.Text, _damDto.Id,_sireDto.Id, selectedProject.Id, pairObj.PairingStartDate, pairObj.PairingEndDate);  //FIXME not fully implemented and have to handle issue with EndData


                //if there are no errors 
                MessageBox.Show($"Pairing added successfully!\n\nDam: {damComboBox.SelectedItem}\nSire: {sireComboBox.SelectedItem}\nPairing ID: {pairingIdTextBox.Text}\nProject: {projectComboBox.SelectedItem}\nPairing Date: {pairingDatePicker.Value.ToShortDateString()}",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear form for next entry
                pairingIdTextBox.Text = "";
                damComboBox.SelectedIndex = -1;
                sireComboBox.SelectedIndex = -1;
                projectComboBox.SelectedIndex = -1;
                pairingDatePicker.Value = DateTime.Today;
            }
            finally
            {
                // Hide loading spinner
                loadingSpinner.Visible = false;
                this.Refresh();
            }
        }

        private void ImportButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"TODO - bulk import from excel logic goes here"); 
        }

        private void SaveAllButton_Click(object sender, EventArgs e)
        {
            // Show loading spinner
            loadingSpinner.Visible = true;
            this.Refresh();

            try
            {
                if (multiplePairingsGrid.Rows.Count == 0)
                {
                    MessageBox.Show("Please add at least one pairing to the list", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //Save all pairings to database
                //requires populating the pairings data object
                //which requires first storing the dam and sire data objects when selected 

                //if there are no errors 
                MessageBox.Show($"Successfully added {multiplePairingsGrid.Rows.Count} pairings!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear grid for next batch
                multiplePairingsGrid.Rows.Clear();
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


