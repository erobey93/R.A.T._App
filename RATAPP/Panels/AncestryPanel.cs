using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using RATAPP.Forms;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RATAPP.Panels
{
    public partial class AncestryPanel : Panel, INavigable
    {
        private TreeView ancestryTree;
        private Panel infoPanel;
        private Label titleLabel;
        private Button printButton;
        private Button exportButton;
        private ComboBox generationsComboBox;
        private Label generationsLabel;

        private RATAppBaseForm _parentForm;
        //private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;
        private RATAPPLibrary.Services.AnimalService _animalService;
        private RATAPPLibrary.Services.LineageService _lineageService;
        private AnimalDto[] _animals;
        private RatAppDbContextFactory _contextFactory;

        // The currently selected rodent
        private AnimalDto selectedRodent;

        public AncestryPanel(RATAppBaseForm parentForm, RatAppDbContextFactory contextFactory) //TODO - , AnimalDto[] allAnimals, AnimalDto currAnimal
        {
            _parentForm = parentForm;
            //_context = context;
            _animalService = new RATAPPLibrary.Services.AnimalService(contextFactory);
            _lineageService = new RATAPPLibrary.Services.LineageService(contextFactory);

            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // AncestryPanel
            this.Name = "AncestryPanel";
            this.Size = new Size(800, 600);
            this.BackColor = Color.White;

            this.ResumeLayout(false);
        }

        private void InitializeCustomComponents()
        {
            // Title Label
            titleLabel = new Label
            {
                Text = "Ancestry",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(200, 30),
                AutoSize = true
            };
            this.Controls.Add(titleLabel);

            // Generations selection
            generationsLabel = new Label
            {
                Text = "Generations:",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 60),
                Size = new Size(90, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(generationsLabel);

            generationsComboBox = new ComboBox
            {
                Location = new Point(110, 60),
                Size = new Size(60, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            generationsComboBox.Items.AddRange(new object[] { "1", "2", "3", "4", "5" });
            generationsComboBox.SelectedIndex = 2; // Default to 3 generations
            generationsComboBox.SelectedIndexChanged += GenerationsComboBox_SelectedIndexChanged;
            this.Controls.Add(generationsComboBox);

            // Buttons
            printButton = new Button
            {
                Text = "Print Tree",
                Location = new Point(580, 60),
                Size = new Size(90, 30),
                BackColor = SystemColors.Control
            };
            printButton.Click += PrintButton_Click;
            this.Controls.Add(printButton);

            exportButton = new Button
            {
                Text = "Export",
                Location = new Point(680, 60),
                Size = new Size(90, 30),
                BackColor = SystemColors.Control
            };
            exportButton.Click += ExportButton_Click;
            this.Controls.Add(exportButton);

            // TreeView for ancestry
            ancestryTree = new TreeView
            {
                Location = new Point(20, 100),
                Size = new Size(450, 480),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9),
                ShowNodeToolTips = true,
                HideSelection = false
            };
            ancestryTree.AfterSelect += AncestryTree_AfterSelect;
            this.Controls.Add(ancestryTree);

            // Info panel for selected rodent
            infoPanel = new Panel
            {
                Location = new Point(490, 100),
                Size = new Size(280, 480),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke
            };
            this.Controls.Add(infoPanel);

            // Add default info to the panel
            Label defaultInfoLabel = new Label
            {
                Text = "Select an animal to view details",
                Location = new Point(10, 10),
                Size = new Size(260, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            infoPanel.Controls.Add(defaultInfoLabel);
        }

        // Load ancestry data for a specific rodent
        public void LoadRodentAncestry(AnimalDto animal)
        {
            if (animal == null) return;

            selectedRodent = animal;
            titleLabel.Text = $"Ancestry Tree: {animal.name} (ID: {animal.Id})";

            // Clear existing tree
            ancestryTree.Nodes.Clear();

            // Create root node for the selected rodent
            TreeNode rootNode = CreateRodentNode(animal);
            ancestryTree.Nodes.Add(rootNode);

            // Load parents based on the selected number of generations
            int generations = int.Parse(generationsComboBox.SelectedItem.ToString());
            LoadParents(rootNode, animal, 1, generations);

            // Expand the tree
            ancestryTree.ExpandAll();

            // Select the root node
            ancestryTree.SelectedNode = rootNode;
        }

        private async void LoadParents(TreeNode parentNode, AnimalDto animal, int currentGeneration, int maxGenerations)
        {
            if (currentGeneration >= maxGenerations || animal == null) return;

            //get the animal's dam and sire FIXME library should return objects for this, just getting working for now 

            Animal dam = await _lineageService.GetDamByAnimalId(animal.Id);
            Animal sire = await _lineageService.GetSireByAnimalId(animal.Id);

            AnimalDto damDto = await _animalService.MapSingleAnimaltoDto(dam);
            AnimalDto sireDto = await _animalService.MapSingleAnimaltoDto(sire);

            // Add father if available
            if (animal.sireId != null)
            {
                
                TreeNode fatherNode = CreateRodentNode(sireDto);
                parentNode.Nodes.Add(fatherNode);
                LoadParents(fatherNode,sireDto, currentGeneration + 1, maxGenerations);
            }
            else
            {
                TreeNode unknownNode = new TreeNode("Unknown Father");
                unknownNode.ForeColor = Color.Gray;
                parentNode.Nodes.Add(unknownNode);
            }

            // Add dam if available
            if (dam != null)
            {
                TreeNode motherNode = CreateRodentNode(damDto);
                parentNode.Nodes.Add(motherNode);
                LoadParents(motherNode, damDto, currentGeneration + 1, maxGenerations);
            }
            else
            {
                TreeNode unknownNode = new TreeNode("Unknown Mother");
                unknownNode.ForeColor = Color.Gray;
                parentNode.Nodes.Add(unknownNode);
            }
        }

        private TreeNode CreateRodentNode(AnimalDto animal)
        {
            TreeNode node = new TreeNode($"{animal.name} (ID: {animal.Id})");

            // Set node color based on gender
            if (animal.sex == "Male")
            {
                node.ForeColor = Color.Blue;
            }
            else if (animal.sex == "Female")
            {
                node.ForeColor = Color.DeepPink;
            }

            var getTraits = _animalService.GetAnimalTraits(animal.Id); //FIXME this should come from the animal.Traits object but the data isn't available TODO
            
            // Set tooltip with additional information
            node.ToolTipText = $"Name: {animal.name}\n" +
                              $"ID: {animal.Id}\n" +
                              $"Gender: {animal.sex}\n" +
                              $"DOB: {animal.DateOfBirth.ToShortDateString()}\n" +
                              $"Coat: {animal.color}";

            // Store the rodent object in the tag for later reference
            node.Tag = animal;

            return node;
        }

        private void DisplayRodentInfo(AnimalDto animal)
        {
            // Clear the info panel
            infoPanel.Controls.Clear();

            if (animal == null) return;

            // Create labels for rodent information
            int yPos = 10;

            // Add photo if available
            if (animal.imageUrl != null && File.Exists(animal.imageUrl))
            {
                try
                {
                    PictureBox pictureBox = new PictureBox
                    {
                        Location = new Point(90, yPos),
                        Size = new Size(100, 100),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Image = Image.FromFile(animal.imageUrl),
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    infoPanel.Controls.Add(pictureBox);
                    yPos += 110;
                }
                catch
                {
                    // If image loading fails, just skip it
                    yPos += 10;
                }
            }
            else
            {
                // Placeholder for no image
                Panel noImagePanel = new Panel
                {
                    Location = new Point(90, yPos),
                    Size = new Size(100, 100),
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.LightGray
                };

                Label noImageLabel = new Label
                {
                    Text = "No Image",
                    Location = new Point(0, 40),
                    Size = new Size(100, 20),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                noImagePanel.Controls.Add(noImageLabel);
                infoPanel.Controls.Add(noImagePanel);
                yPos += 110;
            }

            // Add basic information
            AddInfoLabel("Name:", animal.name, yPos); yPos += 25;
            AddInfoLabel("ID:", animal.Id.ToString(), yPos); yPos += 25;
            AddInfoLabel("Gender:", animal.sex, yPos); yPos += 25;
            AddInfoLabel("Date of Birth:", animal.DateOfBirth.ToShortDateString(), yPos); yPos += 25;
            AddInfoLabel("Age:", CalculateAge(animal.DateOfBirth), yPos); yPos += 25;
            AddInfoLabel("Coat Color:", animal.color, yPos); yPos += 25;
            //AddInfoLabel("Weight:", $"{animal.Weight}g", yPos); yPos += 25;

            // Add separator
            Panel separator = new Panel
            {
                Location = new Point(10, yPos),
                Size = new Size(260, 1),
                BackColor = Color.LightGray
            };
            infoPanel.Controls.Add(separator);
            yPos += 10;

            // Add breeding information if available
            //TODO 
            //if (animal.LitterCount > 0)
            //{
            //    AddInfoLabel("Litter Count:", animal.LitterCount.ToString(), yPos); yPos += 25;
            //    AddInfoLabel("Last Litter:", animal.LastLitterDate?.ToShortDateString() ?? "N/A", yPos); yPos += 25;
            //}

            // Add view details button
            Button viewDetailsButton = new Button
            {
                Text = "View Full Details",
                Location = new Point(70, yPos),
                Size = new Size(140, 30),
                BackColor = SystemColors.Control
            };
            viewDetailsButton.Click += (sender, e) => ViewFullDetails(animal);
            infoPanel.Controls.Add(viewDetailsButton);
        }

        private void AddInfoLabel(string labelText, string value, int yPosition)
        {
            Label label = new Label
            {
                Text = labelText,
                Location = new Point(10, yPosition),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label valueLabel = new Label
            {
                Text = value,
                Location = new Point(110, yPosition),
                Size = new Size(160, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };

            infoPanel.Controls.Add(label);
            infoPanel.Controls.Add(valueLabel);
        }

        private string CalculateAge(DateTime dateOfBirth)
        {
            DateTime now = DateTime.Now;
            int months = (now.Year - dateOfBirth.Year) * 12 + now.Month - dateOfBirth.Month;

            if (months < 1)
            {
                int days = (int)(now - dateOfBirth).TotalDays;
                return $"{days} days";
            }
            else if (months < 24)
            {
                return $"{months} months";
            }
            else
            {
                int years = months / 12;
                int remainingMonths = months % 12;
                return remainingMonths > 0 ? $"{years} years, {remainingMonths} months" : $"{years} years";
            }
        }

        private async void ViewFullDetails(AnimalDto animal)
        {
            // This would open the full details form for the selected rodent
            //MessageBox.Show($"Opening full details for {rodent.Name}", "View Details", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //FIXME this shouldn't be here but just getting this to work 
            AnimalDto[] allAnimals = await _animalService.GetAllAnimalsAsync();
            // In a real implementation, you would open the rodent details form here
            AnimalPanel indAnimalPanel = new AnimalPanel(this._parentForm, _contextFactory, allAnimals, animal);
            indAnimalPanel.Show();
        }

        #region Event Handlers

        private void AncestryTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is AnimalDto selectedRodent)
            {
                DisplayRodentInfo(selectedRodent);
            }
            else
            {
                // Clear the info panel if an "Unknown" node is selected
                infoPanel.Controls.Clear();
                Label unknownLabel = new Label
                {
                    Text = "No information available",
                    Location = new Point(10, 10),
                    Size = new Size(260, 20),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                infoPanel.Controls.Add(unknownLabel);
            }
        }

        private void GenerationsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectedRodent != null)
            {
                LoadRodentAncestry(selectedRodent);
            }
        }

        private void PrintButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Printing functionality would be implemented here.", "Print Tree", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // In a real implementation, you would create a print document and print the tree
            // PrintDocument printDoc = new PrintDocument();
            // printDoc.PrintPage += PrintDoc_PrintPage;
            // PrintDialog printDialog = new PrintDialog();
            // printDialog.Document = printDoc;
            // if (printDialog.ShowDialog() == DialogResult.OK)
            // {
            //     printDoc.Print();
            // }
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Export functionality would be implemented here.", "Export Tree", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // In a real implementation, you would export the tree to a file
            // SaveFileDialog saveDialog = new SaveFileDialog();
            // saveDialog.Filter = "PDF Files (*.pdf)|*.pdf|Image Files (*.png)|*.png";
            // if (saveDialog.ShowDialog() == DialogResult.OK)
            // {
            //     // Export logic based on selected format
            // }
        }

        public Task RefreshDataAsync()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

    //mock data below this will come from the library 

//    public enum Gender
//    {
//        Male,
//        Female,
//        Unknown
//    }

//    public class AnimalDto
//    {
//        public string ID { get; set; }
//        public string Name { get; set; }
//        public Gender Gender { get; set; }
//        public DateTime DateOfBirth { get; set; }
//        public string CoatColor { get; set; }
//        public double Weight { get; set; }
//        public string PhotoPath { get; set; }
//        public int LitterCount { get; set; }
//        public DateTime? LastLitterDate { get; set; }

//        // Ancestry
//        public AnimalDto Father { get; set; }
//        public AnimalDto Mother { get; set; }

//        public AnimalDto()
//        {
//            ID = "";
//            Name = "";
//            Gender = Gender.Unknown;
//            DateOfBirth = DateTime.Now;
//            CoatColor = "";
//            Weight = 0;
//            PhotoPath = null;
//            LitterCount = 0;
//            LastLitterDate = null;
//        }
//    }
//}



//namespace RATAPP.Panels
//{
//    public partial class AnimalPanel : Panel, INavigable
// {
// Keep existing field declarations

// Add new fields for weight and cage number
//private Label weightLabel;
//private Label cageNumberLabel;
//private TextBox weightTextBox;
//private TextBox cageNumberTextBox;
//private ComboBox cageNumberComboBox;

// Constructor remains the same

//private void InitializeComponent(AnimalDto animal)
//{
//    _animal = animal;

//    // Set panel properties with a more modern look
//    this.Size = new Size(1200, 800);
//    this.BackColor = Color.FromArgb(245, 245, 245); // Slightly off-white for less eye strain
//    this.Padding = new Padding(20);
//    this.AutoScroll = true; // Enable scrolling for smaller screens

//    // Initialize the main container layout
//    InitializeMainLayout();

//    // Initialize buttons first as they're referenced by other methods
//    InitializeButtons();
//    InitializeBottomButtons();

//    if (animal != null && !_isEditMode)
//    {
//        LoadAnimalDataAsync(animal.Id);
//    }
//    else
//    {
//        InitializeComboBoxes();
//    }
//}

//        private void InitializeMainLayout()
//        {
//            // Create main container TableLayoutPanel for better organization
//            TableLayoutPanel mainContainer = new TableLayoutPanel
//            {
//                Dock = DockStyle.Fill,
//                ColumnCount = 2,
//                RowCount = 1,
//                Padding = new Padding(10),
//                BackColor = Color.Transparent
//            };

//            // Set column widths - 70% for data, 30% for image
//            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
//            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

//            // Create left panel for data entry
//            Panel dataEntryPanel = CreateDataEntryPanel();

//            // Create right panel for images
//            Panel imagePanel = CreateImagePanel();

//            // Add panels to main container
//            mainContainer.Controls.Add(dataEntryPanel, 0, 0);
//            mainContainer.Controls.Add(imagePanel, 1, 0);

//            // Add main container to the panel
//            this.Controls.Add(mainContainer);
//        }

//        private Panel CreateDataEntryPanel()
//        {
//            Panel dataPanel = new Panel
//            {
//                Dock = DockStyle.Fill,
//                AutoScroll = true,
//                Padding = new Padding(10)
//            };

//            // Create grouped sections using GroupBox for better visual organization
//            GroupBox identificationGroup = CreateGroupBox("Identification", 0);
//            GroupBox physicalGroup = CreateGroupBox("Physical Characteristics", 220);
//            GroupBox housingGroup = CreateGroupBox("Housing & Management", 440);
//            GroupBox parentageGroup = CreateGroupBox("Parentage", 660);

//            // Add identification fields (ID, Name, Registration)
//            AddFieldToGroup(identificationGroup, "Registration #/ID", regNumTextBox, 20);
//            AddFieldToGroup(identificationGroup, "Animal Name", animalNameTextBox, 60);
//            AddFieldToGroup(identificationGroup, "Species", speciesComboBox, 100);
//            AddFieldToGroup(identificationGroup, "Sex", sexComboBox, 140);

//            // Add physical characteristics fields
//            AddFieldToGroup(physicalGroup, "Variety", varietyComboBox, 20);
//            AddFieldToGroup(physicalGroup, "Color", colorComboBox, 60);
//            AddFieldToGroup(physicalGroup, "Markings", markingComboBox, 100);
//            AddFieldToGroup(physicalGroup, "Ear Type", earTypeComboBox, 140);
//            AddFieldToGroup(physicalGroup, "Weight", weightTextBox, 180); // New field

//            // Add housing & management fields
//            AddFieldToGroup(housingGroup, "Cage #", cageNumberComboBox, 20); // New field
//            AddFieldToGroup(housingGroup, "Breeder", breederInfoComboBox, 60);
//            AddFieldToGroup(housingGroup, "Genetics", genotypeComboBox, 100);
//            AddFieldToGroup(housingGroup, "% Inbred", inbredTextBox, 140);

//            // Add parentage fields
//            AddFieldToGroup(parentageGroup, "Dam", damComboBox, 20);
//            AddFieldToGroup(parentageGroup, "Sire", sireComboBox, 60);

//            // Add comments section at the bottom
//            GroupBox commentsGroup = CreateGroupBox("Comments", 880);
//            commentsGroup.Height = 150;

//            commentsTextBox = new TextBox
//            {
//                Multiline = true,
//                ScrollBars = ScrollBars.Vertical,
//                Dock = DockStyle.Fill,
//                Font = new Font("Segoe UI", 10F),
//                BackColor = Color.White
//            };

//            commentsGroup.Controls.Add(commentsTextBox);

//            // Add all groups to the data panel
//            dataPanel.Controls.Add(identificationGroup);
//            dataPanel.Controls.Add(physicalGroup);
//            dataPanel.Controls.Add(housingGroup);
//            dataPanel.Controls.Add(parentageGroup);
//            dataPanel.Controls.Add(commentsGroup);

//            return dataPanel;
//        }

//        private GroupBox CreateGroupBox(string title, int yPosition)
//        {
//            return new GroupBox
//            {
//                Text = title,
//                Location = new Point(10, yPosition),
//                Size = new Size(700, 200),
//                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
//                ForeColor = Color.FromArgb(60, 60, 60)
//            };
//        }

//        private void AddFieldToGroup(GroupBox group, string labelText, Control control, int yPosition)
//        {
//            Label label = new Label
//            {
//                Text = labelText,
//                Location = new Point(20, yPosition + 5),
//                AutoSize = true,
//                Font = new Font("Segoe UI", 9.5F)
//            };

//            control.Location = new Point(200, yPosition);
//            control.Width = 450;
//            control.Font = new Font("Segoe UI", 9.5F);

//            if (control is TextBox textBox)
//            {
//                textBox.BorderStyle = BorderStyle.FixedSingle;
//                textBox.BackColor = Color.White;
//            }
//            else if (control is ComboBox comboBox)
//            {
//                comboBox.FlatStyle = FlatStyle.Flat;
//                comboBox.BackColor = Color.White;
//                comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
//            }

//            group.Controls.Add(label);
//            group.Controls.Add(control);
//        }

//        private Panel CreateImagePanel()
//        {
//            Panel imagePanel = new Panel
//            {
//                Dock = DockStyle.Fill,
//                Padding = new Padding(10)
//            };

//            // Create a GroupBox for the image section
//            GroupBox imageGroup = new GroupBox
//            {
//                Text = "Animal Images",
//                Dock = DockStyle.Top,
//                Height = 350,
//                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
//                ForeColor = Color.FromArgb(60, 60, 60)
//            };

//            // Initialize the animal photo box with better styling
//            animalPhotoBox = new PictureBox
//            {
//                Size = new Size(250, 250),
//                Location = new Point(25, 30),
//                BorderStyle = BorderStyle.FixedSingle,
//                SizeMode = PictureBoxSizeMode.Zoom,
//                BackColor = Color.White
//            };

//            // Add a label with instructions
//            Label photoInstructionLabel = new Label
//            {
//                Text = "Click to change image",
//                Location = new Point(25, 290),
//                AutoSize = true,
//                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
//                ForeColor = Color.Gray
//            };

//            imageGroup.Controls.Add(animalPhotoBox);
//            imageGroup.Controls.Add(photoInstructionLabel);

//            // Create a GroupBox for the thumbnails
//            GroupBox thumbnailGroup = new GroupBox
//            {
//                Text = "Additional Images",
//                Dock = DockStyle.Fill,
//                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
//                ForeColor = Color.FromArgb(60, 60, 60)
//            };

//            // Initialize the thumbnail panel with better styling
//            thumbnailPanel = new FlowLayoutPanel
//            {
//                Dock = DockStyle.Fill,
//                Padding = new Padding(10),
//                AutoScroll = true,
//                FlowDirection = FlowDirection.LeftToRight,
//                WrapContents = true,
//                BackColor = Color.White
//            };

//            thumbnailGroup.Controls.Add(thumbnailPanel);

//            imagePanel.Controls.Add(imageGroup);
//            imagePanel.Controls.Add(thumbnailGroup);

//            return imagePanel;
//        }

//        // Method to initialize the action buttons with better styling
//        private void InitializeButtons()
//        {
//            // Create a panel for the action buttons
//            Panel actionButtonPanel = new Panel
//            {
//                Dock = DockStyle.Bottom,
//                Height = 60,
//                Padding = new Padding(10)
//            };

//            // Create buttons with modern styling
//            saveButton = CreateStyledButton("Save Changes", Color.FromArgb(0, 120, 212), Color.White);
//            updateButton = CreateStyledButton("Update Data", Color.FromArgb(0, 120, 212), Color.White);
//            cancelButton = CreateStyledButton("Cancel", Color.FromArgb(200, 200, 200), Color.Black);

//            // Position buttons
//            saveButton.Location = new Point(10, 10);
//            updateButton.Location = new Point(10, 10);
//            cancelButton.Location = new Point(170, 10);

//            // Set up event handlers
//            saveButton.Click += async (sender, e) => await SaveButtonClick(sender, e);
//            updateButton.Click += UpdateButtonClick;
//            cancelButton.Click += async (sender, e) => await CancelButtonClick(sender, e);

//            // Add buttons to panel
//            actionButtonPanel.Controls.Add(saveButton);
//            actionButtonPanel.Controls.Add(updateButton);
//            actionButtonPanel.Controls.Add(cancelButton);

//            // Add navigation buttons
//            prevButton = CreateStyledButton("< Previous", Color.FromArgb(240, 240, 240), Color.Black);
//            nextButton = CreateStyledButton("Next >", Color.FromArgb(240, 240, 240), Color.Black);

//            prevButton.Location = new Point(actionButtonPanel.Width - 240, 10);
//            nextButton.Location = new Point(actionButtonPanel.Width - 120, 10);

//            prevButton.Click += PreviousButtonClick;
//            nextButton.Click += NextButtonClick;

//            actionButtonPanel.Controls.Add(prevButton);
//            actionButtonPanel.Controls.Add(nextButton);

//            // Add the button panel to the main panel
//            this.Controls.Add(actionButtonPanel);

//            // Set initial button visibility
//            if (_animal != null && !_isEditMode)
//            {
//                saveButton.Visible = false;
//                cancelButton.Visible = false;
//                updateButton.Visible = true;
//            }
//            else
//            {
//                saveButton.Visible = true;
//                cancelButton.Visible = true;
//                updateButton.Visible = false;
//            }
//        }

//        private Button CreateStyledButton(string text, Color backColor, Color foreColor)
//        {
//            return new Button
//            {
//                Text = text,
//                Width = 150,
//                Height = 40,
//                Font = new Font("Segoe UI", 10F),
//                BackColor = backColor,
//                ForeColor = foreColor,
//                FlatStyle = FlatStyle.Flat,
//                FlatAppearance = { BorderSize = 0 },
//                Cursor = Cursors.Hand
//            };
//        }

//        // Method to initialize the bottom navigation buttons with better styling
//        private void InitializeBottomButtons()
//        {
//            // Create a panel for the feature navigation buttons
//            Panel featureButtonPanel = new Panel
//            {
//                Dock = DockStyle.Bottom,
//                Height = 60,
//                Padding = new Padding(10),
//                BackColor = Color.FromArgb(230, 230, 230)
//            };

//            // Create a FlowLayoutPanel to hold the buttons
//            FlowLayoutPanel buttonFlow = new FlowLayoutPanel
//            {
//                Dock = DockStyle.Fill,
//                FlowDirection = FlowDirection.LeftToRight,
//                WrapContents = false,
//                AutoScroll = true
//            };

//            // Create feature buttons with consistent styling
//            indAncestryButton = CreateFeatureButton("Ancestry", "View and edit ancestry information");
//            Button geneticsButton = CreateFeatureButton("Genetics", "Manage genetic information");
//            Button breedingHistoryButton = CreateFeatureButton("Breeding History", "View breeding records");
//            documentsButton = CreateFeatureButton("Documents", "Manage related documents");
//            healthButton = CreateFeatureButton("Health Records", "View health history");

//            // Set up event handlers
//            indAncestryButton.Click += (sender, e) => {
//                var ancestryForm = new IndividualAnimalAncestryForm(_parentForm, _context, _animal);
//                ancestryForm.Show();
//            };

//            documentsButton.Click += (sender, e) => {
//                var documentForm = new DocumentsForm(_parentForm, _context);
//                documentForm.Show();
//            };

//            healthButton.Click += (sender, e) => {
//                var healthForm = new HealthRecordForm(_parentForm, _context, _animal);
//                healthForm.Show();
//            };

//            // Add buttons to flow panel
//            buttonFlow.Controls.Add(indAncestryButton);
//            buttonFlow.Controls.Add(geneticsButton);
//            buttonFlow.Controls.Add(breedingHistoryButton);
//            buttonFlow.Controls.Add(documentsButton);
//            buttonFlow.Controls.Add(healthButton);

//            // Add flow panel to feature panel
//            featureButtonPanel.Controls.Add(buttonFlow);

//            // Add feature panel to main panel
//            this.Controls.Add(featureButtonPanel);
//        }

//        private Button CreateFeatureButton(string text, string tooltip)
//        {
//            Button button = new Button
//            {
//                Text = text,
//                Width = 140,
//                Height = 40,
//                Margin = new Padding(5, 0, 5, 0),
//                Font = new Font("Segoe UI", 9.5F),
//                BackColor = Color.FromArgb(250, 250, 250),
//                FlatStyle = FlatStyle.Flat,
//                FlatAppearance = { BorderColor = Color.FromArgb(200, 200, 200) },
//                Cursor = Cursors.Hand
//            };

//            // Add tooltip
//            ToolTip tip = new ToolTip();
//            tip.SetToolTip(button, tooltip);

//            return button;
//        }

//        // Update ParseAnimalData to include new fields
//        private AnimalDto ParseAnimalData()
//        {
//            // Existing code...

//            AnimalDto animal = new AnimalDto
//            {
//                // Existing properties...

//                // Add new properties
//                Weight = string.IsNullOrEmpty(weightTextBox.Text) ? null : weightTextBox.Text,
//                CageNumber = cageNumberComboBox.Text
//            };

//            return animal;
//        }

//        // Other methods remain largely the same, with UI improvements applied
//        // ...
//    }
//}

