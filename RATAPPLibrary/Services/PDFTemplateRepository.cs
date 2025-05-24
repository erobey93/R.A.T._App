using PdfSharp.Charting;
using PdfSharp;
using PdfSharp.Drawing;

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

        //private void InitializePedigreeTemplate()
        //{
        //    // For an A4 page (595 x 842 points)
        //    const float pageHeight = 842;
        //    const float pageWidth = 595;
        //    const float footerYPosition = pageHeight - 40; // 40 points from bottom
        //    const float logoWidth = 100; // Adjust logo width as needed
        //    const float logoHeight = 100; // Adjust logo height as needed

        //    var pedigreeTemplate = new PdfTemplate
        //    {
        //        Name = "PedigreeCertificate",
        //        PageSize = PageSize.A4,
        //        Margins = new PdfMargin { Left = 20, Right = 20, Top = 20, Bottom = 50 },
        //        Elements = new List<PdfElement>
        //{
        //    //Logo centered at top TODO - static, needs to be dynamic
        //    new ImageElement
        //    {
        //        Type = "Image",
        //        ImagePath = "RATAPP/Resources/RATAPPLogo.png", // Path to your logo
        //        X = (pageWidth - logoWidth) / 2, // Horizontally centered
        //        Y = 20, // 20 points from top (matches top margin)
        //        Width = logoWidth,
        //        Height = logoHeight
        //    },

        //    // Title - TODO would be nice to somehow incorporate club, breeder, etc 
        //    new TextElement
        //    {
        //        Type = "Text",
        //        Text = "Certified Pedigree",
        //        FontFamily = "Times New Roman",
        //        FontSize = 24,
        //        IsBold = true,
        //        X = 0,
        //        Y = 20 + logoHeight + 10, // Below logo + 10pt spacing
        //        Width = pageWidth,
        //        Height = 30,
        //        HorizontalAlignment = HorizontalAlignment.Center
        //    },


        //    // Breeder Label
        //    new TextElement {
        //        Type = "Text",
        //        Text = "Breeder:)",
        //        FontFamily = "Times New Roman",
        //        FontSize = 14,
        //        IsBold = true,
        //        X = 50, Y = 90,
        //        Width = 100, Height = 20
        //    },
        //    new TextElement {
        //        Type = "Text",
        //        //DataField = "",
        //        Text = "Top Left Dream Rodentry (TLDR)", //FIXME
        //        FontFamily = "Times New Roman",
        //        FontSize = 14,
        //        X = 150, Y = 90,
        //        Width = 400, Height = 20
        //    },

        //    // Name Label
        //    new TextElement {
        //        Type = "Text",
        //        Text = "Name:",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        IsBold = true,
        //        X = 50, Y = 120,
        //        Width = 60, Height = 20
        //    },
        //    new TextElement {
        //        Type = "Text",
        //        DataField = "Animal.name",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        X = 110, Y = 120,
        //        Width = 300, Height = 20
        //    },

        //    // Registration Label
        //    new TextElement {
        //        Type = "Text",
        //        Text = "Registration:",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        IsBold = true,
        //        X = 50, Y = 150,
        //        Width = 100, Height = 20
        //    },
        //    new TextElement {
        //        Type = "Text",
        //        DataField = "Animal.regNum",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        X = 150, Y = 150,
        //        Width = 300, Height = 20
        //    },

        //    // Date of Birth and Variety
        //    new TextElement {
        //        Type = "Text",
        //        Text = "Date of Birth:",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        IsBold = true,
        //        X = 50, Y = 180,
        //        Width = 100, Height = 20
        //    },
        //    new TextElement {
        //        Type = "Text",
        //        DataField = "Animal.DateOfBirth",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        X = 150, Y = 180,
        //        Width = 150, Height = 20
        //    },
        //    new TextElement {
        //        Type = "Text",
        //        Text = "Variety:",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        IsBold = true,
        //        X = 350, Y = 180,
        //        Width = 60, Height = 20
        //    },
        //    new TextElement {
        //        Type = "Text",
        //        //DataField = "Animal.Variety", TODO - NESTED FIXME
        //        Text = " TODO - nested",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        X = 410, Y = 180,
        //        Width = 150, Height = 20
        //    },

        //    // Section Headers
        //    new TextElement {
        //        Type = "Text",
        //        Text = "Parents",
        //        FontFamily = "Times New Roman",
        //        FontSize = 14,
        //        IsBold = true,
        //        X = 50, Y = 220,
        //        Width = 200, Height = 25
        //    },
        //    new TextElement {
        //        Type = "Text",
        //        Text = "Grandparents",
        //        FontFamily = "Times New Roman",
        //        FontSize = 14,
        //        IsBold = true,
        //        X = 300, Y = 220,
        //        Width = 200, Height = 25
        //    },

        //    // Parents Section
        //    // Dam
        //    new TextElement {
        //        Type = "Text",
        //        Text = "Dam:",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        IsBold = true,
        //        X = 50, Y = 250,
        //        Width = 40, Height = 20
        //    },
        //    new TextElement {
        //        Type = "Text",
        //        //DataField = "Ancestors.Dam.ancestor.Name", TODO nested
        //        Text= "TODO-nested",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        X = 90, Y = 250,
        //        Width = 200, Height = 20
        //    },

        //    // Sire
        //    new TextElement {
        //        Type = "Text",
        //        Text = "Sire:",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        IsBold = true,
        //        X = 50, Y = 280,
        //        Width = 40, Height = 20
        //    },
        //    new TextElement {
        //        Type = "Text",
        //        //DataField = "Ancestors.Sire.ancestor.Name", FIXME TODO
        //        Text= "TODO-nested",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        X = 90, Y = 280,
        //        Width = 200, Height = 20
        //    },

        //    // Grandparents Section
        //    // Dam's Dam
        //    new TextElement {
        //        Type = "Text",
        //        Text = "Dam's Dam:",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        IsBold = true,
        //        X = 300, Y = 250,
        //        Width = 80, Height = 20
        //    },
        //    new TextElement {
        //        Type = "Text",
        //        //DataField = "Ancestors.Dam's Dam.ancestor.Name",
        //        Text = "Grandparent 1 - Dam's Dam",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        X = 380, Y = 250,
        //        Width = 200, Height = 20
        //    },

        //    // Dam's Sire
        //    new TextElement {
        //        Type = "Text",
        //        Text = "Dam's Sire:",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        IsBold = true,
        //        X = 300, Y = 280,
        //        Width = 80, Height = 20
        //    },
        //    new TextElement {
        //        Type = "Text",
        //        //DataField = "Ancestors.Dam's Sire.ancestor.Name", TODO nested
        //        Text = "Grandparent 2 - Dam's Sire",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        X = 380, Y = 280,
        //        Width = 200, Height = 20
        //    },

        //    // Sire's Dam
        //    new TextElement {
        //        Type = "Text",
        //        Text = "Sire's Dam:",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        IsBold = true,
        //        X = 300, Y = 310,
        //        Width = 80, Height = 20
        //    },
        //    new TextElement {
        //        Type = "Text",
        //        //DataField = "Ancestors.Sire's Dam.ancestor.Name", FIXME
        //        Text = "Grandparent 3 - Sire's Dam",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        X = 380, Y = 310,
        //        Width = 200, Height = 20
        //    },

        //    // Sire's Sire
        //    new TextElement {
        //        Type = "Text",
        //        Text = "Sire's Sire:",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        IsBold = true,
        //        X = 300, Y = 340,
        //        Width = 80, Height = 20
        //    },
        //    new TextElement {
        //        Type = "Text",
        //        //DataField = "Ancestors.Sire's Sire.ancestor.Name", TODO nested 
        //        Text = "Grandparent 4 - Sire's Sire",
        //        FontFamily = "Times New Roman",
        //        FontSize = 12,
        //        X = 380, Y = 340,
        //        Width = 200, Height = 20
        //    }, 
        //    // Footer - using explicit page dimensions
        //    new TextElement
        //    {
        //        Type = "Text",
        //        DataField = "FooterText",
        //        Text = "Generated by R.A.T. App", // Default if data missing
        //        FontFamily = "Times New Roman",
        //        FontSize = 10,
        //        Color = "#808080",
        //        X = 0,
        //        Y = footerYPosition, // Now using pre-calculated position
        //        Width = pageWidth,  // Full page width
        //        Height = 20,
        //        HorizontalAlignment = HorizontalAlignment.Center
        //    }
        //},
        //        DefaultValues = new Dictionary<string, string>
        //{
        //    { "FooterText", $"Generated by R.A.T. App on {DateTime.Now:MM/dd/yyyy}" }
        //}
        //    };

        //    _templates.Add(pedigreeTemplate.Name, pedigreeTemplate);
        //}

        private void InitializePedigreeTemplate()
{
    const float pageHeight = 842; // A4 height in points
    const float pageWidth = 595;  // A4 width in points
    const float logoWidth = 100;
    const float logoHeight = 100;
    const float footerYPosition = pageHeight - 40;

    var pedigreeTemplate = new PdfTemplate
    {
        Name = "PedigreeCertificate",
        PageSize = PageSize.A4,
        Margins = new PdfMargin { Left = 20, Right = 20, Top = 20, Bottom = 50 },
        Elements = new List<PdfElement>
        {
            // --- Centered Logo ---
            new ImageElement
            {
                Type = "Image",
                ImagePath = "C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\DALL·E 2024-01-29 19.13.34 - Create another logo for 'TLDR', a mousery and rattery in Portland, Oregon, similar to the previous design but with an emphasis on clearer visibility o.jpg",
                X = (pageWidth - logoWidth) / 2,
                Y = 25,
                Width = logoWidth,
                Height = logoHeight
            },

            // --- Title (with breeder/club info) ---
            new TextElement
            {
                Type = "Text",
                DataField = "CertificateTitle",
                Text = "CERTIFIED PEDIGREE", // Default if data missing
                FontFamily = "Times New Roman",
                FontSize = 24,
                IsBold = true,
                X = 0,
                Y = 25 + logoHeight + 10, // Below logo
                Width = pageWidth,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Center,
                Color = "#000080" // Navy blue
            },

            // --- Breeder Info ---
            new TextElement {
                Type = "Text",
                Text = "Breeder:",
                FontFamily = "Times New Roman",
                FontSize = 14,
                IsBold = true,
                X = 50, Y = 160, // Adjusted Y position
                Width = 100, Height = 20
            },
            new TextElement {
                Type = "Text",
                DataField = "Animal.Breeder",
                Text = "Top Left Dream Rodentry (TLDR)", // Fallback
                FontFamily = "Times New Roman",
                FontSize = 14,
                X = 150, Y = 160,
                Width = 400, Height = 20
            },

            // --- Animal Details Section ---
            // Name
            new TextElement {
                Type = "Text",
                Text = "Name:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 50, Y = 190,
                Width = 60, Height = 20
            },
            new TextElement {
                Type = "Text",
                DataField = "Animal.Name",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 110, Y = 190,
                Width = 300, Height = 20
            },

            // Registration
            new TextElement {
                Type = "Text",
                Text = "Registration:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 50, Y = 220,
                Width = 100, Height = 20
            },
            new TextElement {
                Type = "Text",
                DataField = "Animal.RegNum",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 150, Y = 220,
                Width = 300, Height = 20
            },

            // Date of Birth & Variety
            new TextElement {
                Type = "Text",
                Text = "Date of Birth:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 50, Y = 250,
                Width = 100, Height = 20
            },
            new TextElement {
                Type = "Text",
                DataField = "Animal.DateOfBirth",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 150, Y = 250,
                Width = 150, Height = 20
            },
            new TextElement {
                Type = "Text",
                Text = "Variety:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 350, Y = 250,
                Width = 60, Height = 20
            },
            new TextElement {
                Type = "Text",
                DataField = "Animal.Variety",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 410, Y = 250,
                Width = 150, Height = 20
            },

            // --- Pedigree Sections ---
            // Parents Header
            new TextElement {
                Type = "Text",
                Text = "PARENTS",
                FontFamily = "Times New Roman",
                FontSize = 14,
                IsBold = true,
                X = 50, Y = 290,
                Width = 200, Height = 25
            },

            // Grandparents Header
            new TextElement {
                Type = "Text",
                Text = "GRANDPARENTS",
                FontFamily = "Times New Roman",
                FontSize = 14,
                IsBold = true,
                X = 300, Y = 290,
                Width = 200, Height = 25
            },

            // --- Parents ---
            // Dam
            new TextElement {
                Type = "Text",
                Text = "Dam:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 50, Y = 320,
                Width = 40, Height = 20
            },
            new TextElement {
                Type = "Text",
                DataField = "Ancestors.Dam.ancestor.Name",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 90, Y = 320,
                Width = 200, Height = 20
            },

            // Sire
            new TextElement {
                Type = "Text",
                Text = "Sire:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 50, Y = 350,
                Width = 40, Height = 20
            },
            new TextElement {
                Type = "Text",
                DataField = "Ancestors.Sire.ancestor.Name",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 90, Y = 350,
                Width = 200, Height = 20
            },

            // --- Grandparents ---
            // Dam's Dam
            new TextElement {
                Type = "Text",
                Text = "Dam's Dam:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 300, Y = 320,
                Width = 80, Height = 20
            },
            new TextElement {
                Type = "Text",
                DataField = "Ancestors.Dam's Dam.ancestor.Name",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 380, Y = 320,
                Width = 200, Height = 20
            },

            // Dam's Sire
            new TextElement {
                Type = "Text",
                Text = "Dam's Sire:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 300, Y = 350,
                Width = 80, Height = 20
            },
            new TextElement {
                Type = "Text",
                DataField = "Ancestors.Dam's Sire.ancestor.Name",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 380, Y = 350,
                Width = 200, Height = 20
            },

            // Sire's Dam
            new TextElement {
                Type = "Text",
                Text = "Sire's Dam:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 300, Y = 380,
                Width = 80, Height = 20
            },
            new TextElement {
                Type = "Text",
                DataField = "Ancestors.Sire's Dam.ancestor.Name",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 380, Y = 380,
                Width = 200, Height = 20
            },

            // Sire's Sire
            new TextElement {
                Type = "Text",
                Text = "Sire's Sire:",
                FontFamily = "Times New Roman",
                FontSize = 12,
                IsBold = true,
                X = 300, Y = 410,
                Width = 80, Height = 20
            },
            new TextElement {
                Type = "Text",
                DataField = "Ancestors.Sire's Sire.ancestor.Name",
                FontFamily = "Times New Roman",
                FontSize = 12,
                X = 380, Y = 410,
                Width = 200, Height = 20
            },

            // --- Footer ---
            new TextElement
            {
                Type = "Text",
                DataField = "FooterText",
                Text = "Generated by R.A.T. App", // Default
                FontFamily = "Times New Roman",
                FontSize = 10,
                Color = "#808080",
                X = 0,
                Y = footerYPosition,
                Width = pageWidth,
                Height = 20,
                HorizontalAlignment = HorizontalAlignment.Center
            }
        },
        DefaultValues = new Dictionary<string, string>
        {
            { "FooterText", $"Generated by R.A.T. App on {DateTime.Now:MM/dd/yyyy}" },
            { "CertificateTitle", "CERTIFIED PEDIGREE" }
        }
    };

    _templates.Add(pedigreeTemplate.Name, pedigreeTemplate);
}
        private void InitializeTemplates()
        {
            // For an A4 page (595 x 842 points)
            const float pageHeight = 842;
            const float pageWidth = 595;
            const float footerYPosition = pageHeight - 40; // 40 points from bottom

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
                
            // Footer - Corrected implementation
            new TextElement
            {
                Type = "Text",
                DataField = "FooterText", // Changed from GenerationDate to FooterText
                Text = "Generated by R.A.T. App", // Default text if data missing
                FontFamily = "Times New Roman",
                FontSize = 10,
                Color = "#808080",
                X = 0,
                Y = pageHeight, // Positions at bottom of page
                Width = pageWidth,
                Height = 20,
                HorizontalAlignment = HorizontalAlignment.Center
            }
        },
                DefaultValues = new Dictionary<string, string>
        {
            { "FooterText", $"Generated by R.A.T. App on {DateTime.Now:MM/dd/yyyy}" }
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

        private void RenderImageElement(XGraphics gfx, ImageElement element, object data)
        {
            try
            {
                string imagePath = element.ImagePath;
                if (File.Exists(imagePath))
                {
                    XImage logo = XImage.FromFile(imagePath);
                    gfx.DrawImage(logo, element.X, element.Y, element.Width, element.Height);
                }
            }
            catch
            {
                // Skip if logo not found
            }
        }
    }
}