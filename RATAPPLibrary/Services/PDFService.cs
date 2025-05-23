using PdfSharp.Drawing;
using PdfSharp.Pdf;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Breeding;
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

        private readonly PdfTemplateRepository _templateRepository;

        public PdfService()
        {
            _templateRepository = new PdfTemplateRepository();
        }

        public void GenerateBirthCertificateForAnimal(string outputPath, AnimalDto animal)
        {
            var template = _templateRepository.GetTemplate("BirthCertificateAnimal");
            GeneratePdfFromTemplate(outputPath, template, animal);
        }

        public void GeneratePdfFromTemplate(string outputPath, PdfTemplate template, object data)
        {
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            page.Size = template.PageSize;

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                // Draw border
                var borderPen = new XPen(XColor.FromArgb(0, 0, 0), 2);
                gfx.DrawRectangle(borderPen,
                    template.Margins.Left,
                    template.Margins.Top,
                    page.Width - template.Margins.Left - template.Margins.Right,
                    page.Height - template.Margins.Top - template.Margins.Bottom);

                foreach (var element in template.Elements)
                {
                    switch (element)
                    {
                        case TextElement textElement:
                            RenderTextElement(gfx, textElement, data, template);
                            break;
                        case ImageElement imageElement:
                            RenderImageElement(gfx, imageElement, data);
                            break;
                    }
                }
            }

            document.Save(outputPath);
        }

        private void RenderTextElement(XGraphics gfx, TextElement element, object data, PdfTemplate template)
        {
            var fontStyle = element.IsBold ? XFontStyleEx.Bold : XFontStyleEx.Regular;
            var font = new XFont(element.FontFamily, element.FontSize, fontStyle);
            var brush = XBrushes.Black; // Could parse from element.Color

            string text = element.Text;
            if (!string.IsNullOrEmpty(element.DataField))
            {
                text = GetValueFromData(element.DataField, data, template);
            }

            var rect = new XRect(element.X, element.Y, element.Width, element.Height);
            var format = XStringFormats.TopLeft;

            switch (element.HorizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    format = XStringFormats.TopCenter;
                    break;
                case HorizontalAlignment.Right:
                    format = XStringFormats.TopRight;
                    break;
            }

            gfx.DrawString(text, font, brush, rect, format);
        }

        private void RenderImageElement(XGraphics gfx, ImageElement element, object data)
        {
            try
            {
                string imagePath = element.ImagePath;
                if (!string.IsNullOrEmpty(element.DataField))
                {
                    imagePath = GetValueFromData(element.DataField, data, null) as string;
                }

                if (File.Exists(imagePath))
                {
                    XImage image = XImage.FromFile(imagePath);
                    gfx.DrawImage(image, element.X, element.Y, element.Width, element.Height);
                }
            }
            catch
            {
                // Image not found - skip it
            }
        }

        private string GetValueFromData(string dataField, object data, PdfTemplate template)
        {
            // Check template defaults first
            if (template?.DefaultValues != null && template.DefaultValues.TryGetValue(dataField, out var defaultValue))
            {
                return defaultValue;
            }

            // Use reflection to get value from data object
            var property = data.GetType().GetProperty(dataField);
            if (property != null)
            {
                var value = property.GetValue(data);
                if (value is DateTime dateValue)
                {
                    return dateValue.ToString("MM-dd-yy");
                }
                return value?.ToString() ?? string.Empty;
            }

            return string.Empty;
        }
        //public void GenerateBirthCertificateForAnimal(string outputPath, AnimalDto animal)
        //{
        //    PdfDocument document = new PdfDocument();
        //    PdfPage page = document.AddPage();
        //    page.Size = PdfSharp.PageSize.A4;

        //    using (var gfx = XGraphics.FromPdfPage(page))
        //    {
        //        // Draw border
        //        var borderPen = new XPen(XColor.FromArgb(0, 0, 0), 2);
        //        gfx.DrawRectangle(borderPen, 20, 20, page.Width - 40, page.Height - 40);

        //        // Draw title
        //        var titleFont = new XFont("Times New Roman", 24); //FIXME , XFontStyle.Bold doesn't work need to fix 
        //        var normalFont = new XFont("Times New Roman", 12);
        //        var smallFont = new XFont("Times New Roman", 10);

        //        gfx.DrawString("Certificate of Birth", titleFont, XBrushes.Black,
        //            new XRect(0, 40, page.Width, 30), XStringFormats.Center);

        //        // Draw date of birth
        //        gfx.DrawString(animal.DateOfBirth.ToString("MM-dd-yy"), normalFont, XBrushes.Black,
        //            new XRect(0, 80, page.Width, 20), XStringFormats.Center);
        //        gfx.DrawString("Date of Birth", normalFont, XBrushes.Black,
        //            new XRect(0, 100, page.Width, 20), XStringFormats.Center);

        //        // Draw animal details
        //        int startY = 150;
        //        int leftMargin = 50;

        //        gfx.DrawString($"Name/ID: {animal.name} ({animal.regNum})", normalFont, XBrushes.Black, new XPoint(leftMargin, startY));
        //        gfx.DrawString($"Sex: {animal.sex}", normalFont, XBrushes.Black, new XPoint(leftMargin + 300, startY));

        //        startY += 30;
        //        gfx.DrawString($"Dam: {animal.damId ?? 0}", normalFont, XBrushes.Black, new XPoint(leftMargin, startY));
        //        gfx.DrawString($"Sire: {animal.sireId ?? 0}", normalFont, XBrushes.Black, new XPoint(leftMargin + 300, startY));

        //        startY += 30;
        //        gfx.DrawString($"Variety: {animal.variety}", normalFont, XBrushes.Black, new XPoint(leftMargin, startY));

        //        startY += 30;
        //        gfx.DrawString($"Color: {animal.color}", normalFont, XBrushes.Black, new XPoint(leftMargin, startY));
        //        gfx.DrawString($"Markings: {animal.markings}", normalFont, XBrushes.Black, new XPoint(leftMargin + 300, startY));

        //        startY += 30;
        //        gfx.DrawString("Breeder's Name: Emily Robey - TLDR", normalFont, XBrushes.Black, new XPoint(leftMargin, startY));

        //        // Draw logo if available
        //        try
        //        {
        //            XImage logo = XImage.FromFile("RATAPP/Resources/RATAPPLogo.png");
        //            gfx.DrawImage(logo, page.Width - 170, 150, 100, 100);
        //        }
        //        catch
        //        {
        //            // Logo not found - continue without it
        //        }

        //        // Draw footer
        //        var footerText = "Bred By: TLDR – AFRMA Registered Mousery & Rattery";
        //        gfx.DrawString(footerText, smallFont, XBrushes.Black,
        //            new XRect(0, page.Height - 50, page.Width, 20), XStringFormats.Center);
        //    }

        //    document.Save(outputPath);
        //}

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
                var titleFont = new XFont("Times New Roman", 24); //FIXME , XFontStyle.Bold
                var normalFont = new XFont("Times New Roman", 12);
                var smallFont = new XFont("Times New Roman", 10);

                gfx.DrawString("Certificate of Birth", titleFont, XBrushes.Black,
                    new XRect(0, 40, page.Width, 30), XStringFormats.Center);

                // Draw date of birth
                gfx.DrawString(litter.DateOfBirth.ToString(), normalFont, XBrushes.Black, //FUXNE .ToString("MM-dd-yy")
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
                var titleFont = new XFont("Times New Roman", 24); //FIXME BOLD issue
                var normalFont = new XFont("Times New Roman", 12);
                var smallFont = new XFont("Times New Roman", 10);

                gfx.DrawString("Certified Pedigree", titleFont, XBrushes.Navy,
                    new XRect(0, 40, page.Width, 30), XStringFormats.Center);

                // Draw breeder name
                gfx.DrawString("TLDR", new XFont("Times New Roman", 16), //FIXME BOLD issue 
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

    /// <summary>
    /// Service for generating PDF documents in the R.A.T. App.
    /// Handles the creation of PDF files from templates with dynamic content.
    ///
    /// Key Features:
    /// - Template-based PDF generation
    /// - Image and text integration
    /// - Custom document formatting
    ///
    /// Current Capabilities:
    /// - Background image support
    /// - Basic text placement
    /// - Single page documents
    ///
    /// Known Limitations:
    /// - Fixed text positioning
    /// - Limited formatting options
    /// - No multi-page support
    /// - Basic error handling
    ///
    /// Planned Improvements:
    /// - Implement BaseService pattern
    /// - Add dynamic text positioning
    /// - Support multiple pages
    /// - Enhanced template system
    /// - Better error handling
    ///
    /// Dependencies:
    /// - PdfSharp: For PDF generation
    /// - System.IO: For file operations
    /// </summary>
    
        /// <summary>
        /// Generates a PDF document using a template image and custom text.
        ///
        /// Process:
        /// 1. Validates input image exists
        /// 2. Creates new PDF document
        /// 3. Adds template image as background
        /// 4. Overlays text content
        /// 5. Saves to specified location
        ///
        /// Parameters:
        /// - imagePath: Path to template image
        /// - outputPath: Where to save the PDF
        /// - id: Animal ID to display
        /// - animalName: Animal name to display
        ///
        /// Text Formatting:
        /// - Uses Arial 12pt font
        /// - Fixed positions (100,150) for ID
        /// - Fixed positions (100,200) for name
        ///
        /// TODO:
        /// - Add configurable text positions
        /// - Support custom fonts and sizes
        /// - Add more dynamic content options
        ///
        /// Throws:
        /// - FileNotFoundException if template image not found
        /// </summary>
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