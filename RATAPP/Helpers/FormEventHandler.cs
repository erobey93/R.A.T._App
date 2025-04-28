using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using RATAPPLibrary.Data.Models.Breeding;

namespace RATAPP.Helpers
{
    /// <summary>
    /// Generic event handler for breeding-related forms
    /// </summary>
    public class FormEventHandler
    {
        private readonly FormDataManager _dataManager;
        private readonly LoadingSpinnerHelper _spinner;

        public FormEventHandler(FormDataManager dataManager, LoadingSpinnerHelper spinner)
        {
            _dataManager = dataManager;
            _spinner = spinner;
        }

        public async Task HandleFormLoadAsync(ComboBox speciesBox, ComboBox damBox, ComboBox sireBox, ComboBox projectBox)
        {
            try
            {
                _spinner.Show();
                
                // Load species
                var species = await _dataManager.GetSpeciesAsync();
                speciesBox.Items.AddRange(species);

                // Load projects
                var projects = await _dataManager.GetProjectsAsync();
                projectBox.Items.AddRange(projects);
            }
            finally
            {
                _spinner.Hide();
            }
        }

        public async Task HandleSpeciesSelectionChangedAsync(ComboBox speciesBox, ComboBox damBox, ComboBox sireBox)
        {
            if (speciesBox.SelectedItem == null) return;

            try
            {
                _spinner.Show();
                string selectedSpecies = speciesBox.SelectedItem.ToString();

                // Clear existing items
                damBox.Items.Clear();
                sireBox.Items.Clear();

                // Load dams and sires for selected species
                var dams = await _dataManager.GetDamsAsync(selectedSpecies);
                var sires = await _dataManager.GetSiresAsync(selectedSpecies);

                foreach (var dam in dams)
                {
                    damBox.Items.Add(dam.name);
                }

                foreach (var sire in sires)
                {
                    sireBox.Items.Add(sire.name);
                }
            }
            finally
            {
                _spinner.Hide();
            }
        }

        public async Task HandleAddPairingClickAsync(string pairingId, ComboBox damBox, ComboBox sireBox, ComboBox projectBox, DateTimePicker datePicker)
        {
            try
            {
                _spinner.Show();

                // Validate inputs
                if (string.IsNullOrWhiteSpace(pairingId) || damBox.SelectedIndex == -1 || 
                    sireBox.SelectedIndex == -1 || projectBox.SelectedIndex == -1)
                {
                    MessageBox.Show("Please fill in all required fields", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var pairing = new Pairing
                {
                    PairingId = pairingId,
                    DamName = damBox.SelectedItem.ToString(),
                    SireName = sireBox.SelectedItem.ToString(),
                    ProjectName = projectBox.SelectedItem.ToString(),
                    PairingDate = datePicker.Value
                };

                await _dataManager.SavePairingAsync(pairing);

                MessageBox.Show("Pairing added successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                _spinner.Hide();
            }
        }

        public async Task HandleAddLitterClickAsync(string litterId, string litterName, ComboBox damBox, ComboBox sireBox, int pairId, DateTimePicker datePicker)
        {
            try
            {
                _spinner.Show();

                // Validate inputs
                if (string.IsNullOrWhiteSpace(litterId) || damBox.SelectedIndex == -1 || 
                    sireBox.SelectedIndex == -1)
                {
                    MessageBox.Show("Please fill in all required fields", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var litter = new Litter
                {
                    LitterId = litterId,
                    Name = litterName,
                    PairId = pairId,
                    DateOfBirth = datePicker.Value
                };

                await _dataManager.SaveLitterAsync(litter);

                MessageBox.Show("Litter added successfully!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                _spinner.Hide();
            }
        }

        public void HandleAddToGridClick(string id, ComboBox damBox, ComboBox sireBox, ComboBox projectBox, 
            DateTimePicker datePicker, DataGridView grid)
        {
            try
            {
                _spinner.Show();

                // Validate inputs
                if (string.IsNullOrWhiteSpace(id) || damBox.SelectedIndex == -1 || 
                    sireBox.SelectedIndex == -1 || projectBox.SelectedIndex == -1)
                {
                    MessageBox.Show("Please fill in all required fields", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                grid.Rows.Add(
                    damBox.SelectedItem.ToString(),
                    sireBox.SelectedItem.ToString(),
                    id,
                    projectBox.SelectedItem.ToString(),
                    datePicker.Value.ToShortDateString()
                );
            }
            finally
            {
                _spinner.Hide();
            }
        }

        public async Task HandleSaveAllPairingsAsync(DataGridView grid, ComboBox damBox, ComboBox sireBox, ComboBox projectBox)
        {
            try
            {
                _spinner.Show();

                if (grid.Rows.Count == 0)
                {
                    MessageBox.Show("Please add at least one item to the list", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                foreach (DataGridViewRow row in grid.Rows)
                {
                    var pairing = new Pairing
                    {
                        DamName = row.Cells["Dam"].Value.ToString(),
                        SireName = row.Cells["Sire"].Value.ToString(),
                        PairingId = row.Cells["PairingID"].Value.ToString(),
                        ProjectName = row.Cells["Project"].Value.ToString(),
                        PairingDate = DateTime.Parse(row.Cells["PairingDate"].Value.ToString())
                    };

                    await _dataManager.SavePairingAsync(pairing);
                }

                MessageBox.Show($"Successfully added {grid.Rows.Count} pairings!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                grid.Rows.Clear();
            }
            finally
            {
                _spinner.Hide();
            }
        }

        public void HandleCancelClick(Form form)
        {
            if (MessageBox.Show("Are you sure you want to cancel? Any unsaved data will be lost.",
                "Confirm Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                form.Close();
            }
        }
    }
}
