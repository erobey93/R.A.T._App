using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

namespace RATAPPLibrary.Services
{
    public class PdfService
    {
        public interface IPdfService
        {
            void GeneratePdfFromTemplate(string imagePath, string outputPath, string id, string animalName);
            void GeneratePedigreeCertificate(string outputPath, AnimalDto animal, Dictionary<string, (Animal ancestor, Dictionary<string, List<string>> traits)> ancestors);
            void GenerateBirthCertificateForAnimal(string outputPath, AnimalDto animal);
            void GenerateBirthCertificateForLitter(string outputPath, Litter litter);
        }

        public void GenerateBirthCertificateForAnimal(string outputPath, AnimalDto animal)
        {
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                // Draw border
                var borderPen = new XPen(XColor.FromArgb(0, 0, 0), 2);
                gfx.DrawRectangle(borderPen, 20, 20, page.Width - 40, page.Height - 40);

                // Draw title
                var titleFont = new XFont("Times New Roman", 24, XFontStyle.Bold);
                var normalFont = new XFont("Times New Roman", 12);
                var smallFont = new XFont("Times New Roman", 10);

                gfx.DrawString("Certificate of Birth", titleFont, XBrushes.Black,
                    new XRect(0, 40, page.Width, 30), XStringFormats.Center);

                // Draw date of birth
                gfx.DrawString(animal.DateOfBirth.ToString("MM-dd-yy"), normalFont, XBrushes.Black,
                    new XRect(0, 80, page.Width, 20), XStringFormats.Center);
                gfx.DrawString("Date of Birth", normalFont, XBrushes.Black,
                    new XRect(0, 100, page.Width, 20), XStringFormats.Center);

                // Draw animal details
                int startY = 150;
                int leftMargin = 50;

                gfx.DrawString($"Name/ID: {animal.name} ({animal.regNum})", normalFont, XBrushes.Black, new XPoint(leftMargin, startY));
                gfx.DrawString($"Sex: {animal.sex}", normalFont, XBrushes.Black, new XPoint(leftMargin + 300, startY));

                startY += 30;
                gfx.DrawString($"Dam: {animal.damId ?? 0}", normalFont, XBrushes.Black, new XPoint(leftMargin, startY));
                gfx.DrawString($"Sire: {animal.sireId ?? 0}", normalFont, XBrushes.Black, new XPoint(leftMargin + 300, startY));

                startY += 30;
                gfx.DrawString($"Variety: {animal.variety}", normalFont, XBrushes.Black, new XPoint(leftMargin, startY));

                startY += 30;
                gfx.DrawString($"Color: {animal.color}", normalFont, XBrushes.Black, new XPoint(leftMargin, startY));
                gfx.DrawString($"Markings: {animal.markings}", normalFont, XBrushes.Black, new XPoint(leftMargin + 300, startY));

                startY += 30;
                gfx.DrawString("Breeder's Name: Emily Robey - TLDR", normalFont, XBrushes.Black, new XPoint(leftMargin, startY));

                // Draw logo if available
                try
                {
                    XImage logo = XImage.FromFile("RATAPP/Resources/RATAPPLogo.png");
                    gfx.DrawImage(logo, page.Width - 170, 150, 100, 100);
                }
                catch
                {
                    // Logo not found - continue without it
                }

                // Draw footer
                var footerText = "Bred By: TLDR – AFRMA Registered Mousery & Rattery";
                gfx.DrawString(footerText, smallFont, XBrushes.Black,
                    new XRect(0, page.Height - 50, page.Width, 20), XStringFormats.Center);
            }

            document.Save(outputPath);
        }

        public void GenerateBirthCertificateForLitter(string outputPath, Litter litter)
        {
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                // Draw border
                var borderPen = new XPen(XColor.FromArgb(0, 0, 0), 2);
                gfx.DrawRectangle(borderPen, 20, 20, page.Width - 40, page.Height - 40);

                // Draw title
                var titleFont = new XFont("Times New Roman", 24, XFontStyle.Bold);
                var normalFont = new XFont("Times New Roman", 12);
                var smallFont = new XFont("Times New Roman", 10);

                gfx.DrawString("Certificate of Birth", titleFont, XBrushes.Black,
                    new XRect(0, 40, page.Width, 30), XStringFormats.Center);

                // Draw date of birth
                gfx.DrawString(litter.DateOfBirth.ToString("MM-dd-yy"), normalFont, XBrushes.Black,
                    new XRect(0, 80, page.Width, 20), XStringFormats.Center);
                gfx.DrawString("Date of Birth", normalFont, XBrushes.Black,
                    new XRect(0, 100, page.Width, 20), XStringFormats.Center);

                // Draw litter details
                int startY = 150;
                int leftMargin = 50;

                gfx.DrawString($"Litter Name/ID: {litter.Name}", normalFont, XBrushes.Black, new XPoint(leftMargin, startY));

                startY += 30;
                gfx.DrawString($"Dam: {litter.Pair?.Dam?.Name ?? "Unknown"}", normalFont, XBrushes.Black, new XPoint(leftMargin, startY));
                gfx.DrawString($"Sire: {litter.Pair?.Sire?.Name ?? "Unknown"}", normalFont, XBrushes.Black, new XPoint(leftMargin + 300, startY));

                startY += 30;
                gfx.DrawString($"Number of Pups: {litter.NumPups}", normalFont, XBrushes.Black, new XPoint(leftMargin, startY));

                startY += 30;
                gfx.DrawString("Breeder's Name: Emily Robey - TLDR", normalFont, XBrushes.Black, new XPoint(leftMargin, startY));

                // Draw logo if available
                try
                {
                    XImage logo = XImage.FromFile("RATAPP/Resources/RATAPPLogo.png");
                    gfx.DrawImage(logo, page.Width - 170, 150, 100, 100);
                }
                catch
                {
                    // Logo not found - continue without it
                }

                // Draw footer
                var footerText = "Bred By: TLDR – AFRMA Registered Mousery & Rattery";
                gfx.DrawString(footerText, smallFont, XBrushes.Black,
                    new XRect(0, page.Height - 50, page.Width, 20), XStringFormats.Center);
            }

