using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Services;

namespace RATAPP.Forms
{
    public class PedigreeForm : Form
    {
        private readonly RatAppDbContextFactory _contextFactory;
        private readonly AnimalDto _currentAnimal;
        private readonly LineageService _lineageService;
        private readonly TraitService _traitService;
        private readonly PDFService _pdfService;

        // UI Components
        private Panel certificatePanel;
        private Label titleLabel;
        private Label breederLabel;
        private Label animalNameLabel;
        private Label registrationLabel;
        private PictureBox logoPictureBox;
        private Button generatePdfButton;
        private Button closeButton;

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
            _pdfService = new PDFService();

            InitializeComponents();
            LoadPedigreeData();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Pedigree Certificate";
            this.BackColor = Color.White;

            // Certificate Panel (main display area)
            certificatePanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(860, 600),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            // Title
            titleLabel = new Label
            {
                Text = "Certified Pedigree",
                Font = new Font("Times New Roman", 24, FontStyle.Bold),
                ForeColor = Color.Navy,
                Location = new Point(300, 30),
                Size = new Size(300, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Breeder Name
            breederLabel = new Label
            {
                Text = "TLDR",
                Font = new Font("Times New Roman", 16, FontStyle.Bold),
                Location = new Point(50, 80),
                Size = new Size(200, 30)
            };

            // Animal Name
            animalNameLabel = new Label
            {
                Text = _currentAnimal.name,
                Font = new Font("Times New Roman", 14),
                Location = new Point(50, 120),
                Size = new Size(300, 25)
            };

            // Registration Number
            registrationLabel = new Label
            {
                Text = $"Registration: {_currentAnimal.regNum}",
                Font = new Font("Times New Roman", 14),
                Location = new Point(50, 150),
                Size = new Size(300, 25)
            };

            // Logo
            logoPictureBox = new PictureBox
            {
                Location = new Point(700, 30),
                Size = new Size(100, 100),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Image.FromFile("RATAPP/Resources/RATAPPLogo.png")
            };

            // Generate PDF Button
            generatePdfButton = new Button
            {
                Text = "Generate PDF",
                Location = new Point(650, 630),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            generatePdfButton.Click += GeneratePdfButton_Click;

            // Close Button
            closeButton = new Button
            {
                Text = "Close",
                Location = new Point(770, 630),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(200, 200, 200),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            closeButton.Click += (s, e) => this.Close();

            // Add controls to certificate panel
            certificatePanel.Controls.Add(titleLabel);
            certificatePanel.Controls.Add(breederLabel);
            certificatePanel.Controls.Add(animalNameLabel);
            certificatePanel.Controls.Add(registrationLabel);
            certificatePanel.Controls.Add(logoPictureBox);

            // Add controls to form
            this.Controls.Add(certificatePanel);
            this.Controls.Add(generatePdfButton);
            this.Controls.Add(closeButton);

            // Draw border
            certificatePanel.Paint += (s, e) =>
            {
                var borderRect = new Rectangle(0, 0, certificatePanel.Width - 1, certificatePanel.Height - 1);
                using (var borderPen = new Pen(Color.FromArgb(100, 149, 237), 2))
                {
                    e.Graphics.DrawRectangle(borderPen, borderRect);
                }
            };
        }

        private Dictionary<string, (Animal ancestor, Dictionary<string, List<string>> traits)> _ancestors = 
            new Dictionary<string, (Animal, Dictionary<string, List<string>>)>();

        private async Task LoadPedigreeData()
        {
            try
            {
                // First Generation - Parents
                var dam = await _lineageService.GetDamByAnimalId(_currentAnimal.Id);
                var sire = await _lineageService.GetSireByAnimalId(_currentAnimal.Id);

                if (dam != null)
                {
                    var damTraits = await _traitService.GetTraitMapForSingleAnimal(dam.Id);
                    _ancestors["Dam"] = (dam, damTraits);
                    AddAncestorToDisplay(dam, damTraits, 350, 200, "Dam");

                    // Second Generation - Dam's Parents
                    var damsDam = await _lineageService.GetDamByAnimalId(dam.Id);
                    var damsSire = await _lineageService.GetSireByAnimalId(dam.Id);

                    if (damsDam != null)
                    {
                        var damsDamTraits = await _traitService.GetTraitMapForSingleAnimal(damsDam.Id);
                        _ancestors["Dam's Dam"] = (damsDam, damsDamTraits);
                        AddAncestorToDisplay(damsDam, damsDamTraits, 600, 150, "Dam's Dam");
                    }

                    if (damsSire != null)
                    {
                        var damsSireTraits = await _traitService.GetTraitMapForSingleAnimal(damsSire.Id);
                        _ancestors["Dam's Sire"] = (damsSire, damsSireTraits);
                        AddAncestorToDisplay(damsSire, damsSireTraits, 600, 250, "Dam's Sire");
                    }
                }

                if (sire != null)
                {
                    var sireTraits = await _traitService.GetTraitMapForSingleAnimal(sire.Id);
                    _ancestors["Sire"] = (sire, sireTraits);
                    AddAncestorToDisplay(sire, sireTraits, 350, 300, "Sire");

                    // Second Generation - Sire's Parents
                    var siresDam = await _lineageService.GetDamByAnimalId(sire.Id);
                    var siresSire = await _lineageService.GetSireByAnimalId(sire.Id);

                    if (siresDam != null)
                    {
                        var siresDamTraits = await _traitService.GetTraitMapForSingleAnimal(siresDam.Id);
                        _ancestors["Sire's Dam"] = (siresDam, siresDamTraits);
                        AddAncestorToDisplay(siresDam, siresDamTraits, 600, 350, "Sire's Dam");
                    }

                    if (siresSire != null)
                    {
                        var siresSireTraits = await _traitService.GetTraitMapForSingleAnimal(siresSire.Id);
                        _ancestors["Sire's Sire"] = (siresSire, siresSireTraits);
                        AddAncestorToDisplay(siresSire, siresSireTraits, 600, 450, "Sire's Sire");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading pedigree data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddAncestorToDisplay(Animal animal, Dictionary<string, List<string>> traits, int x, int y, string relation)
        {
            var panel = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(250, 80),
                BorderStyle = BorderStyle.FixedSingle
            };

            var nameLabel = new Label
            {
                Text = $"{relation}: {animal.Name}",
                Location = new Point(5, 5),
                Size = new Size(240, 20),
                Font = new Font("Times New Roman", 10, FontStyle.Bold)
            };

            var traitLabel = new Label
            {
                Text = FormatTraits(traits),
                Location = new Point(5, 25),
                Size = new Size(240, 50),
                Font = new Font("Times New Roman", 9)
            };

            panel.Controls.Add(nameLabel);
            panel.Controls.Add(traitLabel);
            certificatePanel.Controls.Add(panel);
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

        private async void GeneratePdfButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "PDF files (*.pdf)|*.pdf";
                    saveDialog.FileName = $"Pedigree_{_currentAnimal.regNum}.pdf";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        _pdfService.GeneratePedigreeCertificate(saveDialog.FileName, _currentAnimal, _ancestors);
                        MessageBox.Show("Pedigree certificate has been generated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
