using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using RATAPP.Forms;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;

namespace RATAPP.Forms
{
    public class IndividualAnimalAncestryForm : Form
    {
        private RatAppDbContext _context;
        private AnimalDto _currentAnimal; // Assuming Rat is your animal model
        private RATAppBaseForm _baseForm;

        // UI Components
        private Label titleLabel;
        private Label animalIdLabel;
        private Label animalNameLabel;
        private ListBox ancestryListBox;
        private Button addParentButton;
        private Button viewFamilyTreeButton;
        private Button viewPedigreeButton;
        private Panel familyTreePanel; // Placeholder for family tree display
        private RichTextBox pedigreeTextBox; // Placeholder for pedigree display

        public static IndividualAnimalAncestryForm Create(RATAppBaseForm baseForm, RatAppDbContextFactory contextFactory, AnimalDto animal)
        {
            return new IndividualAnimalAncestryForm(baseForm, contextFactory, animal);
        }

        public IndividualAnimalAncestryForm(RATAppBaseForm baseForm, RatAppDbContextFactory contextFactory, AnimalDto animal)
        {
            _baseForm = baseForm;
            //_context = context;
            _currentAnimal = animal;

            this.Dock = DockStyle.Fill;
            this.Size = new Size(800, 600);
            this.BackColor = Color.FromArgb(240, 240, 240);

            InitializeComponents();
            LoadAncestry();
        }

        private void InitializeComponents()
        {
            // Page Title
            titleLabel = new Label
            {
                Text = "Animal Ancestry",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64),
                Location = new Point(20, 20),
                Size = new Size(300, 40),
                AutoSize = true
            };

            // Animal ID Label
            animalIdLabel = new Label
            {
                Text = $"Animal ID: {_currentAnimal.regNum}", //TODO or ID, gotta make a decision here 
                Font = new Font("Segoe UI", 11),
                Location = new Point(20, 70),
                Size = new Size(200, 30),
                AutoSize = true
            };

            // Animal Name Label
            animalNameLabel = new Label
            {
                Text = $"Animal Name: {_currentAnimal.name}",
                Font = new Font("Segoe UI", 11),
                Location = new Point(20, 100),
                Size = new Size(200, 30),
                AutoSize = true
            };

            // Ancestry List Box
            ancestryListBox = new ListBox
            {
                Location = new Point(20, 140),
                Size = new Size(300, 300),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom
            };

            // Add Parent Button
            addParentButton = CreateButton("Add Parent", 340, 140, 120, Color.FromArgb(0, 150, 136));
            addParentButton.Click += AddParentButton_Click;

            // View Family Tree Button
            viewFamilyTreeButton = CreateButton("View Family Tree", 340, 180, 150, Color.FromArgb(0, 120, 215));
            viewFamilyTreeButton.Click += ViewFamilyTreeButton_Click;

            // View Pedigree Button
            viewPedigreeButton = CreateButton("View Pedigree", 340, 220, 150, Color.FromArgb(255, 152, 0));
            viewPedigreeButton.Click += ViewPedigreeButton_Click;

            // Birth Certificate Button
            var birthCertButton = CreateButton("Birth Certificate", 340, 260, 150, Color.FromArgb(0, 150, 136));
            birthCertButton.Click += BirthCertButton_Click;

            // Family Tree Panel (Placeholder)
            familyTreePanel = new Panel
            {
                Location = new Point(340, 260),
                Size = new Size(440, 300),
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Visible = false
            };

            // Pedigree Text Box (Placeholder)
            pedigreeTextBox = new RichTextBox
            {
                Location = new Point(340, 260),
                Size = new Size(440, 300),
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Visible = false
            };

            // Add all controls to the form
            this.Controls.Add(titleLabel);
            this.Controls.Add(animalIdLabel);
            this.Controls.Add(animalNameLabel);
            this.Controls.Add(ancestryListBox);
            this.Controls.Add(addParentButton);
            this.Controls.Add(viewFamilyTreeButton);
            this.Controls.Add(viewPedigreeButton);
            this.Controls.Add(familyTreePanel);
            this.Controls.Add(pedigreeTextBox);
        }

