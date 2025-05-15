using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Services;

namespace RATAPP.Forms
{
    public class LitterDetailsForm : Form
    {
        private readonly RatAppDbContextFactory _contextFactory;
        private readonly Litter _litter;
        private readonly PDFService _pdfService;

        // UI Components
        private Label titleLabel;
        private Label litterNameLabel;
        private Label dateOfBirthLabel;
        private Label numPupsLabel;
        private Label damLabel;
        private Label sireLabel;
        private Button birthCertButton;
        private Button closeButton;
        private DataGridView pupsGridView;

        public static LitterDetailsForm Create(RatAppDbContextFactory contextFactory, Litter litter)
        {
            return new LitterDetailsForm(contextFactory, litter);
        }

        private LitterDetailsForm(RatAppDbContextFactory contextFactory, Litter litter)
        {
            _contextFactory = contextFactory;
            _litter = litter;
            _pdfService = new PDFService();

            InitializeComponents();
            LoadLitterData();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Litter Details";
            this.BackColor = Color.White;

            // Title
            titleLabel = new Label
            {
                Text = "Litter Details",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(300, 40)
            };

            // Birth Certificate Button
            birthCertButton = new Button
            {
                Text = "Generate Birth Certificate",
                Location = new Point(600, 20),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            birthCertButton.Click += BirthCertButton_Click;

            // Litter Details
            litterNameLabel = new Label
            {
                Location = new Point(20, 70),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 12)
            };

            dateOfBirthLabel = new Label
            {
                Location = new Point(20, 100),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 12)
            };

            numPupsLabel = new Label
            {
                Location = new Point(20, 130),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 12)
            };

            damLabel = new Label
            {
                Location = new Point(20, 160),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 12)
            };

            sireLabel = new Label
            {
                Location = new Point(20, 190),
                Size = new Size(400, 25),
                Font = new Font("Segoe UI", 12)
            };

            // Pups Grid
            pupsGridView = new DataGridView
            {
                Location = new Point(20, 230),
                Size = new Size(760, 280),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true
            };

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

            // Add controls
            this.Controls.AddRange(new Control[] {
                titleLabel,
                birthCertButton,
                litterNameLabel,
                dateOfBirthLabel,
                numPupsLabel,
                damLabel,
                sireLabel,
                pupsGridView,
                closeButton
            });
        }

        private void LoadLitterData()
        {
            litterNameLabel.Text = $"Litter Name: {_litter.Name}";
            dateOfBirthLabel.Text = $"Date of Birth: {_litter.DateOfBirth:MM/dd/yyyy}";
            numPupsLabel.Text = $"Number of Pups: {_litter.NumPups}";
            damLabel.Text = $"Dam: {_litter.Pair?.Dam?.Name ?? "Unknown"}";
            sireLabel.Text = $"Sire: {_litter.Pair?.Sire?.Name ?? "Unknown"}";

            // Set up grid columns
            pupsGridView.Columns.Clear();
            pupsGridView.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { Name = "ID", HeaderText = "ID" },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name" },
                new DataGridViewTextBoxColumn { Name = "Sex", HeaderText = "Sex" },
                new DataGridViewTextBoxColumn { Name = "Color", HeaderText = "Color" },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status" }
            });

            // Load pups data
            LoadPupsData();
        }

        private async void LoadPupsData()
        {
            try
            {
                pupsGridView.Rows.Clear();

                if (_litter.Animals != null)
                {
                    foreach (var pup in _litter.Animals)
                    {
                        var traitService = new TraitService(_contextFactory);
                        var traits = await traitService.GetTraitMapForSingleAnimal(pup.Id);
                        string color = traits.ContainsKey("Color") && traits["Color"].Count > 0 ? 
                            traits["Color"][0] : "Unknown";

                        pupsGridView.Rows.Add(
                            pup.registrationNumber,
                            pup.Name,
                            pup.Sex,
                            color,
                            "Active"  // TODO: Add status tracking
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading pups data: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BirthCertButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "PDF files (*.pdf)|*.pdf";
                    saveDialog.FileName = $"BirthCertificate_Litter_{_litter.Name}.pdf";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        _pdfService.GenerateBirthCertificateForLitter(saveDialog.FileName, _litter);
                        MessageBox.Show("Birth certificate has been generated successfully.", 
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating birth certificate: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
