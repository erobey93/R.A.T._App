using PdfSharp.Charting;
using PdfSharp;

namespace RATAPPLibrary.Services
{
    public class PdfTemplateRepository
    {
        private readonly Dictionary<string, PdfTemplate> _templates = new Dictionary<string, PdfTemplate>();

        public PdfTemplateRepository()
        {
            InitializeTemplates();
        }

        private void InitializeTemplates()
        {
            // Birth Certificate for Animal
            var birthCertTemplate = new PdfTemplate
            {
                Name = "BirthCertificateAnimal",
                PageSize = PageSize.A4,
                Margins = new PdfMargin { Left = 20, Right = 20, Top = 20, Bottom = 50 },
                Elements = new List<PdfElement>
            {
                // Border
                new TextElement {
                    Type = "Border",
                    X = 0, Y = 0, Width = 100, Height = 100,
                    Text = "" // Border is drawn differently
                },
                
                // Title
                new TextElement {
                    Type = "Text",
                    Name = "Title",
                    Text = "Certificate of Birth",
                    FontFamily = "Times New Roman",
                    FontSize = 24,
                    IsBold = true,
                    X = 0, Y = 40, Width = 595, Height = 30,
                    HorizontalAlignment = HorizontalAlignment.Center
                },
                
                // Date of Birth
                new TextElement {
                    Type = "Text",
                    Name = "DateValue",
                    DataField = "DateOfBirth",
                    FontFamily = "Times New Roman",
                    FontSize = 12,
                    X = 0, Y = 80, Width = 595, Height = 20,
                    HorizontalAlignment = HorizontalAlignment.Center
                },
                new TextElement {
                    Type = "Text",
                    Name = "DateLabel",
                    Text = "Date of Birth",
                    FontFamily = "Times New Roman",
                    FontSize = 12,
                    IsBold = true,
                    X = 0, Y = 100, Width = 595, Height = 20,
                    HorizontalAlignment = HorizontalAlignment.Center
                },
                
                // Name/ID
                new TextElement {
                    Type = "Text",
                    Name = "NameLabel",
                    Text = "Name/ID:",
                    FontFamily = "Times New Roman",
                    FontSize = 12,
                    IsBold = true,
                    X = 50, Y = 150, Width = 80, Height = 20
                },
                new TextElement {
                    Type = "Text",
                    Name = "NameValue",
                    DataField = "Name",
                    FontFamily = "Times New Roman",
                    FontSize = 12,
                    X = 130, Y = 150, Width = 200, Height = 20
                },
                
                // Sex
                new TextElement {
                    Type = "Text",
                    Name = "SexLabel",
                    Text = "Sex:",
                    FontFamily = "Times New Roman",
                    FontSize = 12,
                    IsBold = true,
                    X = 350, Y = 150, Width = 40, Height = 20
                },
                new TextElement {
                    Type = "Text",
                    Name = "SexValue",
                    DataField = "Sex",
                    FontFamily = "Times New Roman",
                    FontSize = 12,
                    X = 390, Y = 150, Width = 50, Height = 20
                },
                
                // Continue with all other fields...
                
                // Logo
                new ImageElement {
                    Type = "Image",
                    Name = "Logo",
                    ImagePath = "RATAPP/Resources/RATAPPLogo.png",
                    X = 425, Y = 150, Width = 100, Height = 100
                },
                
                // Footer
                new TextElement {
                    Type = "Text",
                    Name = "Footer",
                    Text = "Bred By: TLDR – AFRMA Registered Mousery & Rattery",
                    FontFamily = "Times New Roman",
                    FontSize = 10,
                    X = 0, Y = 800, Width = 595, Height = 20,
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            },
                DefaultValues = new Dictionary<string, string>
            {
                { "BreederName", "Emily Robey - TLDR" }
            }
            };

            _templates.Add(birthCertTemplate.Name, birthCertTemplate);

            // Add other templates (LitterCertificate, PedigreeCertificate) similarly
        }

        public PdfTemplate GetTemplate(string templateName)
        {
            if (_templates.TryGetValue(templateName, out var template))
            {
                return template;
            }
            throw new KeyNotFoundException($"Template '{templateName}' not found.");
        }
    }
}