using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Data.Models.Animal_Management;

namespace RATAPP.Helpers
{
    /// <summary>
    /// Handles all events for the pairing form.
    /// This pattern can be applied to other forms that need similar event handling.
    /// </summary>
    public class PairingEventHandler
    {
        private readonly PairingDataManager _dataManager;
        private readonly LoadingSpinnerHelper _spinner;
        private List<Species> _allSpecies;

        public PairingEventHandler(PairingDataManager dataManager, LoadingSpinnerHelper spinner)
        {
            _dataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
            _spinner = spinner ?? throw new ArgumentNullException(nameof(spinner));
        }

        /// <summary>
        /// Handles the initial form load
        /// </summary>
        public async Task HandleFormLoadAsync(
            ComboBox speciesComboBox,
            ComboBox damComboBox,
            ComboBox sireComboBox,
            ComboBox projectComboBox)
        {
            await _spinner.ExecuteWithSpinner(async () =>
            {
                // Load species
                _allSpecies = await _dataManager.LoadSpeciesAsync();
                foreach (var species in _allSpecies)
                {
                    speciesComboBox.Items.Add(new DropdownHelper.ListItem<Species>
                    {
                        Display = species.CommonName,
                        Value = species
                    });
                }

                // Load initial animals and projects
                await LoadAnimalsAsync(damComboBox, sireComboBox, null);
                await LoadProjectsAsync(projectComboBox);
            });
        }

        /// <summary>
        /// Handles species selection change
        /// </summary>
        public async Task HandleSpeciesSelectionChangedAsync(
            ComboBox speciesComboBox,
            ComboBox damComboBox,
            ComboBox sireComboBox)
        {
            if (speciesComboBox.SelectedItem is DropdownHelper.ListItem<Species> selectedItem)
            {
                await _spinner.ExecuteWithSpinner(async () =>
                {
                    await LoadAnimalsAsync(damComboBox, sireComboBox, selectedItem.Value.CommonName);
                });
            }
        }

        /// <summary>
        /// Handles the add pairing button click
        /// </summary>
        public async Task HandleAddPairingClickAsync(
            string pairingId,
            ComboBox damComboBox,
            ComboBox sireComboBox,
            ComboBox projectComboBox,
            DateTimePicker pairingDatePicker)
        {
            await _spinner.ExecuteWithSpinner(async () =>
            {
                var data = new PairingDataManager.PairingData
                {
                    PairingId = pairingId,
                    Dam = (damComboBox.SelectedItem as DropdownHelper.ListItem<AnimalDto>)?.Value,
                    Sire = (sireComboBox.SelectedItem as DropdownHelper.ListItem<AnimalDto>)?.Value,
                    Project = (projectComboBox.SelectedItem as DropdownHelper.ListItem<Project>)?.Value,
                    PairingDate = pairingDatePicker.Value
                };

                if (await _dataManager.SavePairingAsync(data))
                {
                    // Clear form for next entry
                    ClearForm(pairingId, damComboBox, sireComboBox, projectComboBox, pairingDatePicker);
                }
            });
        }

        /// <summary>
        /// Handles adding a pairing to the grid
        /// </summary>
        public void HandleAddToGridClick(
            string pairingId,
            ComboBox damComboBox,
            ComboBox sireComboBox,
            ComboBox projectComboBox,
            DateTimePicker pairingDatePicker,
            DataGridView pairingsGrid)
        {
            // Validate inputs
            if (!ValidateGridInputs(pairingId, damComboBox, sireComboBox, projectComboBox))
                return;

            // Add to grid
            pairingsGrid.Rows.Add(
                pairingId,
                (projectComboBox.SelectedItem as DropdownHelper.ListItem<Project>)?.Display,
                (damComboBox.SelectedItem as DropdownHelper.ListItem<AnimalDto>)?.Display,
                (sireComboBox.SelectedItem as DropdownHelper.ListItem<AnimalDto>)?.Display,
                pairingDatePicker.Value.ToShortDateString()
            );

            // Clear form for next entry
            ClearForm(pairingId, damComboBox, sireComboBox, projectComboBox, pairingDatePicker);
        }

