using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using RATAPPLibrary.Services;
using RATAPPLibrary.Services.Genetics;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.DbContexts;

namespace RATAPP.Forms
{
    public class AssignTraitForm : RATAppBaseForm
    {
        private readonly TraitService _traitService;
        private readonly GeneService _geneService;
        private readonly AnimalService _animalService;

        // Form Controls
        private ComboBox animalComboBox;
        private ComboBox traitTypeComboBox;
        private ListBox availableTraitsListBox;
        private ListBox selectedTraitsListBox;
        private Button addTraitButton;
        private Button removeTraitButton;
        private Button saveButton;
        private Button cancelButton;
        private Label genotypePreviewLabel;

        private Animal selectedAnimal;
        private Dictionary<string, List<string>> currentTraits;

        public AssignTraitForm(TraitService traitService, GeneService geneService, AnimalService animalService, RatAppDbContextFactory contextFactory)
            : base(contextFactory)
        {
            _traitService = traitService;
            _geneService = geneService;
            _animalService = animalService;

            InitializeComponents();
            SetupLayout();
            RegisterEventHandlers();
            LoadAnimals();
            LoadTraitTypes();
        }

        private void InitializeComponents()
        {
            this.Text = "Assign Traits";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            animalComboBox = new ComboBox
            {
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            traitTypeComboBox = new ComboBox
            {
                Width = 200,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            availableTraitsListBox = new ListBox
            {
                Width = 200,
                Height = 300,
                SelectionMode = SelectionMode.MultiExtended
            };

            selectedTraitsListBox = new ListBox
            {
                Width = 200,
                Height = 300,
                SelectionMode = SelectionMode.MultiExtended
            };

            addTraitButton = new Button
            {
                Text = "→",
                Width = 40,
                Height = 30
            };

            removeTraitButton = new Button
            {
                Text = "←",
                Width = 40,
                Height = 30
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

            genotypePreviewLabel = new Label
            {
                AutoSize = false,
                Width = 400,
                Height = 60,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5)
            };
        }

        private void SetupLayout()
        {
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 5,
                Padding = new Padding(10)
            };

            // Set column widths
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            // Animal selection
            var animalPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 10)
            };
            animalPanel.Controls.AddRange(new Control[]
            {
                new Label { Text = "Select Animal:", AutoSize = true },
                animalComboBox
            });
            mainLayout.Controls.Add(animalPanel, 0, 0);
            mainLayout.SetColumnSpan(animalPanel, 4);

            // Trait type selection
            mainLayout.Controls.Add(new Label { Text = "Trait Type:", AutoSize = true }, 0, 1);
            mainLayout.Controls.Add(traitTypeComboBox, 0, 2);

