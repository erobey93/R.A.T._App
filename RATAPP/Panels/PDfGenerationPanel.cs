using System;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Windows.Forms;
using PdfSharp;
using PdfSharp.Diagnostics;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;

namespace RATAPP.Panels
{
    //TODO - just messing around but not a fundamental implementation 
    public partial class PdfGenerationPanel : Panel 
    {
        private ComboBox cmbAnimalOrLitterId;
        private Button btnGeneratePdf;
        private Panel pdfPreviewPanel;

        public PdfGenerationPanel()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Dropdown for selecting ID
            cmbAnimalOrLitterId = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Height = 30
            };
            LoadAnimalOrLitterIds(); // Populate dropdown

            // Button to generate PDF
            btnGeneratePdf = new Button
            {
                Text = "Generate PDF",
                Dock = DockStyle.Top,
                Height = 40
            };
            btnGeneratePdf.Click += BtnGeneratePdf_Click;

            // Panel for PDF preview (you can later add a viewer here)
            pdfPreviewPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Add components to the panel
            Controls.Add(pdfPreviewPanel);
            Controls.Add(btnGeneratePdf);
            Controls.Add(cmbAnimalOrLitterId);
        }

        private void LoadAnimalOrLitterIds()
        {
            // Load IDs (replace with actual data fetching logic)
            cmbAnimalOrLitterId.Items.AddRange(new[] { "Animal1", "Animal2", "Litter1", "Litter2" });
            cmbAnimalOrLitterId.SelectedIndex = 0; // Default selection
        }

        private void BtnGeneratePdf_Click(object sender, EventArgs e)
        {
            if (cmbAnimalOrLitterId.SelectedItem == null)
            {
                MessageBox.Show("Please select an ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedId = cmbAnimalOrLitterId.SelectedItem.ToString();
            GenerateAndDisplayPdf(selectedId);
        }

        private void GenerateAndDisplayPdf(string id)
        {
            try
            {
                // Path to the pre-made PDF template
                string templatePath = "path/to/template.pdf";
                string outputPath = $"GeneratedPDFs/{id}_Profile.pdf";

                // Create the directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                // Populate the template
                PopulatePdfTemplate(templatePath, outputPath, id);

                // Display the PDF in the preview panel (or open it externally)
                MessageBox.Show($"PDF generated: {outputPath}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // You can replace this with an embedded PDF viewer
                System.Diagnostics.Process.Start("explorer.exe", outputPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulatePdfTemplate(string templatePath, string outputPath, string id)
        {
            // Load the template PDF
            PdfDocument template = PdfReader.Open(templatePath, PdfDocumentOpenMode.Modify);

            // Access the first page of the template
            PdfPage page = template.Pages[0];

            // Create a graphics object for the page
            using (var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page))
            {
                // Define a font
                var font = new PdfSharp.Drawing.XFont("Arial", 12);

                // Write the ID at a specific location on the page
                gfx.DrawString($"Animal or Litter ID: {id}", font, PdfSharp.Drawing.XBrushes.Black,
                    new PdfSharp.Drawing.XPoint(50, 100)); // Adjust coordinates as needed
            }

            // Save the modified PDF to the output path
            template.Save(outputPath);
        }
    }
}