        /// <summary>
        /// Handles saving all pairings from the grid
        /// </summary>
        public async Task HandleSaveAllPairingsAsync(
            DataGridView pairingsGrid,
            ComboBox damComboBox,
            ComboBox sireComboBox,
            ComboBox projectComboBox)
        {
            await _spinner.ExecuteWithSpinner(async () =>
            {
                string errorMessage;
                if (!ValidationHelper.ValidateMultiplePairings(pairingsGrid, out errorMessage))
                {
                    ValidationHelper.ShowError(errorMessage);
                    return;
                }

                var pairings = new List<PairingDataManager.PairingData>();
                foreach (DataGridViewRow row in pairingsGrid.Rows)
                {
                    pairings.Add(new PairingDataManager.PairingData
                    {
                        PairingId = row.Cells["PairingID"].Value?.ToString(),
                        Dam = GetAnimalFromDisplay(row.Cells["Dam"].Value?.ToString(), damComboBox, sireComboBox),
                        Sire = GetAnimalFromDisplay(row.Cells["Sire"].Value?.ToString(), damComboBox, sireComboBox),
                        Project = GetProjectFromDisplay(row.Cells["Project"].Value?.ToString(), projectComboBox),
                        PairingDate = DateTime.Parse(row.Cells["PairingDate"].Value?.ToString())
                    });
                }

                if (await _dataManager.SaveMultiplePairingsAsync(pairings))
                {
                    pairingsGrid.Rows.Clear();
                }
            });
        }

        /// <summary>
        /// Handles the cancel button click
        /// </summary>
        public void HandleCancelClick(Form form)
        {
            if (MessageBox.Show(
                "Are you sure you want to cancel? Any unsaved data will be lost.",
                "Confirm Cancel",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                form.Close();
            }
        }

        #region Private Helper Methods

        private async Task LoadAnimalsAsync(ComboBox damComboBox, ComboBox sireComboBox, string species)
        {
            // Load dams
            var dams = await _dataManager.LoadAnimalsAsync("Female", species);
            PopulateAnimalComboBox(damComboBox, dams);

            // Load sires
            var sires = await _dataManager.LoadAnimalsAsync("Male", species);
            PopulateAnimalComboBox(sireComboBox, sires);
        }

        private async Task LoadProjectsAsync(ComboBox projectComboBox)
        {
            var projects = await _dataManager.LoadProjectsAsync();
            projectComboBox.Items.Clear();
            foreach (var project in projects)
            {
                projectComboBox.Items.Add(new DropdownHelper.ListItem<Project>
                {
                    Display = project.Name,
                    Value = project
                });
            }
        }

        private void PopulateAnimalComboBox(ComboBox comboBox, List<AnimalDto> animals)
        {
            comboBox.Items.Clear();
            foreach (var animal in animals)
            {
                comboBox.Items.Add(new DropdownHelper.ListItem<AnimalDto>
                {
                    Display = $"{animal.Id} - {animal.name}",
                    Value = animal
                });
            }
        }

        private bool ValidateGridInputs(
            string pairingId,
            ComboBox damComboBox,
            ComboBox sireComboBox,
            ComboBox projectComboBox)
        {
            if (string.IsNullOrWhiteSpace(pairingId) ||
                damComboBox.SelectedIndex == -1 ||
                sireComboBox.SelectedIndex == -1 ||
                projectComboBox.SelectedIndex == -1)
            {
                ValidationHelper.ShowError("Please fill in all required fields");
                return false;
            }
            return true;
        }

        private void ClearForm(
            string pairingId,
            ComboBox damComboBox,
            ComboBox sireComboBox,
            ComboBox projectComboBox,
            DateTimePicker pairingDatePicker)
        {
            pairingId = string.Empty;
            damComboBox.SelectedIndex = -1;
            sireComboBox.SelectedIndex = -1;
            projectComboBox.SelectedIndex = -1;
            pairingDatePicker.Value = DateTime.Today;
        }

        private AnimalDto GetAnimalFromDisplay(string display, ComboBox damComboBox, ComboBox sireComboBox)
        {
            if (string.IsNullOrEmpty(display))
                return null;

            // Find the matching item in the dam/sire combo boxes
            var damItem = damComboBox.Items.Cast<DropdownHelper.ListItem<AnimalDto>>()
                .FirstOrDefault(i => i.Display == display);
            if (damItem != null)
                return damItem.Value;

            var sireItem = sireComboBox.Items.Cast<DropdownHelper.ListItem<AnimalDto>>()
                .FirstOrDefault(i => i.Display == display);
            return sireItem?.Value;
        }

        private Project GetProjectFromDisplay(string display, ComboBox projectComboBox)
        {
            if (string.IsNullOrEmpty(display))
                return null;

            // Find the matching item in the project combo box
            var projectItem = projectComboBox.Items.Cast<DropdownHelper.ListItem<Project>>()
                .FirstOrDefault(i => i.Display == display);
            return projectItem?.Value;
        }

        #endregion
    }
}