            document.Save(outputPath);
        }

        public void GeneratePedigreeCertificate(string outputPath, AnimalDto animal, Dictionary<string, (Animal ancestor, Dictionary<string, List<string>> traits)> ancestors)
        {
            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                // Draw border
                var borderPen = new XPen(XColor.FromArgb(100, 149, 237), 2);
                gfx.DrawRectangle(borderPen, 20, 20, page.Width - 40, page.Height - 40);

                // Draw title
                var titleFont = new XFont("Times New Roman", 24, XFontStyle.Bold);
                var normalFont = new XFont("Times New Roman", 12);
                var smallFont = new XFont("Times New Roman", 10);

                gfx.DrawString("Certified Pedigree", titleFont, XBrushes.Navy,
                    new XRect(0, 40, page.Width, 30), XStringFormats.Center);

                // Draw breeder name
                gfx.DrawString("TLDR", new XFont("Times New Roman", 16, XFontStyle.Bold),
                    XBrushes.Black, new XPoint(40, 80));

                // Draw animal details
                gfx.DrawString($"Name: {animal.name}", normalFont, XBrushes.Black, new XPoint(40, 120));
                gfx.DrawString($"Registration: {animal.regNum}", normalFont, XBrushes.Black, new XPoint(40, 140));

                // Draw logo if available
                try
                {
                    XImage logo = XImage.FromFile("RATAPP/Resources/RATAPPLogo.png");
                    gfx.DrawImage(logo, page.Width - 120, 40, 80, 80);
                }
                catch
                {
                    // Logo not found - continue without it
                }

                // Draw ancestors
                int startY = 180;
                foreach (var entry in ancestors)
                {
                    var (ancestor, traits) = entry.Value;
                    if (ancestor != null)
                    {
                        string phenotype = FormatTraitsForPdf(traits);
                        gfx.DrawString($"{entry.Key}: {ancestor.Name}", normalFont, XBrushes.Black, new XPoint(40, startY));
                        gfx.DrawString(phenotype, smallFont, XBrushes.Black, new XPoint(40, startY + 20));
                        startY += 50;
                    }
                }

                // Draw footer
                var footerText = $"Generated by R.A.T. App on {DateTime.Now:MM/dd/yyyy}";
                gfx.DrawString(footerText, smallFont, XBrushes.Gray,
                    new XRect(0, page.Height - 50, page.Width, 20), XStringFormats.Center);
            }

            document.Save(outputPath);
        }

        private string FormatTraitsForPdf(Dictionary<string, List<string>> traits)
        {
            var phenotype = "";

            if (traits.ContainsKey("Color") && traits["Color"].Count > 0)
                phenotype += traits["Color"][0];

            if (traits.ContainsKey("Coat Type") && traits["Coat Type"].Count > 0)
                phenotype += $" {traits["Coat Type"][0]}";

            if (traits.ContainsKey("Marking") && traits["Marking"].Count > 0)
                phenotype += $", {traits["Marking"][0]}";

            if (traits.ContainsKey("Ear Type") && traits["Ear Type"].Count > 0)
                phenotype += $", {traits["Ear Type"][0]}";

            return phenotype.TrimStart(',', ' ');
        }

        public void GeneratePdfFromTemplate(string imagePath, string outputPath, string id, string animalName)
        {
            // Validate input paths
            if (!File.Exists(imagePath))
                throw new FileNotFoundException("Template image not found.", imagePath);

            // Create a new PDF document
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();

            // Draw the background image and add text
            using (var gfx = XGraphics.FromPdfPage(page))
            {
                // Load and draw the image
                XImage image = XImage.FromFile(imagePath);
                gfx.DrawImage(image, 0, 0, page.Width, page.Height);

                // Add text
                var font = new XFont("Arial", 12);
                gfx.DrawString($"Animal ID: {id}", font, XBrushes.Black, new XPoint(100, 150));  // Adjust as needed
                gfx.DrawString($"Animal Name: {animalName}", font, XBrushes.Black, new XPoint(100, 200)); // Adjust as needed
            }

            // Save the PDF
            document.Save(outputPath);
        }
    }
}
