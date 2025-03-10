using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;

namespace RATAPP.Forms
{
    public class HealthRecordForm : Form
    {
        private RatAppDbContext _context;
        private AnimalDto _currentAnimal;
        private RATAppBaseForm _baseForm;

        // UI Components
        private Label titleLabel;
        private Label animalIdLabel;
        private Label animalNameLabel;
        private TabControl healthTabControl;
        private RichTextBox notesTextBox;
        private ListBox recordsListBox;
        private RichTextBox testResultsTextBox;
        private RichTextBox necropsyResultsTextBox;
        private RichTextBox causeOfDeathTextBox;
        private RichTextBox medicationTextBox;
        private RichTextBox healthTrendsTextBox;
        private Button addRecordButton;

        public static HealthRecordForm Create(RATAppBaseForm baseForm, RatAppDbContext context, AnimalDto animal)
        {
            return new HealthRecordForm(baseForm, context, animal);
        }

        public HealthRecordForm(RATAppBaseForm baseForm, RatAppDbContext context, AnimalDto animal)
        {
            _baseForm = baseForm;
            _context = context;
            _currentAnimal = animal;

            this.Dock = DockStyle.Fill;
            this.Size = new Size(800, 600);
            this.BackColor = Color.FromArgb(240, 240, 240);

            InitializeComponents();
            LoadHealthData();
        }

        private void InitializeComponents()
        {
            // Page Title
            titleLabel = new Label
            {
                Text = "Animal Health",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64),
                Location = new Point(20, 20),
                Size = new Size(300, 40),
                AutoSize = true
            };

            // Animal ID Label
            animalIdLabel = new Label
            {
                Text = $"Animal ID: {_currentAnimal.regNum}",
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

            // Health Tab Control
            healthTabControl = new TabControl
            {
                Location = new Point(20, 140),
                Size = new Size(760, 440),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };

            // Notes Tab
            TabPage notesTab = new TabPage("Notes");
            notesTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            notesTab.Controls.Add(notesTextBox);
            healthTabControl.TabPages.Add(notesTab);

            // Records Tab
            TabPage recordsTab = new TabPage("Records");
            recordsListBox = new ListBox
            {
                Dock = DockStyle.Left,
                Width = 200,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            addRecordButton = CreateButton("Add Record", 210, 10, 120, Color.FromArgb(0, 150, 136));
            addRecordButton.Click += AddRecordButton_Click;

            RichTextBox recordDetailsTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Left = 210,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true
            };
            recordsListBox.SelectedIndexChanged += (s, e) => {
                //TODO: load record details to the recordDetailsTextBox, using the recordsListBox.SelectedItem.
                recordDetailsTextBox.Text = "Placeholder record details";
            };

            Panel recordsPanel = new Panel { Dock = DockStyle.Fill };
            recordsPanel.Controls.Add(recordDetailsTextBox);
            recordsPanel.Controls.Add(addRecordButton);
            recordsTab.Controls.Add(recordsListBox);
            recordsTab.Controls.Add(recordsPanel);

            healthTabControl.TabPages.Add(recordsTab);

            // Test Results Tab
            TabPage testResultsTab = new TabPage("Test Results");
            testResultsTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            testResultsTab.Controls.Add(testResultsTextBox);
            healthTabControl.TabPages.Add(testResultsTab);

            // Necropsy Results Tab
            TabPage necropsyResultsTab = new TabPage("Necropsy Results");
            necropsyResultsTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            necropsyResultsTab.Controls.Add(necropsyResultsTextBox);
            healthTabControl.TabPages.Add(necropsyResultsTab);

            // Cause of Death Tab
            TabPage causeOfDeathTab = new TabPage("Cause of Death");
            causeOfDeathTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            causeOfDeathTab.Controls.Add(causeOfDeathTextBox);
            healthTabControl.TabPages.Add(causeOfDeathTab);

            // Medication tab
            TabPage medicationTab = new TabPage("Medication");
            medicationTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            medicationTab.Controls.Add(causeOfDeathTextBox);
            healthTabControl.TabPages.Add(medicationTab);

            // Medication tab
            TabPage trendsTab = new TabPage("Health Trends");
            healthTrendsTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            trendsTab.Controls.Add(healthTrendsTextBox);
            healthTabControl.TabPages.Add(trendsTab);

            // Add all controls to the form
            this.Controls.Add(titleLabel);
            this.Controls.Add(animalIdLabel);
            this.Controls.Add(animalNameLabel);
            this.Controls.Add(healthTabControl);
        
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

        private void LoadHealthData()
        {
            // TODO: Load health data from _context and populate the UI elements
            // Example placeholder data:
            notesTextBox.Text = "Placeholder notes...";
            recordsListBox.Items.Add("Record 1 (Date: 2023-10-26)");
            recordsListBox.Items.Add("Record 2 (Date: 2023-11-01)");
            testResultsTextBox.Text = "Placeholder test results...";
            necropsyResultsTextBox.Text = "Placeholder necropsy results...";
            causeOfDeathTextBox.Text = "Placeholder cause of death...";
        }

        private void AddRecordButton_Click(object sender, EventArgs e)
        {
            // TODO: Implement logic to add a health record
            MessageBox.Show("Add Health Record functionality to be implemented.");
            // Open a new form or dialog to add a record
        }
    }
}