            // Available and Selected traits
            var availableTraitsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true
            };
            availableTraitsPanel.Controls.AddRange(new Control[]
            {
                new Label { Text = "Available Traits:", AutoSize = true },
                availableTraitsListBox
            });
            mainLayout.Controls.Add(availableTraitsPanel, 0, 3);

            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                Padding = new Padding(5)
            };
            buttonPanel.Controls.AddRange(new Control[]
            {
                addTraitButton,
                removeTraitButton
            });
            mainLayout.Controls.Add(buttonPanel, 1, 3);

            var selectedTraitsPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true
            };
            selectedTraitsPanel.Controls.AddRange(new Control[]
            {
                new Label { Text = "Selected Traits:", AutoSize = true },
                selectedTraitsListBox
            });
            mainLayout.Controls.Add(selectedTraitsPanel, 2, 3);

            // Genotype preview
            var genotypePanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0)
            };
            genotypePanel.Controls.AddRange(new Control[]
            {
                new Label { Text = "Predicted Genotype:", AutoSize = true },
                genotypePreviewLabel
            });
            mainLayout.Controls.Add(genotypePanel, 0, 4);
            mainLayout.SetColumnSpan(genotypePanel, 4);

            // Button panel
            var actionButtonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(5)
            };
            actionButtonPanel.Controls.AddRange(new Control[]
            {
                cancelButton,
                saveButton
            });

            this.Controls.Add(mainLayout);
            this.Controls.Add(actionButtonPanel);
        }

        private void RegisterEventHandlers()
        {
            animalComboBox.SelectedIndexChanged += AnimalComboBox_SelectedIndexChanged;
            traitTypeComboBox.SelectedIndexChanged += TraitTypeComboBox_SelectedIndexChanged;
            addTraitButton.Click += AddTraitButton_Click;
            removeTraitButton.Click += RemoveTraitButton_Click;
            saveButton.Click += SaveButton_Click;
            selectedTraitsListBox.SelectedIndexChanged += SelectedTraits_Changed;
        }

        private async void LoadAnimals()
        {
            try
            {
                var animals = await _animalService.GetAllAnimalsAsync();
                animalComboBox.DataSource = animals;
                animalComboBox.DisplayMember = "DisplayName";
                animalComboBox.ValueMember = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading animals: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private async void AnimalComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (animalComboBox.SelectedItem != null)
            {
                var animalDto = animalComboBox.SelectedItem as AnimalDto;
                var parseAnimalDto = await _animalService.MapAnimalDtoBackToAnimal(animalDto);
                selectedAnimal = parseAnimalDto;
                currentTraits = await _traitService.GetTraitMapForSingleAnimal(selectedAnimal.Id);
                await RefreshTraitLists();
            }
        }

        private async void TraitTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            await RefreshTraitLists();
        }

        private async Task RefreshTraitLists()
        {
            if (traitTypeComboBox.SelectedItem == null || selectedAnimal == null)
                return;

            try
            {
                var traitType = traitTypeComboBox.Text;
                var species = selectedAnimal.Line?.Stock?.Species.CommonName;
                if (species == null)
                    return;

                var availableTraits = await _traitService.GetTraitsByTypeAndSpeciesAsync(traitType, species);
                var currentTypeTraits = currentTraits.ContainsKey(traitType) ? currentTraits[traitType] : new List<string>();

                availableTraitsListBox.Items.Clear();
                selectedTraitsListBox.Items.Clear();

                foreach (var trait in availableTraits)
                {
                    if (!currentTypeTraits.Contains(trait))
                        availableTraitsListBox.Items.Add(trait);
                }

                foreach (var trait in currentTypeTraits)
                {
                    selectedTraitsListBox.Items.Add(trait);
                }

                UpdateGenotypePreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing traits: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddTraitButton_Click(object sender, EventArgs e)
        {
            var selectedItems = availableTraitsListBox.SelectedItems.Cast<string>().ToList();
            foreach (var item in selectedItems)
            {
                availableTraitsListBox.Items.Remove(item);
                selectedTraitsListBox.Items.Add(item);
            }
            UpdateGenotypePreview();
        }

        private void RemoveTraitButton_Click(object sender, EventArgs e)
        {
            var selectedItems = selectedTraitsListBox.SelectedItems.Cast<string>().ToList();
            foreach (var item in selectedItems)
            {
                selectedTraitsListBox.Items.Remove(item);
                availableTraitsListBox.Items.Add(item);
            }
            UpdateGenotypePreview();
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                foreach (string traitName in selectedTraitsListBox.Items)
                {
                    var trait = await _traitService.GetTraitByNameAsync(traitName);
                    await _traitService.CreateAnimalTraitAsync(trait.Id, selectedAnimal.Id);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving traits: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectedTraits_Changed(object sender, EventArgs e)
        {
            UpdateGenotypePreview();
        }

        private void UpdateGenotypePreview()
        {
            var selectedTraits = selectedTraitsListBox.Items.Cast<string>().ToList();
            if (selectedTraits.Any())
            {
                genotypePreviewLabel.Text = string.Join(", ", selectedTraits.Select(t => $"{t}: Genotype pending..."));
            }
            else
            {
                genotypePreviewLabel.Text = "No traits selected";
            }
        }

        private bool ValidateForm()
        {
            if (animalComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select an animal.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (selectedTraitsListBox.Items.Count == 0)
            {
                MessageBox.Show("Please select at least one trait.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
    }
}