        private Button CreateButton(string text, int x, int y, int width, Color color)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 30),
                Font = new Font("Segoe UI", 10),
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        private void LoadAncestry()
        {
            ancestryListBox.Items.Clear();
            // TODO: Load ancestry from _context and populate ancestryListBox
            // Example placeholder data:
            ancestryListBox.Items.Add("Parent 1 (ID: 101)");
            ancestryListBox.Items.Add("Parent 2 (ID: 102)");
        }

        private void AddParentButton_Click(object sender, EventArgs e)
        {
            // TODO: Implement logic to add a parent to the current animal
            MessageBox.Show("Add Parent functionality to be implemented.");
            // Open a new form or dialog to select/create a parent animal
        }

        private void ViewFamilyTreeButton_Click(object sender, EventArgs e)
        {
            // TODO: Implement logic to display the family tree
            MessageBox.Show("View Family Tree functionality to be implemented.");
            familyTreePanel.Visible = true;
            pedigreeTextBox.Visible = false;
            // Use a graphical library or custom drawing to display the tree in familyTreePanel
        }

        private void ViewPedigreeButton_Click(object sender, EventArgs e)
        {
            var pedigreeForm = PedigreeForm.Create(_baseForm.ContextFactory, _currentAnimal);
            pedigreeForm.ShowDialog();
        }

        private void BirthCertButton_Click(object sender, EventArgs e)
        {
            var birthCertForm = BirthCertificateForm.CreateForAnimal(_baseForm.ContextFactory, _currentAnimal);
            birthCertForm.ShowDialog();
        }
    }
}
//namespace RATAPP.Panels
//{
//    public partial class IndividualAncestryPanel : Form, INavigable
//    {
//        private RATAppBaseForm _baseForm;
//        private RatAppDbContext _context;
//        private TabControl _tabControl;
//        private Panel _pedigreePanel;
//        private Panel _familyTreePanel;

//        public IndividualAncestryPanel(RATAppBaseForm baseForm, RatAppDbContext context)
//        {
//            _baseForm = baseForm;
//            _context = context;
//            InitializeComponent();
//            InitializeAncestryPanel();
//        }

//        private void InitializeComponent()
//        {
//            SuspendLayout();
//            // 
//            // IndividualAncestryPanel
//            // 
//            ClientSize = new Size(3239, 1309);
//            Name = "IndividualAncestryPanel";
//            ResumeLayout(false);
//        }

//        private void InitializeAncestryPanel()
//        {
//            _tabControl = new TabControl
//            {
//                Dock = DockStyle.Fill
//            };
//            _pedigreePanel = new Panel
//            {
//                Dock = DockStyle.Fill
//            };
//            _familyTreePanel = new Panel
//            {
//                Dock = DockStyle.Fill
//            };

//            _tabControl.TabPages.Add(new TabPage("Pedigree") { Controls = { _pedigreePanel } });
//            _tabControl.TabPages.Add(new TabPage("Family Tree") { Controls = { _familyTreePanel } });

//            this.Controls.Add(_tabControl);

//            // Placeholder content - replace with your actual pedigree and family tree logic
//            Label pedigreeLabel = new Label
//            {
//                Text = "Pedigree content goes here.",
//                Dock = DockStyle.Fill,
//                TextAlign = ContentAlignment.MiddleCenter
//            };
//            _pedigreePanel.Controls.Add(pedigreeLabel);

//            Label familyTreeLabel = new Label
//            {
//                Text = "Family Tree content goes here.",
//                Dock = DockStyle.Fill,
//                TextAlign = ContentAlignment.MiddleCenter
//            };
//            _familyTreePanel.Controls.Add(familyTreeLabel);
//        }

//        public async Task RefreshDataAsync()
//        {
//            // Implement logic to refresh data for pedigree and family tree
//            // This could involve fetching data from the database and updating the UI
//            // Example:
//            // _pedigreePanel.Controls.Clear();
//            // _familyTreePanel.Controls.Clear();
//            // InitializeAncestryPanel(); // Or update the existing panels with new data

//            await Task.CompletedTask; // Placeholder for async operation
//        }
//    }
//}
