using PdfSharp.Drawing;
using PdfSharp.Pdf;
using static System.Net.Mime.MediaTypeNames;

namespace RATAPPLibrary.Services
{
    public class PdfService  //: BaseService TODO use new BaseService + context factory pattern 
    {
        public interface IPdfService
        {
            void GeneratePdfFromTemplate(string imagePath, string outputPath, string id, string animalName);
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
