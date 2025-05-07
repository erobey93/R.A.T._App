using System;
using System.Drawing;
using System.Windows.Forms;
using RATAPPLibrary.Services;
using RATAPPLibrary.Services.Genetics;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Data.DbContexts;

namespace RATAPP.Forms
{
    public class AddTraitForm : RATAppBaseForm
    {
        private readonly TraitService _traitService;
        private readonly GeneService _geneService;

        // Form Controls
        private TextBox nameTextBox;
        private ComboBox traitTypeComboBox;
        private ComboBox speciesComboBox;
        private TextBox genotypeTextBox;
        private RichTextBox descriptionTextBox;
        private Button saveButton;
        private Button cancelButton;

        RatAppDbContext _context; //TODO

        public AddTraitForm(TraitService traitService, GeneService geneService, RatAppDbContext context)
            : base(context)
        {
            _traitService = traitService;
            _geneService = geneService;
            _context = context; 

            InitializeComponents();
            SetupLayout();
            RegisterEventHandlers();
            LoadTraitTypes();
            LoadSpecies();
        }

        private void InitializeComponents()
        {
            this.Text = "Add New Trait";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            nameTextBox = new TextBox
            {
                Width = 200
            };

            traitTypeComboBox = new ComboBox
            {
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            speciesComboBox = new ComboBox
            {
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            genotypeTextBox = new TextBox
            {
                Width = 200
            };

            descriptionTextBox = new RichTextBox
            {
                Width = 400,
                Height = 100
            };

            saveButton = new Button
            {
                Text = "Save",
                Width = 100,
                Height = 30,
                DialogResult = DialogResult.OK
            };

            cancelButton = new Button
            {
                Text = "Cancel",
                Width = 100,
                Height = 30,
                DialogResult = DialogResult.Cancel
            };
        }

        private void SetupLayout()
        {
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(10),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            // Set column widths
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

            // Add controls with labels
            mainLayout.Controls.Add(new Label { Text = "Name:", AutoSize = true }, 0, 0);
            mainLayout.Controls.Add(nameTextBox, 1, 0);

            mainLayout.Controls.Add(new Label { Text = "Trait Type:", AutoSize = true }, 0, 1);
            mainLayout.Controls.Add(traitTypeComboBox, 1, 1);

            mainLayout.Controls.Add(new Label { Text = "Species:", AutoSize = true }, 0, 2);
            mainLayout.Controls.Add(speciesComboBox, 1, 2);

            mainLayout.Controls.Add(new Label { Text = "Genotype:", AutoSize = true }, 0, 3);
            mainLayout.Controls.Add(genotypeTextBox, 1, 3);

            mainLayout.Controls.Add(new Label { Text = "Description:", AutoSize = true }, 0, 4);
            mainLayout.Controls.Add(descriptionTextBox, 1, 4);

            // Button panel
            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(5)
            };

            buttonPanel.Controls.Add(cancelButton);
            buttonPanel.Controls.Add(saveButton);

            mainLayout.Controls.Add(buttonPanel, 1, 6);
            mainLayout.SetColumnSpan(buttonPanel, 2);

            this.Controls.Add(mainLayout);
        }

        private void RegisterEventHandlers()
        {
            saveButton.Click += SaveButton_Click;
            cancelButton.Click += (s, e) => this.Close();
            this.FormClosing += AddTraitForm_FormClosing;
        }

        private async void LoadTraitTypes()
        {
            try
            {
                var traitTypes = await _traitService.GetAllTraitTypesAsync();
                traitTypeComboBox.DataSource = traitTypes;
                traitTypeComboBox.DisplayMember = "Name";
                traitTypeComboBox.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading trait types: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSpecies()
        {
            // TODO: Implement species loading
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                var trait = await _traitService.CreateTraitAsync(
                    nameTextBox.Text,
                    (int)traitTypeComboBox.SelectedValue,
                    speciesComboBox.Text,
                    descriptionTextBox.Text
                );

                // If genotype is specified, update it
                if (!string.IsNullOrWhiteSpace(genotypeTextBox.Text))
                {
                    // TODO: Implement genotype update logic
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving trait: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Please enter a trait name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (traitTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a trait type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (speciesComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a species.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void AddTraitForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK && HasUnsavedChanges())
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Are you sure you want to close?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private bool HasUnsavedChanges()
        {
            return !string.IsNullOrWhiteSpace(nameTextBox.Text) ||
                   !string.IsNullOrWhiteSpace(genotypeTextBox.Text) ||
                   !string.IsNullOrWhiteSpace(descriptionTextBox.Text) ||
                   traitTypeComboBox.SelectedItem != null ||
                   speciesComboBox.SelectedItem != null;
        }
    }
}
