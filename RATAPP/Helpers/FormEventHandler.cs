using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
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
                speciesBox.Items.AddRange(species.ToArray());

                // Load projects
                var projects = await _dataManager.GetProjectsAsync();
                projectBox.Items.AddRange(projects.Select(p => p.Name).ToArray());
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

                var project = (await _dataManager.GetProjectsAsync())
                    .First(p => p.Name == projectBox.SelectedItem.ToString());

                var pairing = new Pairing
                {
                    pairingId = pairingId,
                    DamId = int.Parse(damBox.SelectedItem.ToString().Split('-')[0].Trim()),
                    SireId = int.Parse(sireBox.SelectedItem.ToString().Split('-')[0].Trim()),
                    ProjectId = project.Id,
                    PairingStartDate = datePicker.Value,
                    CreatedOn = DateTime.Now,
                    LastUpdated = DateTime.Now
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

        public async Task HandleAddLitterClickAsync(string litterId, string litterName, int pairId, 
            DateTimePicker datePicker, TextBox numPups, TextBox numMales, TextBox numFemales, TextBox notes)
        {
            try
            {
                _spinner.Show();

                // Validate inputs
                if (string.IsNullOrWhiteSpace(litterId) || string.IsNullOrWhiteSpace(litterName))
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
                    DateOfBirth = datePicker.Value,
                    NumPups = !string.IsNullOrWhiteSpace(numPups.Text) ? int.Parse(numPups.Text) : null,
                    NumMale = !string.IsNullOrWhiteSpace(numMales.Text) ? int.Parse(numMales.Text) : null,
                    NumFemale = !string.IsNullOrWhiteSpace(numFemales.Text) ? int.Parse(numFemales.Text) : null,
                    Notes = notes.Text,
                    CreatedOn = DateTime.Now,
                    LastUpdated = DateTime.Now
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

        public void HandleAddLitterToGridClick(string litterId, string litterName, int pairId,
            DateTimePicker datePicker, TextBox numPups, TextBox numMales, TextBox numFemales, TextBox notes,
            DataGridView grid)
        {
            try
            {
                _spinner.Show();

                // Validate inputs
                if (string.IsNullOrWhiteSpace(litterId) || string.IsNullOrWhiteSpace(litterName))
                {
                    MessageBox.Show("Please fill in all required fields", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                grid.Rows.Add(
                    litterId,
                    litterName,
                    pairId.ToString(),
                    datePicker.Value.ToShortDateString(),
                    numPups.Text,
                    numMales.Text,
                    numFemales.Text,
                    notes.Text
                );
            }
            finally
            {
                _spinner.Hide();
            }
        }

        public async Task HandleSaveAllLittersAsync(DataGridView grid)
        {
            try
            {
                _spinner.Show();

                if (grid.Rows.Count == 0)
                {
                    MessageBox.Show("Please add at least one litter to the list", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                foreach (DataGridViewRow row in grid.Rows)
                {
                    var litter = new Litter
                    {
                        LitterId = row.Cells["LitterID"].Value.ToString(),
                        Name = row.Cells["Name"].Value.ToString(),
                        PairId = int.Parse(row.Cells["PairID"].Value.ToString()),
                        DateOfBirth = DateTime.Parse(row.Cells["BirthDate"].Value.ToString()),
                        NumPups = !string.IsNullOrWhiteSpace(row.Cells["NumPups"].Value?.ToString()) 
                            ? int.Parse(row.Cells["NumPups"].Value.ToString()) : null,
                        NumMale = !string.IsNullOrWhiteSpace(row.Cells["NumMales"].Value?.ToString()) 
                            ? int.Parse(row.Cells["NumMales"].Value.ToString()) : null,
                        NumFemale = !string.IsNullOrWhiteSpace(row.Cells["NumFemales"].Value?.ToString()) 
                            ? int.Parse(row.Cells["NumFemales"].Value.ToString()) : null,
                        Notes = row.Cells["Notes"].Value?.ToString(),
                        CreatedOn = DateTime.Now,
                        LastUpdated = DateTime.Now
                    };

                    await _dataManager.SaveLitterAsync(litter);
                }

                MessageBox.Show($"Successfully added {grid.Rows.Count} litters!", "Success", 
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
