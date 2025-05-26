using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Services;
using System.Collections.Generic;
using RATAPPLibrary.Services.Genetics;

namespace RATAPP.Forms
{
    public class PedigreeForm : Form
    {
        private readonly RatAppDbContextFactory _contextFactory;
        private readonly AnimalDto _currentAnimal;
        private readonly LineageService _lineageService;
        private readonly TraitService _traitService;
        private readonly PdfService _pdfService;
        private readonly AnimalService _animalService;
        private readonly BreedingCalculationService _breedingCalculationService; 

        // UI Components
        private Panel certificatePanel;
        //private Label titleLabel;
        //private Label breederLabel;
        private Label inbredCoLabel; 
        private Label animalNameLabel;
        private Label registrationLabel;
        //private PictureBox logoPictureBox;
        private Button generatePdfButton;
        private Button closeButton;

        private string inbred; 

        public static PedigreeForm Create(RatAppDbContextFactory contextFactory, AnimalDto animal)
        {
            return new PedigreeForm(contextFactory, animal);
        }

        public PedigreeForm(RatAppDbContextFactory contextFactory, AnimalDto animal)
        {
            _contextFactory = contextFactory;
            _currentAnimal = animal;
            _lineageService = new LineageService(contextFactory);
            _traitService = new TraitService(contextFactory);
            _pdfService = new PdfService();
            _animalService = new AnimalService(contextFactory);
            _breedingCalculationService = new BreedingCalculationService(contextFactory);

            InitializeComponents();
            _ = LoadPedigreeData();
        }

