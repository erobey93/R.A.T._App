using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Services;
using static RATAPPLibrary.Services.PdfService;

namespace RATAPP.Forms
{
    public class BirthCertificateForm : Form
    {
        private readonly RatAppDbContextFactory _contextFactory;
        private readonly AnimalDto _animal;
        private readonly Litter _litter;
        private readonly PdfService _pdfService;
        private readonly bool _isLitterCertificate;

        // UI Components
        private Panel certificatePanel;
        private Label titleLabel;
        private Label dateLabel;
        private Label nameLabel;
        private Label litterLabel;
        private Label damLabel;
        private Label sireLabel;
        private Label varietyLabel;
        private Label colorLabel;
        private Label breederLabel;
        private Label adoptionLabel;
        private Label ownerLabel;
        private PictureBox logoPictureBox;
        private Button generatePdfButton;
        private Button closeButton;

        public static BirthCertificateForm CreateForAnimal(RatAppDbContextFactory contextFactory, AnimalDto animal)
        {
            return new BirthCertificateForm(contextFactory, animal, null);
        }

        public static BirthCertificateForm CreateForLitter(RatAppDbContextFactory contextFactory, Litter litter)
        {
            return new BirthCertificateForm(contextFactory, null, litter);
        }

        private BirthCertificateForm(RatAppDbContextFactory contextFactory, AnimalDto animal, Litter litter)
        {
            _contextFactory = contextFactory;
            _animal = animal;
            _litter = litter;
            _pdfService = new PdfService();
            _isLitterCertificate = litter != null;

            InitializeComponents();
            LoadCertificateData();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Birth Certificate";
            this.BackColor = Color.White;

            // Certificate Panel
            certificatePanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(760, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            // Title
            titleLabel = new Label
            {
                Text = "Certificate of Birth",
                Font = new Font("Old English Text MT", 24, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(200, 20),
                Size = new Size(400, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Date of Birth
            dateLabel = new Label
            {
                Text = "Date of Birth",
                Font = new Font("Times New Roman", 12),
                Location = new Point(200, 70),
                Size = new Size(400, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Create and position all form fields
            CreateFormFields();

            // Logo
            logoPictureBox = new PictureBox
            {
                Location = new Point(550, 100),
                Size = new Size(150, 150),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Image.FromFile("RATAPP/Resources/RATAPPLogo.png")
            };

            // Generate PDF Button
            generatePdfButton = new Button
            {
                Text = "Generate PDF",
                Location = new Point(550, 530),
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
                Location = new Point(670, 530),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(200, 200, 200),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat
            };
            closeButton.Click += (s, e) => this.Close();

            // Add controls to certificate panel
            certificatePanel.Controls.Add(titleLabel);
            certificatePanel.Controls.Add(dateLabel);
            certificatePanel.Controls.Add(logoPictureBox);

            // Add all form fields to panel
            AddFieldsToPanel();

            // Add footer text
            var footerLabel = new Label
            {
                Text = "Bred By: TLDR – AFRMA Registered Mousery & Rattery",
                Font = new Font("Times New Roman", 10),
                Location = new Point(20, 460),
                Size = new Size(720, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            certificatePanel.Controls.Add(footerLabel);

            // Add controls to form
            this.Controls.Add(certificatePanel);
            this.Controls.Add(generatePdfButton);
            this.Controls.Add(closeButton);

            // Draw border
            certificatePanel.Paint += (s, e) =>
            {
                var borderRect = new Rectangle(0, 0, certificatePanel.Width - 1, certificatePanel.Height - 1);
                using (var borderPen = new Pen(Color.Black, 2))
                {
                    e.Graphics.DrawRectangle(borderPen, borderRect);
                }
            };
        }

        private void CreateFormFields()
        {
            // Name/ID
            nameLabel = CreateFieldLabel("Name/ID:", 50, 120);

            // Litter Name/ID
            litterLabel = CreateFieldLabel("Litter Name/ID:", 50, 150);

            // Dam & Sire
            damLabel = CreateFieldLabel("Dam:", 50, 180);
            sireLabel = CreateFieldLabel("Sire:", 50, 210);

            // Variety
            varietyLabel = CreateFieldLabel("Variety:", 50, 240);

            // Color & Markings
            colorLabel = CreateFieldLabel("Color:", 50, 270);

            // Breeder's Name
            breederLabel = CreateFieldLabel("Breeder's Name:", 50, 300);

            // Adoption Date
            adoptionLabel = CreateFieldLabel("Adoption Date:", 50, 330);

            // Owner
            ownerLabel = CreateFieldLabel("Owner:", 50, 360);
        }

        private Label CreateFieldLabel(string labelText, int x, int y)
        {
            return new Label
            {
                Text = labelText,
                Font = new Font("Times New Roman", 12),
                Location = new Point(x, y),
                Size = new Size(500, 20),
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        private void AddFieldsToPanel()
        {
            certificatePanel.Controls.Add(nameLabel);
            certificatePanel.Controls.Add(litterLabel);
            certificatePanel.Controls.Add(damLabel);
            certificatePanel.Controls.Add(sireLabel);
            certificatePanel.Controls.Add(varietyLabel);
            certificatePanel.Controls.Add(colorLabel);
            certificatePanel.Controls.Add(breederLabel);
            certificatePanel.Controls.Add(adoptionLabel);
            certificatePanel.Controls.Add(ownerLabel);
        }

        private async Task LoadCertificateData()
        {
            try
            {
                if (_isLitterCertificate)
                {
                    LoadLitterData();
                }
                else
                {
                    await LoadAnimalData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading certificate data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadLitterData()
        {
            dateLabel.Text = _litter.DateOfBirth.ToString(); //"MM-dd-yy" TODO formatting not working 
            litterLabel.Text = $"Litter Name/ID: {_litter.Name}";
            damLabel.Text = $"Dam: {_litter.Pair?.Dam?.Name ?? "Unknown"}";
            sireLabel.Text = $"Sire: {_litter.Pair?.Sire?.Name ?? "Unknown"}";
            breederLabel.Text = "Breeder's Name: Emily Robey - TLDR";
        }

        private async Task LoadAnimalData()
        {
            dateLabel.Text = _animal.DateOfBirth.ToString("MM-dd-yy");
            nameLabel.Text = $"Name/ID: {_animal.name} ({_animal.regNum})";
            
            // Get dam and sire using LineageService
            var lineageService = new LineageService(_contextFactory);
            var dam = await lineageService.GetDamByAnimalId(_animal.Id);
            var sire = await lineageService.GetSireByAnimalId(_animal.Id);

            damLabel.Text = $"Dam: {dam?.Name ?? "Unknown"}";
            sireLabel.Text = $"Sire: {sire?.Name ?? "Unknown"}";

            // Get traits
            var traitService = new TraitService(_contextFactory);
            var traits = await traitService.GetTraitMapForSingleAnimal(_animal.Id);

            string variety = "";
            if (traits.ContainsKey("Coat Type") && traits["Coat Type"].Count > 0)
                variety = traits["Coat Type"][0];

            string color = "";
            if (traits.ContainsKey("Color") && traits["Color"].Count > 0)
                color = traits["Color"][0];

            string markings = "";
            if (traits.ContainsKey("Marking") && traits["Marking"].Count > 0)
                markings = traits["Marking"][0];

            varietyLabel.Text = $"Variety: {variety}";
            colorLabel.Text = $"Color: {color} Markings: {markings}";
            breederLabel.Text = "Breeder's Name: Emily Robey - TLDR";
        }

        private async void GeneratePdfButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "PDF files (*.pdf)|*.pdf";
                    string defaultName = _isLitterCertificate ? 
                        $"BirthCertificate_Litter_{_litter.Name}.pdf" : 
                        $"BirthCertificate_{_animal.regNum}.pdf";
                    saveDialog.FileName = defaultName;

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        if (_isLitterCertificate)
                            _pdfService.GenerateBirthCertificateForLitter(saveDialog.FileName, _litter);
                        else
                            _pdfService.GenerateBirthCertificateForAnimal(saveDialog.FileName, _animal);

                        MessageBox.Show("Birth certificate has been generated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
