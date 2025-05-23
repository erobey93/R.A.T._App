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
            InitializePedigreeTemplate();
        }

        private void InitializePedigreeTemplate()
        {
            var pedigreeTemplate = new PdfTemplate
            {
                Name = "PedigreeCertificate",
                PageSize = PageSize.A4,
                Margins = new PdfMargin { Left = 20, Right = 20, Top = 20, Bottom = 50 },
                Elements = new List<PdfElement>
        {
            // Title
            new TextElement {
                Type = "Text",
                Text = "Pedigree",
                FontFamily = "Times New Roman",
                FontSize = 24,
                IsBold = true,
                X = 0, Y = 40,
                Width = 595, Height = 30,
                HorizontalAlignment = HorizontalAlignment.Center
            },

            // Breeder Label
            new TextElement {
                Type = "Text",
                Text = "Breeder:)",
                FontFamily = "Times New Roman",
                FontSize = 14,
                IsBold = true,
                X = 50, Y = 90,
                Width = 100, Height = 20
            },
            new TextElement {
                Type = "Text",
                //DataField = "",
                Text = "Top Left Dream Rodentry (TLDR)", //FIXME
                FontFamily = "Times New Roman",
                FontSize = 14,
                X = 150, Y = 90,
                Width = 400, Height = 20
            },

            // Name Label
            new TextElement {
                Type = "Text",
                Text = "Name:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 50, Y = 120,
                Width = 60, Height = 20
            },
            new TextElement {
                Type = "Text",
                DataField = "Animal.name",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 110, Y = 120,
                Width = 300, Height = 20
            },

            // Registration Label
            new TextElement {
                Type = "Text",
                Text = "Registration:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 50, Y = 150,
                Width = 100, Height = 20
            },
            new TextElement {
                Type = "Text",
                DataField = "Animal.regNum",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 150, Y = 150,
                Width = 300, Height = 20
            },

            // Date of Birth and Variety
            new TextElement {
                Type = "Text",
                Text = "Date of Birth:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 50, Y = 180,
                Width = 100, Height = 20
            },
            new TextElement {
                Type = "Text",
                DataField = "Animal.DateOfBirth",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 150, Y = 180,
                Width = 150, Height = 20
            },
            new TextElement {
                Type = "Text",
                Text = "Variety:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 350, Y = 180,
                Width = 60, Height = 20
            },
            new TextElement {
                Type = "Text",
                //DataField = "Animal.Variety", TODO - NESTED FIXME
                Text = " TODO - nested",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 410, Y = 180,
                Width = 150, Height = 20
            },

            // Section Headers
            new TextElement {
                Type = "Text",
                Text = "Parents",
                FontFamily = "Times New Roman",
                FontSize = 14,
                IsBold = true,
                X = 50, Y = 220,
                Width = 200, Height = 25
            },
            new TextElement {
                Type = "Text",
                Text = "Grandparents",
                FontFamily = "Times New Roman",
                FontSize = 14,
                IsBold = true,
                X = 300, Y = 220,
                Width = 200, Height = 25
            },

            // Parents Section
            // Dam
            new TextElement {
                Type = "Text",
                Text = "Dam:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 50, Y = 250,
                Width = 40, Height = 20
            },
            new TextElement {
                Type = "Text",
                //DataField = "Ancestors.Dam.ancestor.Name", TODO nested
                Text= "TODO-nested",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 90, Y = 250,
                Width = 200, Height = 20
            },

            // Sire
            new TextElement {
                Type = "Text",
                Text = "Sire:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 50, Y = 280,
                Width = 40, Height = 20
            },
            new TextElement {
                Type = "Text",
                //DataField = "Ancestors.Sire.ancestor.Name", FIXME TODO
                Text= "TODO-nested",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 90, Y = 280,
                Width = 200, Height = 20
            },

            // Grandparents Section
            // Dam's Dam
            new TextElement {
                Type = "Text",
                Text = "Dam's Dam:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 300, Y = 250,
                Width = 80, Height = 20
            },
            new TextElement {
                Type = "Text",
                //DataField = "Ancestors.Dam's Dam.ancestor.Name",
                Text = "Grandparent 1 - Dam's Dam",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 380, Y = 250,
                Width = 200, Height = 20
            },

            // Dam's Sire
            new TextElement {
                Type = "Text",
                Text = "Dam's Sire:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 300, Y = 280,
                Width = 80, Height = 20
            },
            new TextElement {
                Type = "Text",
                //DataField = "Ancestors.Dam's Sire.ancestor.Name", TODO nested
                Text = "Grandparent 2 - Dam's Sire",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 380, Y = 280,
                Width = 200, Height = 20
            },

            // Sire's Dam
            new TextElement {
                Type = "Text",
                Text = "Sire's Dam:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 300, Y = 310,
                Width = 80, Height = 20
            },
            new TextElement {
                Type = "Text",
                //DataField = "Ancestors.Sire's Dam.ancestor.Name", FIXME
                Text = "Grandparent 3 - Sire's Dam",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 380, Y = 310,
                Width = 200, Height = 20
            },

            // Sire's Sire
            new TextElement {
                Type = "Text",
                Text = "Sire's Sire:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 300, Y = 340,
                Width = 80, Height = 20
            },
            new TextElement {
                Type = "Text",
                //DataField = "Ancestors.Sire's Sire.ancestor.Name", TODO nested 
                Text = "Grandparent 4 - Sire's Sire",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 380, Y = 340,
                Width = 200, Height = 20
            }, 
             // Footer with generation date (matches first version)
            new TextElement
            {
                Type = "Text",
                DataField = "GenerationDate",
                FontFamily = "Times New Roman",
                FontSize = 10,
                Color = "#808080", // Gray color
                X = 0,
                Y = 800, // Position at bottom of page
                Width = 595,
                Height = 20,
                HorizontalAlignment = HorizontalAlignment.Center
            }
            },

                DefaultValues = new Dictionary<string, string>
        {
            { "FooterText", $"Generated by R.A.T. App on {DateTime.Now:MM/dd/yyyy}" }
        }
            };

            _templates.Add(pedigreeTemplate.Name, pedigreeTemplate);
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