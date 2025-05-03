using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using System.Net.Mail;
using PdfSharp.Pdf.IO;
using RATAPP.Forms;
using RATAPP.Panels;
using static Microsoft.Azure.Amqp.Serialization.SerializableType;
using System.Xml.Linq;
using iTextSharp.text.pdf;
using Font = iTextSharp.text.Font;

namespace RATAPP.Forms
{
    public class DocumentsForm : Form
    {
        private RatAppDbContext _context;
        private string _currentUsername;
        private string _userRole;
        private RATAppBaseForm _baseForm;

        // UI Components
        private TabControl tabControl;
        private ListBox documentListBox;
        private RichTextBox documentPreviewBox;
        private Button printButton;
        private Button emailButton;
        private Button publishButton;
        private Button createNewButton;
        private ComboBox documentTypeComboBox;
        private TextBox searchBox;

        // Document management
        private Dictionary<string, List<Document>> documents;

        public static async Task<DocumentsForm> CreateAsync(
            RATAppBaseForm baseForm,
            RatAppDbContext context,
            string username,
            string role)
        {
            var panel = new DocumentsForm(baseForm, context);
            await panel.LoadDocumentsAsync();
            return panel;
        }

        public DocumentsForm(RATAppBaseForm baseForm, RatAppDbContext context)
        {
            _baseForm = baseForm;
            _context = context;
            // _currentUsername = username;
            //_userRole = role;

            this.Dock = DockStyle.Fill;
            this.Size = new System.Drawing.Size(800, 600);
            this.BackColor = Color.FromArgb(240, 240, 240);

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Page Title
            var titleLabel = new Label
            {
                Text = "Document Management",
                Font = new System.Drawing.Font("Segoe UI", 18, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(64, 64, 64),
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(300, 40),
                AutoSize = true
            };

            // Document Type Selector
            documentTypeComboBox = new ComboBox
            {
                Location = new Point(20, 70),
                Size = new Size(200, 30),
                Font = new System.Drawing.Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            documentTypeComboBox.Items.AddRange(new object[] { "All", "Care Sheets", "Birth Certificates", "Pedigrees" });
            documentTypeComboBox.SelectedIndex = 0;
            documentTypeComboBox.SelectedIndexChanged += async (s, e) => await FilterDocuments();

            // Search Box
            searchBox = new TextBox
            {
                Location = new Point(230, 70),
                Size = new Size(200, 30),
                Font = new System.Drawing.Font("Segoe UI", 11),
                PlaceholderText = "Search documents..."
            };
            searchBox.TextChanged += async (s, e) => await FilterDocuments();

            // Create New Button
            createNewButton = CreateButton("Create New", 440, 70, 120, Color.FromArgb(0, 150, 136));
            createNewButton.Click += (s, e) => CreateNewDocument();

            // Document List
            documentListBox = new ListBox
            {
                Location = new Point(20, 110),
                Size = new Size(300, this.Height - 220),
                Font = new System.Drawing.Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom
            };
            documentListBox.SelectedIndexChanged += (s, e) => UpdateDocumentPreview();

            // Document Preview
            documentPreviewBox = new RichTextBox
            {
                Location = new Point(340, 110),
                Size = new Size(this.Width - 360, this.Height - 220),
                Font = new System.Drawing.Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            // Action Buttons
            printButton = CreateButton("Print", this.Width - 360, this.Height - 100, 100, Color.FromArgb(0, 120, 215));
            printButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            printButton.Click += (s, e) => PrintDocument();

            emailButton = CreateButton("Email", this.Width - 240, this.Height - 100, 100, Color.FromArgb(255, 152, 0));
            emailButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            emailButton.Click += (s, e) => EmailDocument();

            publishButton = CreateButton("Publish", this.Width - 120, this.Height - 100, 100, Color.FromArgb(76, 175, 80));
            publishButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            publishButton.Click += (s, e) => PublishDocument();

            // Add all controls to the panel
            this.Controls.Add(titleLabel);
            this.Controls.Add(documentTypeComboBox);
            this.Controls.Add(searchBox);
            this.Controls.Add(createNewButton);
            this.Controls.Add(documentListBox);
            this.Controls.Add(documentPreviewBox);
            this.Controls.Add(printButton);
            this.Controls.Add(emailButton);
            this.Controls.Add(publishButton);
        }

        private Button CreateButton(string text, int x, int y, int width, Color color)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 30),
                Font = new System.Drawing.Font("Segoe UI", 10),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        private async Task LoadDocumentsAsync()
        {
            try
            {
                // In a real application, you would load these from your database
                documents = new Dictionary<string, List<Document>>
                {
                    ["Care Sheets"] = new List<Document>
                    {
                        new Document { Id = 1, Name = "Rat Care Guide", Type = "Care Sheets", Content = "Detailed guide for rat care..." },
                        new Document { Id = 2, Name = "Mouse Care Guide", Type = "Care Sheets", Content = "Comprehensive mouse care instructions..." }
                    },
                    ["Birth Certificates"] = new List<Document>
                    {
                        new Document { Id = 3, Name = "Litter A Birth Certificate", Type = "Birth Certificates", Content = "Birth certificate for Litter A..." },
                        new Document { Id = 4, Name = "Litter B Birth Certificate", Type = "Birth Certificates", Content = "Birth certificate for Litter B..." }
                    },
                    ["Pedigrees"] = new List<Document>
                    {
                        new Document { Id = 5, Name = "Rat 1 Pedigree", Type = "Pedigrees", Content = "Pedigree information for Rat 1..." },
                        new Document { Id = 6, Name = "Mouse 1 Pedigree", Type = "Pedigrees", Content = "Pedigree information for Mouse 1..." }
                    }
                };

                await FilterDocuments();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading documents: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task FilterDocuments()
        {
            string selectedType = documentTypeComboBox.SelectedItem.ToString();
            string searchText = searchBox.Text.ToLower();

            documentListBox.Items.Clear();

            IEnumerable<Document> filteredDocs;
            if (selectedType == "All")
            {
                filteredDocs = documents.Values.SelectMany(x => x);
            }
            else
            {
                filteredDocs = documents[selectedType];
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredDocs = filteredDocs.Where(d => d.Name.ToLower().Contains(searchText));
            }

            foreach (var doc in filteredDocs)
            {
                documentListBox.Items.Add(doc.Name);
            }
        }

        private void UpdateDocumentPreview()
        {
            if (documentListBox.SelectedItem != null)
            {
                string selectedDocName = documentListBox.SelectedItem.ToString();
                var selectedDoc = documents.Values.SelectMany(x => x).FirstOrDefault(d => d.Name == selectedDocName);
                if (selectedDoc != null)
                {
                    documentPreviewBox.Text = selectedDoc.Content;
                }
            }
            else
            {
                documentPreviewBox.Text = "";
            }
        }

        private void PrintDocument()
        {
            if (documentListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a document to print.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // In a real application, you would implement actual printing logic here
            MessageBox.Show($"Printing document: {documentListBox.SelectedItem}", "Print", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void EmailDocument()
        {
            if (documentListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a document to email.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // In a real application, you would implement email sending logic here
            MessageBox.Show($"Emailing document: {documentListBox.SelectedItem}", "Email", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PublishDocument()
        {
            if (documentListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a document to publish.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // In a real application, you would implement publishing logic here
            MessageBox.Show($"Publishing document: {documentListBox.SelectedItem}", "Publish", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CreateNewDocument()
        {
            // Open a new form or dialog to create a new document
            using (var createDocForm = new CreateDocumentForm())
            {
                if (createDocForm.ShowDialog() == DialogResult.OK)
                {
                    // Add the new document to the appropriate list
                    if (!documents.ContainsKey(createDocForm.DocumentType))
                    {
                        documents[createDocForm.DocumentType] = new List<Document>();
                    }
                    documents[createDocForm.DocumentType].Add(new Document
                    {
                        Id = documents.Values.SelectMany(x => x).Max(d => d.Id) + 1,
                        Name = createDocForm.DocumentName,
                        Type = createDocForm.DocumentType,
                        Content = createDocForm.DocumentContent
                    });

                    // Refresh the document list
                    FilterDocuments();
                }
            }
        }
    }

    public class Document
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
    }

    public class CreateDocumentForm : Form
    {
        private TextBox nameTextBox;
        private ComboBox typeComboBox;
        private RichTextBox contentRichTextBox;
        private Button createButton;
        private Button cancelButton;

        public string DocumentName { get; private set; }
        public string DocumentType { get; private set; }
        public string DocumentContent { get; private set; }

        public CreateDocumentForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Create New Document";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            var nameLabel = new Label
            {
                Text = "Document Name:",
                Location = new Point(20, 20),
                AutoSize = true
            };

            nameTextBox = new TextBox
            {
                Location = new Point(20, 40),
                Size = new Size(340, 20)
            };

            var typeLabel = new Label
            {
                Text = "Document Type:",
                Location = new Point(20, 70),
                AutoSize = true
            };

            typeComboBox = new ComboBox
            {
                Location = new Point(20, 90),
                Size = new Size(340, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            typeComboBox.Items.AddRange(new object[] { "Care Sheets", "Birth Certificates", "Pedigrees" });

            var contentLabel = new Label
            {
                Text = "Content:",
                Location = new Point(20, 120),
                AutoSize = true
            };

            contentRichTextBox = new RichTextBox
            {
                Location = new Point(20, 140),
                Size = new Size(340, 250)
            };

            createButton = new Button
            {
                Text = "Create",
                Location = new Point(180, 400),
                Size = new Size(80, 30),
                DialogResult = DialogResult.OK
            };
            createButton.Click += (s, e) => CreateDocument();

            cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(280, 400),
                Size = new Size(80, 30),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.AddRange(new Control[] { nameLabel, nameTextBox, typeLabel, typeComboBox, contentLabel, contentRichTextBox, createButton, cancelButton });
        }

        private void CreateDocument()
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text) || typeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DocumentName = nameTextBox.Text;
            DocumentType = typeComboBox.SelectedItem.ToString();
            DocumentContent = contentRichTextBox.Text;
        }
    }

    //TODO this logic should be moved to the library 
    public static class PdfTemplatePopulator
    {
        public static void CreateBirthCertificate(string outputPath, Dictionary<string, string> data)
        {
            iTextSharp.text.Document document = new iTextSharp.text.Document();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outputPath, FileMode.Create));
            document.Open();

            // Replace the line causing the error
            Font titleFont = FontFactory.GetFont("Helvetica-Bold", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, 18);
            Paragraph title = new Paragraph("Birth Certificate", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);

            // Add content
            Font contentFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);
            foreach (var field in data)
            {
                Paragraph paragraph = new Paragraph($"{field.Key}: {field.Value}", contentFont);
                document.Add(paragraph);
            }

            document.Close();
        }
    }
}

// Usage example:
// var birthCertificateData = new Dictionary<string, string>
// {
//     { "AnimalName", "Whiskers" },
//     { "Species", "Rat" },
//     { "DateOfBirth", "2023-05-15" },
//     { "MotherName", "Luna" },
//     { "FatherName", "Apollo" },
//     { "BreederName", "John Doe" }
// };
// 
// // To populate a template:
// PdfTemplatePopulator.PopulateBirthCertificate("path/to/template.pdf", "path/to/output.pdf", birthCertificateData);
// 
// // To create a new birth certificate:
// PdfTemplatePopulator.CreateBirthCertificate("path/to/new_certificate.pdf", birthCertificateData);

//This DocumentsPanel provides the following features:

//1.A list of documents categorized by type (Care Sheets, Birth Certificates, Pedigrees).
//2. Ability to filter documents by type and search by name.
//3. A preview pane to view the content of selected documents.
//4. Buttons for printing, emailing, and publishing documents (placeholder functionality).
//5. A "Create New" button that opens a form to create new documents.
//6.A PdfTemplatePopulator class that can both populate existing PDF templates and create new birth certificates from scratch.

//To use this panel in your main application, you would add it to your main form's content panel similar to how you're using other panels:

//```csharp
//var documentsPanel = await DocumentsPanel.CreateAsync(baseForm, _context, username, role);
//baseForm.contentPanel.Controls.Clear();
//baseForm.contentPanel.Controls.Add(documentsPanel);
//baseForm.SetActivePanel(documentsPanel);