        private async void InitializeComponents()
        {
            this.Size = new Size(1200, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Pedigree View";
            this.BackColor = Color.White;

            // First add the buttons to the form (so they stay on top)
            // Generate PDF Button
            generatePdfButton = new Button
            {
                Text = "Generate PDF Certificate",
                Location = new Point(950, 550),  // Adjusted Y position
                Size = new Size(180, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            generatePdfButton.Click += GeneratePdfButton_Click;

            // Close Button
            closeButton = new Button
            {
                Text = "Close",
                Location = new Point(800, 550),  // Adjusted Y position and X to be next to PDF button
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(200, 200, 200),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            closeButton.Click += (s, e) => this.Close();

            // Add buttons first
            this.Controls.Add(generatePdfButton);
            this.Controls.Add(closeButton);

            // Then add the main panel
            certificatePanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(1160, 700),  // Reduced height to leave space for buttons
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                AutoScroll = true,
            };

            this.Controls.Add(certificatePanel);

            // Animal Information Section
            var animalPanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(1120, 80),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGray
            };

            // Current Animal Info
            var animalTitle = new Label
            {
                Text = "Selected Animal",
                Font = new Font("Times New Roman", 12, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(150, 20)
            };

            animalNameLabel = new Label
            {
                Text = $"Name: {_currentAnimal.name}",
                Font = new Font("Times New Roman", 11),
                Location = new Point(10, 35),
                Size = new Size(200, 20)
            };

            registrationLabel = new Label
            {
                Text = $"Registration: {_currentAnimal.regNum}",
                Font = new Font("Times New Roman", 11),
                Location = new Point(220, 35),
                Size = new Size(200, 20)
            };

            var dobLabel = new Label
            {
                Text = $"DOB: {_currentAnimal.DateOfBirth.ToString("MM/dd/yyyy") ?? "Unknown"}",
                Font = new Font("Times New Roman", 11),
                Location = new Point(440, 35),
                Size = new Size(200, 20)
            };

            inbredCoLabel = new Label
            {
                Text = $"% Inbred: {inbred}",
                Font = new Font("Times New Roman", 11),
                Location = new Point(660, 35),
                Size = new Size(200, 20)
            };

            var varietyLabel = new Label
            {
                Text = $"Variety: {_currentAnimal.variety ?? "Unknown"}",
                Font = new Font("Times New Roman", 11),
                Location = new Point(780, 35),
                Size = new Size(200, 20)
            };

            animalPanel.Controls.Add(animalTitle);
            animalPanel.Controls.Add(animalNameLabel);
            animalPanel.Controls.Add(registrationLabel);
            animalPanel.Controls.Add(dobLabel);
            animalPanel.Controls.Add(varietyLabel);
            animalPanel.Controls.Add(inbredCoLabel);

            // Tree Structure Panel
            var treePanel = new Panel
            {
                Location = new Point(20, 120),
                Size = new Size(1120, 650),
                BorderStyle = BorderStyle.None,
                BackColor = Color.White
            };

            certificatePanel.Controls.Add(animalPanel);
            certificatePanel.Controls.Add(treePanel);

            // Load the pedigree data
            _ = LoadPedigreeData(treePanel);

            //Get inbred co
            await GetInbredCoEfficient(); 
        }

        private async Task GetInbredCoEfficient()
        {
            // Logic to calculate inbreeding coefficient
            double inbredCo = await _breedingCalculationService.CalculateInbreedingCoefficientAsync(_currentAnimal.Id, _currentAnimal.Id);
            string toString = inbredCo.ToString();

            inbredCoLabel.Text = $"% Inbred: {toString}";
            this.Refresh(); 
        }

        private async Task LoadPedigreeData(Panel treePanel)
        {
            try
            {
                // Clear existing controls
                treePanel.Controls.Clear();

                // Calculate center position
                int centerX = treePanel.Width / 2 - 100;
                int startY = 20;

                // Draw current animal (center top)
                var currentAnimalBox = CreateAnimalBox(_currentAnimal, "Subject");
                currentAnimalBox.Location = new Point(centerX, startY);
                treePanel.Controls.Add(currentAnimalBox);

                // Load parents
                var dam = await _lineageService.GetDamByAnimalId(_currentAnimal.Id);
                var sire = await _lineageService.GetSireByAnimalId(_currentAnimal.Id);

                var damDto = await _animalService.MapSingleAnimaltoDto(dam);

                var sireDto = await _animalService.MapSingleAnimaltoDto(sire);

                if (dam != null || sire != null)
                {
                    int parentsY = startY + 100;

                    // Draw connecting lines
                    using (var g = treePanel.CreateGraphics())
                    {
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        using (var pen = new Pen(Color.Black, 1))
                        {
                            // Line to dam
                            if (dam != null)
                                g.DrawLine(pen, centerX + 50, startY + 50, centerX - 100, parentsY);

                            // Line to sire
                            if (sire != null)
                                g.DrawLine(pen, centerX + 50, startY + 50, centerX + 200, parentsY);
                        }
                    }

                    // Draw parents
                    if (dam != null)
                    {
                        var damBox = CreateAnimalBox(damDto, "Dam");
                        damBox.Location = new Point(centerX - 150, parentsY);
                        treePanel.Controls.Add(damBox);

                        // Load and draw dam's parents
                        await DrawAncestors(treePanel, dam, centerX - 150, parentsY, "Dam's");
                    }

                    if (sire != null)
                    {
                        var sireBox = CreateAnimalBox(sireDto, "Sire");
                        sireBox.Location = new Point(centerX + 150, parentsY);
                        treePanel.Controls.Add(sireBox);

                        // Load and draw sire's parents
                        await DrawAncestors(treePanel, sire, centerX + 150, parentsY, "Sire's");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading pedigree data: {ex.Message}", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Panel CreateAnimalBox(AnimalDto animal, string relation)
        {
            var panel = new Panel
            {
                Size = new Size(200, 80),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            var relationLabel = new Label
            {
                Text = relation,
                Font = new Font("Times New Roman", 10, FontStyle.Bold),
                Location = new Point(5, 5),
                Size = new Size(190, 15)
            };

            var nameLabel = new Label
            {
                Text = animal.name,
                Font = new Font("Times New Roman", 10, FontStyle.Bold),
                Location = new Point(5, 25),
                Size = new Size(190, 15)
            };

            var regLabel = new Label
            {
                Text = $"Reg: {animal.regNum ?? "Unknown"}",
                Font = new Font("Times New Roman", 9),
                Location = new Point(5, 45),
                Size = new Size(190, 15)
            };

            var dobLabel = new Label
            {
                Text = $"DOB: {animal.DateOfBirth.ToString("MM/dd/yyyy") ?? "Unknown"}",
                Font = new Font("Times New Roman", 9),
                Location = new Point(5, 60),
                Size = new Size(190, 15)
            };

            var inbredLabel = new Label
            {
                Text = $"DOB: {animal.DateOfBirth.ToString("MM/dd/yyyy") ?? "Unknown"}",
                Font = new Font("Times New Roman", 9),
                Location = new Point(5, 60),
                Size = new Size(190, 15)
            };

            var variety = new Label
            {
                Text = $"Variety: {animal.DateOfBirth.ToString("MM/dd/yyyy") ?? "Unknown"}",
                Font = new Font("Times New Roman", 9),
                Location = new Point(5, 60),
                Size = new Size(190, 15)
            };

            var color = new Label
            {
                Text = $"DOB: {animal.DateOfBirth.ToString("MM/dd/yyyy") ?? "Unknown"}",
                Font = new Font("Times New Roman", 9),
                Location = new Point(5, 60),
                Size = new Size(190, 15)
            };

            var markings = new Label
            {
                Text = $"DOB: {animal.DateOfBirth.ToString("MM/dd/yyyy") ?? "Unknown"}",
                Font = new Font("Times New Roman", 9),
                Location = new Point(5, 60),
                Size = new Size(190, 15)
            };

            var genotype = new Label
            {
                Text = $"DOB: {animal.DateOfBirth.ToString("MM/dd/yyyy") ?? "Unknown"}",
                Font = new Font("Times New Roman", 9),
                Location = new Point(5, 60),
                Size = new Size(190, 15)
            };

            panel.Controls.Add(relationLabel);
            panel.Controls.Add(nameLabel);
            panel.Controls.Add(regLabel);
            panel.Controls.Add(dobLabel);
            panel.Controls.Add(inbredLabel);

            return panel;
        }

        private async Task DrawAncestors(Panel treePanel, Animal parent, int parentX, int parentY, string relationPrefix)
        {
            var dam = await _lineageService.GetDamByAnimalId(parent.Id);
            var sire = await _lineageService.GetSireByAnimalId(parent.Id);

            var damDto = await _animalService.MapSingleAnimaltoDto(dam);

            var sireDto = await _animalService.MapSingleAnimaltoDto(sire);

            if (damDto != null || sireDto != null)
            {
                int grandParentsY = parentY + 100;
                int offset = 150; // Horizontal offset for grandparents

                // Draw connecting lines
                using (var g = treePanel.CreateGraphics())
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    using (var pen = new Pen(Color.Black, 1))
                    {
                        if (dam != null)
                            g.DrawLine(pen, parentX, parentY + 50, parentX - offset / 2, grandParentsY);
                        if (sire != null)
                            g.DrawLine(pen, parentX, parentY + 50, parentX + offset / 2, grandParentsY);
                    }
                }

                // Draw grandparents
                if (dam != null)
                {
                    var damBox = CreateAnimalBox(damDto, $"{relationPrefix} Dam");
                    damBox.Location = new Point(parentX - offset, grandParentsY);
                    treePanel.Controls.Add(damBox);
                }

                if (sire != null)
                {
                    var sireBox = CreateAnimalBox(sireDto, $"{relationPrefix} Sire");
                    sireBox.Location = new Point(parentX + offset, grandParentsY);
                    treePanel.Controls.Add(sireBox);
                }
            }
        }

        private Dictionary<string, (Animal ancestor, Dictionary<string, List<string>> traits)> _ancestors =
            new Dictionary<string, (Animal, Dictionary<string, List<string>>)>();

        private async Task LoadPedigreeData()
        {
            try
            {
                //FIXME - move this logic into my templateRepository and I should be grabbing ancestors instead of individual animals for dam and sire
                //TODO
                var animal = _currentAnimal as AnimalDto; 
                if (animal == null)
                {
                    //_ancestors = await _lineageService.GetAncestorsByAnimalId(animal.Id);
                }
                
                // First Generation - Parents
                var dam = await _lineageService.GetDamByAnimalId(_currentAnimal.Id);
                var sire = await _lineageService.GetSireByAnimalId(_currentAnimal.Id);

                if (dam != null)
                {
                    var damTraits = await _traitService.GetTraitMapForSingleAnimal(dam.Id);
                    _ancestors["Dam"] = (dam, damTraits);
                    AddAncestorToDisplay(dam, damTraits, "Dam");

                    // Second Generation - Dam's Parents
                    var damsDam = await _lineageService.GetDamByAnimalId(dam.Id);
                    var damsSire = await _lineageService.GetSireByAnimalId(dam.Id);

                    if (damsDam != null)
                    {
                        var damsDamTraits = await _traitService.GetTraitMapForSingleAnimal(damsDam.Id);
                        _ancestors["Dam's Dam"] = (damsDam, damsDamTraits);
                        AddAncestorToDisplay(damsDam, damsDamTraits, "Dam's Dam");
                    }

                    if (damsSire != null)
                    {
                        var damsSireTraits = await _traitService.GetTraitMapForSingleAnimal(damsSire.Id);
                        _ancestors["Dam's Sire"] = (damsSire, damsSireTraits);
                        AddAncestorToDisplay(damsSire, damsSireTraits, "Dam's Sire");
                    }
                }

                if (sire != null)
                {
                    var sireTraits = await _traitService.GetTraitMapForSingleAnimal(sire.Id);
                    _ancestors["Sire"] = (sire, sireTraits);
                    AddAncestorToDisplay(sire, sireTraits, "Sire");

                    // Second Generation - Sire's Parents
                    var siresDam = await _lineageService.GetDamByAnimalId(sire.Id);
                    var siresSire = await _lineageService.GetSireByAnimalId(sire.Id);

                    if (siresDam != null)
                    {
                        var siresDamTraits = await _traitService.GetTraitMapForSingleAnimal(siresDam.Id);
                        _ancestors["Sire's Dam"] = (siresDam, siresDamTraits);
                        AddAncestorToDisplay(siresDam, siresDamTraits, "Sire's Dam");
                    }

                    if (siresSire != null)
                    {
                        var siresSireTraits = await _traitService.GetTraitMapForSingleAnimal(siresSire.Id);
                        _ancestors["Sire's Sire"] = (siresSire, siresSireTraits);
                        AddAncestorToDisplay(siresSire, siresSireTraits, "Sire's Sire");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading pedigree data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddAncestorToDisplay(Animal animal, Dictionary<string, List<string>> traits, string relation)
        {
            // Calculate positions based on relation type
            int x = relation.Contains("'") ? 300 : 50; // Grandparents go to the right
            int y = 320; // Base Y position

            // Adjust Y position based on relation
            if (relation == "Sire") y = 350;
            else if (relation == "Dam's Dam") y = 320;
            else if (relation == "Dam's Sire") y = 350;
            else if (relation == "Sire's Dam") y = 380;
            else if (relation == "Sire's Sire") y = 410;

            var relationLabel = new Label
            {
                Text = $"{relation}:",
                Font = new Font("Times New Roman", 12, FontStyle.Bold),
                Location = new Point(x, y),
                Size = new Size(relation.Contains("'") ? 80 : 40, 20)
            };

            var nameLabel = new Label
            {
                Text = animal.Name,
                Font = new Font("Times New Roman", 12),
                Location = new Point(x + relationLabel.Width, y),
                Size = new Size(200, 20)
            };

            certificatePanel.Controls.Add(relationLabel);
            certificatePanel.Controls.Add(nameLabel);
        }

        private string FormatTraits(Dictionary<string, List<string>> traits)
        {
            var phenotype = "";

            // Add color if present
            if (traits.ContainsKey("Color") && traits["Color"].Count > 0)
                phenotype += traits["Color"][0];

            // Add variety/coat type if present
            if (traits.ContainsKey("Coat Type") && traits["Coat Type"].Count > 0)
                phenotype += $" {traits["Coat Type"][0]}";

            // Add markings if present
            if (traits.ContainsKey("Marking") && traits["Marking"].Count > 0)
                phenotype += $", {traits["Marking"][0]}";

            // Add ear type if present
            if (traits.ContainsKey("Ear Type") && traits["Ear Type"].Count > 0)
                phenotype += $", {traits["Ear Type"][0]}";

            return phenotype.TrimStart(',', ' ');
        }

        //    private async void GeneratePdfButton_Click(object sender, EventArgs e)
        //    {
        //        try
        //        {
        //            using (var saveDialog = new SaveFileDialog())
        //            {
        //                saveDialog.Filter = "PDF files (*.pdf)|*.pdf";
        //                saveDialog.FileName = $"Pedigree_{_currentAnimal.regNum}.pdf";

        //                if (saveDialog.ShowDialog() == DialogResult.OK)
        //                {
        //                    // Create a data object that combines animal and ancestors
        //                    var pdfData = new PedigreePdfData
        //                    {
        //                        Animal = _currentAnimal,
        //                        Ancestors = _ancestors,
        //                        GenerationDate = DateTime.Now
        //                    };

        //                    // Generate using the template system
        //                    _pdfService.GeneratePedigreeForAnimal(
        //                        outputPath: saveDialog.FileName,
        //                        animal: _currentAnimal);

        //                    MessageBox.Show("Pedigree certificate has been generated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show($"Error generating PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //    }
        //}

        private void GeneratePdfButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "PDF files (*.pdf)|*.pdf";
                    saveDialog.FileName = $"Pedigree_{_currentAnimal.regNum}.pdf";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Generate PDF synchronously (since it's not async)
                        _pdfService.GeneratePedigreeForAnimal(
                            outputPath: saveDialog.FileName,
                            ancestors: _ancestors,
                            animal: _currentAnimal);

                        MessageBox.Show("Pedigree generated successfully.", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Data transfer object for pedigree PDF generation
        public class PedigreePdfData
        {
            public AnimalDto Animal { get; set; }
            public Dictionary<string, (Animal ancestor, Dictionary<string, List<string>> traits)> Ancestors { get; set; }
            public DateTime GenerationDate { get; set; }
        }
    }
}
