using PdfSharp.Drawing;
using PdfSharp.Pdf;
using static System.Net.Mime.MediaTypeNames;

namespace RATAPPLibrary.Services
{
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
    public class PdfService  //: BaseService TODO use new BaseService + context factory pattern 
    {
        /// <summary>
        /// Interface defining PDF generation operations.
        /// Allows for dependency injection and testing.
        /// </summary>
        {
            void GeneratePdfFromTemplate(string imagePath, string outputPath, string id, string animalName);
        }

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
