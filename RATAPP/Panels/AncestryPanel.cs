using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using RATAPP.Forms;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RATAPP.Panels
{
    public partial class AncestryPanel : Panel, INavigable
    {
        private TreeView ancestryTree;
        private Panel infoPanel;
        private Panel ancestryInfoPanel;
        private Panel headerPanel; 
        private Label titleLabel;
        private Button printButton;
        private Button generatePedigree;
        private ComboBox generationsComboBox;
        private Label generationsLabel;
        private DataGridView animalCollectionDataGridView;
        private PictureBox loadingSpinner;

        RATAppBaseForm _parentForm;
        private RATAPPLibrary.Services.AnimalService _animalService;
        private RATAPPLibrary.Services.LineageService _lineageService;
        private AnimalDto[] _animals;
        private RatAppDbContextFactory _contextFactory;

        // The currently selected rodent
        private AnimalDto _currentAnimal;
        private bool isSingleAnimal = false;

        public AncestryPanel(RATAppBaseForm parentForm, RatAppDbContextFactory contextFactory) //TODO - , AnimalDto[] allAnimals, AnimalDto currAnimal
        {
            _parentForm = parentForm;
            _contextFactory = contextFactory;
            _animalService = new RATAPPLibrary.Services.AnimalService(contextFactory);
            _lineageService = new RATAPPLibrary.Services.LineageService(contextFactory);

            InitializeLoadingSpinner();
            InitializeComponent();
            InitializeCustomComponents();
            InitializeHeaderPanel();
        }

        //TODO not yet implemented but may make more sense to do this vs. a whole separate form? 
        public AncestryPanel(RATAppBaseForm parentForm, RatAppDbContextFactory contextFactory, AnimalDto selectedAnimal) //TODO - , AnimalDto[] allAnimals, AnimalDto currAnimal
        {
            isSingleAnimal=true; //set flag so we know we're interested in 1 animal, not the whole collection 

            _parentForm = parentForm;
            _contextFactory = contextFactory;
            _animalService = new RATAPPLibrary.Services.AnimalService(contextFactory);
            _lineageService = new RATAPPLibrary.Services.LineageService(contextFactory);
            _currentAnimal = selectedAnimal;

            InitializeComponent();
            InitializeCustomComponents();
            InitializeHeaderPanel(); 
        }

        private void InitializeHeaderPanel()
        {
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(0, 120, 212)
            };

            Label titleLabel = new Label
            {
                Text = "Ancestry Management",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 10)
            };

            Label descriptionLabel = new Label
            {
                Text = "View family trees and create pedigrees",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(25, 40)
            };

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(descriptionLabel);
            this.Controls.Add(headerPanel);
        }

        private void InitializeLoadingSpinner()
        {
            loadingSpinner = new PictureBox
            {
                Size = new Size(50, 50),
                Image = Image.FromFile("C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\Loading_2.gif"),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Visible = false
            };
            this.Controls.Add(loadingSpinner);
            this.Resize += (s, e) => CenterLoadingSpinner();
        }

        private void CenterLoadingSpinner()
        {
            if (loadingSpinner != null)
            {
                loadingSpinner.Location = new Point(
                    (ClientSize.Width - loadingSpinner.Width) / 2,
                    (ClientSize.Height - loadingSpinner.Height) / 2
                );
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingSpinner != null)
            {
                loadingSpinner.Visible = true;
                CenterLoadingSpinner();
                this.Refresh();
            }
        }

        private void HideLoadingIndicator()
        {
            if (loadingSpinner != null)
            {
                loadingSpinner.Visible = false;
                this.Refresh();
            }
        }

        private async void InitializeDataGridView()
        {
            // Clear existing controls
            infoPanel.Controls.Clear();

            // Create container panel with padding
            var gridContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10) // Adds inner spacing
            };

            // Create and configure DataGridView
            animalCollectionDataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.WhiteSmoke,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoGenerateColumns = false // Crucial for manual column control
            };

            await GetAnimalData(); 

            // Create columns with proper data binding
            animalCollectionDataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "name", // Must match AnimalDto property name
                HeaderText = "Name",
                Name = "colName"
            });

            animalCollectionDataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "regNum",
                HeaderText = "Registration #",
                Name = "colRegNum"
            });

            animalCollectionDataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "species",
                HeaderText = "Species",
                Name = "colSpecies"
            });

            // Create binding source
            var bindingSource = new BindingSource
            {
                DataSource = _animals // Bind to full objects
            };

            // Assign binding source to DataGridView
            animalCollectionDataGridView.DataSource = bindingSource;

            // Set up proper selection handling
            animalCollectionDataGridView.SelectionChanged += AnimalDataGridView_SelectionChanged;

            // Add to panel
            infoPanel.Controls.Add(animalCollectionDataGridView);

            // Add container to main panel
            infoPanel.Controls.Add(gridContainer);

            // Configure anchoring for resizing
            infoPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom 
                             | AnchorStyles.Left | AnchorStyles.Right;
        }

        private async Task GetAnimalData()
        {
            if (!isSingleAnimal)
            {
                // Get data
                _animals = await _animalService.GetAllAnimalsAsync();
            }
            else
            {
                // Create a single-element array with the current animal
                _animals = new AnimalDto[] { _currentAnimal };
            }
        }

        private void AnimalDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (animalCollectionDataGridView.SelectedRows.Count == 0) return;

            // Get the bound item directly
            var selectedAnimal = (AnimalDto)animalCollectionDataGridView.SelectedRows[0].DataBoundItem;

            // Update other UI elements or view models
            _currentAnimal = selectedAnimal;
            UpdateAnimalDetailsDisplay(selectedAnimal);
        }

        private void UpdateAnimalDetailsDisplay(AnimalDto animal)
        {
            // Your implementation to update detail views
            // Example:
            // lblName.Text = animal.name;
            // lblRegNum.Text = animal.regNum;
            // picAnimal.Image = LoadImage(animal.imagePath);
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
            // Main form setup
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.WhiteSmoke;

            // Create main container panel
            Panel mainContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };
            this.Controls.Add(mainContainer);

            // Use TableLayoutPanel for stacking with proper spanning
            TableLayoutPanel stackedPanels = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            stackedPanels.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // For ancestryInfoPanel
            stackedPanels.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // For filterPanel
            stackedPanels.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // For contentPanel
            mainContainer.Controls.Add(stackedPanels);

            // Add info panel at the top
            ancestryInfoPanel = CreateInfoPanel(
                "Ancestry Viewer",
                "• View detailed family trees for any animal in your colony\n" +
                "• Track lineage across multiple generations\n" +
                "• Analyze breeding history and relationships\n" +
                "• Export pedigree charts for documentation"
            );
            ancestryInfoPanel.Dock = DockStyle.Fill;
            ancestryInfoPanel.Height = 120;
            ancestryInfoPanel.Margin = new Padding(0, 0, 0, 10);
            stackedPanels.Controls.Add(ancestryInfoPanel, 0, 0);

            // Create filter section
            Panel filterPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Height = 60,
                Margin = new Padding(0, 0, 0, 10)
            };
            // Generations selection (unchanged)
            generationsLabel = new Label
            {
                Text = "Generations:",
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(0, 10)
            };
            filterPanel.Controls.Add(generationsLabel);

            generationsComboBox = new ComboBox
            {
                Width = 200,
                Font = new Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(100, 8)
            };
            generationsComboBox.Items.AddRange(new object[] { "1", "2", "3", "4", "5" });
            generationsComboBox.SelectedIndex = 2;
            generationsComboBox.SelectedIndexChanged += GenerationsComboBox_SelectedIndexChanged;
            filterPanel.Controls.Add(generationsComboBox);
            stackedPanels.Controls.Add(filterPanel, 0, 1);

            // Create content panel to hold tree and info
            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };
            stackedPanels.Controls.Add(contentPanel, 0, 2);

            // Create split container for tree and info
            TableLayoutPanel splitContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            splitContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            splitContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            contentPanel.Controls.Add(splitContainer);

            // TreeView panel (left side)
            ancestryTree = new TreeView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9),
                ShowNodeToolTips = true,
                HideSelection = false
            };
            ancestryTree.AfterSelect += AncestryTree_AfterSelect;

            Panel treeContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 10, 0)
            };
            treeContainer.Controls.Add(ancestryTree);
            splitContainer.Controls.Add(treeContainer, 0, 0);

            // Info panel (right side)
            infoPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            splitContainer.Controls.Add(infoPanel, 1, 0);

            // Create button panel
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(0, 20, 0, 0)
            };

            printButton = CreateButton("Print Tree", 20);
            generatePedigree = CreateButton("Generate Pedigree", 130);

            buttonPanel.Controls.Add(printButton);
            buttonPanel.Controls.Add(generatePedigree);

            printButton.Click += PrintButton_Click;
            generatePedigree.Click += GeneratePedigree_Click;
            mainContainer.Controls.Add(buttonPanel);

            InitializeDataGridView();
        }

        private Button CreateButton(string text, int x)
        {
            Button button = new Button
            {
                Text = text,
                Font = new Font("Segoe UI", 10),
                Location = new Point(x, 0),
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(0, 120, 212),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private Panel CreateInfoPanel(string title, string content)
        {
            Panel panel = new Panel
            {
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10)
            };

            Label titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            };

            Label contentLabel = new Label
            {
                Text = content,
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                Location = new Point(10, 35),
                MaximumSize = new Size(800, 0)
            };

            panel.Controls.Add(titleLabel);
            panel.Controls.Add(contentLabel);
            return panel;
        }

        // Load ancestry data for a specific rodent
        public void LoadAnimalAncestry(AnimalDto animal)
        {
            if (animal == null) return;

            _currentAnimal = animal;

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

        private void DisplayAnimalInfo(AnimalDto animal)
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
            //AddInfoLabel("Weight:", $"{animal.Weight}g", yPos); yPos += 25; TODO

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
                Text = "View Animal's Page",
                Location = new Point(70, yPos),
                Size = new Size(140, 30),
                BackColor = SystemColors.Control
            };
            viewDetailsButton.Click += (sender, e) => ViewFullDetails(animal);
            infoPanel.Controls.Add(viewDetailsButton);

            //Add clear selection button 
            // Add view details button
            Button clearSelectionButton = new Button
            {
                Text = "Clear Selection",
                Location = new Point(70, yPos + 40),
                Size = new Size(140, 30),
                BackColor = SystemColors.Control
            };
            clearSelectionButton.Click += (sender, e) => ClearSelection();
            infoPanel.Controls.Add(clearSelectionButton);
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

        //TODO
        private async void ViewFullDetails(AnimalDto animal)
        {
            MessageBox.Show("Will navigate to individual animal page");
            //FIXME this shouldn't be here but just getting this to work 
            //AnimalDto[] allAnimals = await _animalService.GetAllAnimalsAsync();
            //AnimalPanel indAnimalPanel = new AnimalPanel(this._parentForm, _contextFactory, allAnimals, animal);
            //this.Hide();
            //indAnimalPanel.Show();
        }

        private async void ClearSelection()
        {
            isSingleAnimal = false;

            animalCollectionDataGridView.ClearSelection();
            this.Controls.Clear();
            InitializeComponent();
            InitializeCustomComponents();
            InitializeHeaderPanel();
            
        }

        #region Event Handlers

        private void AncestryTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is AnimalDto selectedRodent)
            {
                DisplayAnimalInfo(selectedRodent);
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
            if (_currentAnimal != null)
            {
                LoadAnimalAncestry(_currentAnimal);
            }
        }

        private void PrintButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Printing functionality would be implemented here.", "Print Tree", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // In a real implementation, you would create a print document and print the tree
            //PrintDocument printDoc = new PrintDocument();
            //printDoc.PrintPage += PrintDoc_PrintPage;
            //PrintDialog printDialog = new PrintDialog();
            //printDialog.Document = printDoc;
            //if (printDialog.ShowDialog() == DialogResult.OK)
            //{
            //    printDoc.Print();
            //}
        }

        private void GeneratePedigree_Click(object sender, EventArgs e)
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

        public async Task RefreshDataAsync()
        {
            //show spinner 
            await GetAnimalData();
            Refresh(); 
            //stop spinner
            //show success message 
           
        }

        #endregion
    }
}
