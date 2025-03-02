using RATAPP.Forms;
using RATAPPLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RatApp.Panels
{
    public partial class AncestryPanel : RATAppBaseForm
    {
        private TreeView ancestryTree;
        private Panel infoPanel;
        private Label titleLabel;
        private Button printButton;
        private Button exportButton;
        private ComboBox generationsComboBox;
        private Label generationsLabel;

        private RATAppBaseForm _parentForm;
        private RATAPPLibrary.Data.DbContexts.RatAppDbContext _context;
        private RATAPPLibrary.Services.AnimalService _animalService;
        private AnimalDto[] _animals;

        // The currently selected rodent
        private Rodent selectedRodent;

        public AncestryPanel(RATAppBaseForm parentForm, RATAPPLibrary.Data.DbContexts.RatAppDbContext context, AnimalDto[] allAnimals, AnimalDto currAnimal)
        {
            _parentForm = parentForm;
            _context = context;
            _animalService = new RATAPPLibrary.Services.AnimalService(_context);

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
                Text = "Ancestry Tree",
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
                Text = "Select a rodent to view details",
                Location = new Point(10, 10),
                Size = new Size(260, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            infoPanel.Controls.Add(defaultInfoLabel);
        }

        // Load ancestry data for a specific rodent
        public void LoadRodentAncestry(Rodent rodent)
        {
            if (rodent == null) return;

            selectedRodent = rodent;
            titleLabel.Text = $"Ancestry Tree: {rodent.Name} (ID: {rodent.ID})";

            // Clear existing tree
            ancestryTree.Nodes.Clear();

            // Create root node for the selected rodent
            TreeNode rootNode = CreateRodentNode(rodent);
            ancestryTree.Nodes.Add(rootNode);

            // Load parents based on the selected number of generations
            int generations = int.Parse(generationsComboBox.SelectedItem.ToString());
            LoadParents(rootNode, rodent, 1, generations);

            // Expand the tree
            ancestryTree.ExpandAll();

            // Select the root node
            ancestryTree.SelectedNode = rootNode;
        }

        private void LoadParents(TreeNode parentNode, Rodent rodent, int currentGeneration, int maxGenerations)
        {
            if (currentGeneration >= maxGenerations || rodent == null) return;

            // Add father if available
            if (rodent.Father != null)
            {
                TreeNode fatherNode = CreateRodentNode(rodent.Father);
                parentNode.Nodes.Add(fatherNode);
                LoadParents(fatherNode, rodent.Father, currentGeneration + 1, maxGenerations);
            }
            else
            {
                TreeNode unknownNode = new TreeNode("Unknown Father");
                unknownNode.ForeColor = Color.Gray;
                parentNode.Nodes.Add(unknownNode);
            }

            // Add mother if available
            if (rodent.Mother != null)
            {
                TreeNode motherNode = CreateRodentNode(rodent.Mother);
                parentNode.Nodes.Add(motherNode);
                LoadParents(motherNode, rodent.Mother, currentGeneration + 1, maxGenerations);
            }
            else
            {
                TreeNode unknownNode = new TreeNode("Unknown Mother");
                unknownNode.ForeColor = Color.Gray;
                parentNode.Nodes.Add(unknownNode);
            }
        }

        private TreeNode CreateRodentNode(Rodent rodent)
        {
            TreeNode node = new TreeNode($"{rodent.Name} (ID: {rodent.ID})");

            // Set node color based on gender
            if (rodent.Gender == Gender.Male)
            {
                node.ForeColor = Color.Blue;
            }
            else if (rodent.Gender == Gender.Female)
            {
                node.ForeColor = Color.DeepPink;
            }

            // Set tooltip with additional information
            node.ToolTipText = $"Name: {rodent.Name}\n" +
                              $"ID: {rodent.ID}\n" +
                              $"Gender: {rodent.Gender}\n" +
                              $"DOB: {rodent.DateOfBirth.ToShortDateString()}\n" +
                              $"Coat: {rodent.CoatColor}";

            // Store the rodent object in the tag for later reference
            node.Tag = rodent;

            return node;
        }

        private void DisplayRodentInfo(Rodent rodent)
        {
            // Clear the info panel
            infoPanel.Controls.Clear();

            if (rodent == null) return;

            // Create labels for rodent information
            int yPos = 10;

            // Add photo if available
            if (rodent.PhotoPath != null && System.IO.File.Exists(rodent.PhotoPath))
            {
                try
                {
                    PictureBox pictureBox = new PictureBox
                    {
                        Location = new Point(90, yPos),
                        Size = new Size(100, 100),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Image = Image.FromFile(rodent.PhotoPath),
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
            AddInfoLabel("Name:", rodent.Name, yPos); yPos += 25;
            AddInfoLabel("ID:", rodent.ID, yPos); yPos += 25;
            AddInfoLabel("Gender:", rodent.Gender.ToString(), yPos); yPos += 25;
            AddInfoLabel("Date of Birth:", rodent.DateOfBirth.ToShortDateString(), yPos); yPos += 25;
            AddInfoLabel("Age:", CalculateAge(rodent.DateOfBirth), yPos); yPos += 25;
            AddInfoLabel("Coat Color:", rodent.CoatColor, yPos); yPos += 25;
            AddInfoLabel("Weight:", $"{rodent.Weight}g", yPos); yPos += 25;

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
            if (rodent.LitterCount > 0)
            {
                AddInfoLabel("Litter Count:", rodent.LitterCount.ToString(), yPos); yPos += 25;
                AddInfoLabel("Last Litter:", rodent.LastLitterDate?.ToShortDateString() ?? "N/A", yPos); yPos += 25;
            }

            // Add view details button
            Button viewDetailsButton = new Button
            {
                Text = "View Full Details",
                Location = new Point(70, yPos),
                Size = new Size(140, 30),
                BackColor = SystemColors.Control
            };
            viewDetailsButton.Click += (sender, e) => ViewFullDetails(rodent);
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

        private void ViewFullDetails(Rodent rodent)
        {
            // This would open the full details form for the selected rodent
            MessageBox.Show($"Opening full details for {rodent.Name}", "View Details", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // In a real implementation, you would open the rodent details form here
            // RodentDetailsForm detailsForm = new RodentDetailsForm(rodent);
            // detailsForm.Show();
        }

        #region Event Handlers

        private void AncestryTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Rodent selectedRodent)
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

        #endregion
    }

    // These classes would typically be defined elsewhere in your application
    // They are included here for completeness

    public enum Gender
    {
        Male,
        Female,
        Unknown
    }

    public class Rodent
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string CoatColor { get; set; }
        public double Weight { get; set; }
        public string PhotoPath { get; set; }
        public int LitterCount { get; set; }
        public DateTime? LastLitterDate { get; set; }

        // Ancestry
        public Rodent Father { get; set; }
        public Rodent Mother { get; set; }

        public Rodent()
        {
            ID = "";
            Name = "";
            Gender = Gender.Unknown;
            DateOfBirth = DateTime.Now;
            CoatColor = "";
            Weight = 0;
            PhotoPath = null;
            LitterCount = 0;
            LastLitterDate = null;
        }
    }
}

