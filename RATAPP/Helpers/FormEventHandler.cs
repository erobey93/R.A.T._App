using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Data.Models;
using Microsoft.EntityFrameworkCore;
using RATAPPLibrary.Services;

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

        public async Task HandleFormLoadAsyncPairing(ComboBox speciesComboBox, ComboBox damBox, ComboBox sireBox, ComboBox projectComboBox)
        {
            try
            {
                _spinner.Show();

                // LOAD SPECIES
                var species = await _dataManager.GetSpeciesAsync();
                speciesComboBox.Items.Clear();
                speciesComboBox.Items.Add("All Species");

                foreach (var s in species)
                {
                    speciesComboBox.Items.Add(s);
                }

                speciesComboBox.SelectedIndex = 0;
                speciesComboBox.DisplayMember = "CommonName";
                speciesComboBox.ValueMember = "Id";
               

                // LOAD PROJECTS
                var projects = await _dataManager.GetProjectsAsync();
                projectComboBox.Items.Clear(); // It's good practice to clear existing items
                foreach (var project in projects)
                {
                    projectComboBox.Items.Add(project);
                }
                projectComboBox.DisplayMember = "Name"; // Now this will work correctly
                projectComboBox.ValueMember = "Id";

                //LOAD ANIMALS 
                //var ratDams = _dataManager.GetDamsAsync("Rat"); //FIXME manually going in to get ratDams and mouseDams to start but I need to fix this logic 
                //var mouseDams = _dataManager.GetDamsAsync("Mouse");

                //var ratSires = _dataManager.GetDamsAsync("Rat");
                //var mouseSires = _dataManager.GetSiresAsync("Mouse");

                //damBox.Items.Clear();
                //sireBox.Items.Clear();

                //damBox.Items.Add("All Dams");
                //sireBox.Items.Add("All Sires");

                //foreach (var d in ratDams.Result)
                //{
                //    damBox.Items.Add(d);
                //}
                //foreach (var d in mouseDams.Result)
                //{
                //    damBox.Items.Add(d);
                //}
                //foreach (var s in ratSires.Result)
                //{
                //    sireBox.Items.Add(s);
                //}
                //foreach (var s in mouseSires.Result)
                //{
                //    sireBox.Items.Add(s);
                //}
                //damBox.DisplayMember = "Name";
                //damBox.ValueMember = "Id";
                //damBox.SelectedIndex = 0;

                //sireBox.DisplayMember = "Name";
                //sireBox.ValueMember = "Id";
                //sireBox.SelectedIndex = 0;

            }
            finally
            {
                _spinner.Hide();
            }
        }
        public async Task HandleFormLoadAsyncLitter(ComboBox speciesComboBox, ComboBox damBox, ComboBox sireBox, ComboBox projectComboBox, ComboBox pairsComboBox) //, ComboBox pairsComboBox
        {
            try
            {
                _spinner.Show();

                // LOAD SPECIES
                var species = await _dataManager.GetSpeciesAsync();
                //speciesBox.Items.AddRange(species.ToArray()); // Use AddRange for a collection
                //speciesBox.ValueMember = "Id"; // What value do we use when we're referring to this, should always be an ID of some kind
                //speciesBox.DisplayMember = "Common Name"; // What do we want to display for users to select
                speciesComboBox.Items.Clear();
                speciesComboBox.Items.Add("All Species");

                foreach (var s in species)
                {
                    speciesComboBox.Items.Add(s);
                }

                speciesComboBox.SelectedIndex = 0;
                speciesComboBox.DisplayMember = "CommonName";
                speciesComboBox.ValueMember = "Id";

                // LOAD PAIRS  
                var pairs = await _dataManager.GetPairingsAsync();

                pairsComboBox.Items.Clear();
                pairsComboBox.Items.Add("All Pairs");

                foreach (var p in pairs)
                {
                    pairsComboBox.Items.Add(p);
                }

                pairsComboBox.SelectedIndex = 0;
                pairsComboBox.DisplayMember = "pairingId"; //FIXME I may need a way to make it easier for user to distinguish pairs i.e. dam+sire+date? 
                pairsComboBox.ValueMember = "pairingId";

                // LOAD PROJECTS
                var projects = await _dataManager.GetProjectsAsync();
                projectComboBox.Items.Clear(); // It's good practice to clear existing items
                foreach (var project in projects)
                {
                    projectComboBox.Items.Add(project);
                }
                projectComboBox.DisplayMember = "Name"; // Now this will work correctly
                projectComboBox.ValueMember = "Id";

                //LOAD ANIMALS 
                var ratDams = _dataManager.GetDamsAsync("Rat"); //FIXME manually going in to get ratDams and mouseDams to start but I need to fix this logic 
                var mouseDams = _dataManager.GetDamsAsync("Mouse");

                var ratSires = _dataManager.GetDamsAsync("Rat");
                var mouseSires = _dataManager.GetSiresAsync("Mouse");

                damBox.Items.Clear();   
                sireBox.Items.Clear();

                damBox.Items.Add("All Dams");
                sireBox.Items.Add("All Sires"); 

                foreach (var d in ratDams.Result)
                {
                    damBox.Items.Add(d);
                }
                foreach (var d in mouseDams.Result)
                {
                    damBox.Items.Add(d);
                }
                foreach (var s in ratSires.Result)
                {
                    sireBox.Items.Add(s);
                }
                foreach(var s in mouseSires.Result)
                {
                    sireBox.Items.Add(s);
                }
                damBox.DisplayMember = "Name";
                damBox.ValueMember = "Id";
                damBox.SelectedIndex = 0;

                sireBox.DisplayMember = "Name";
                sireBox.ValueMember = "Id";
                sireBox.SelectedIndex = 0;

            }
            finally
            {
                _spinner.Hide();
            }
        }

        //When the species changes, the available dams, sires, projects, pairs, lines, etc should also change
        //TODO on everything except for dam and sire right now FIXME 
        public async Task HandleSpeciesSelectionChangedDamAndSireAsync(ComboBox speciesBox, ComboBox damBox, ComboBox sireBox)
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

        //When the species changes, the available dams, sires, projects, pairs, lines, etc should also change
        //TODO on everything except for dam and sire right now FIXME 
        public async Task HandleSpeciesSelectionChangedProjectsLinesPairsAsync(ComboBox speciesBox, ComboBox pairBox, ComboBox projectBox)
        {
            if (speciesBox.SelectedItem == null) return;

            try
            {
                _spinner.Show();
                string selectedSpecies = speciesBox.SelectedItem.ToString();

                // Clear existing items
                pairBox.Items.Clear();
                projectBox.Items.Clear();
                //lineBox.Items.Clear();

                // Load dams and sires for selected species
                var pairs = await _dataManager.GetPairingsBySpeciesAsync(selectedSpecies);
                //var lines = await _dataManager.GetLinesAsync(selectedSpecies);
                var projects = await _dataManager.GetProjectsAsync(); 
                //get projects by species
                //get pairs by species 

                foreach (var pair in pairs) //FIXME IDs don't really make sense it should find the dam and sire based on pair or find the pair based on dam and sire i.e. it should be filtering the pairs based on dam and sire if they exist 
                {
                    pairBox.Items.Add(pair.Id);
                }

                foreach (var project in projects)
                {
                    pairBox.Items.Add(project.Id);
                }
            }
            finally
            {
                _spinner.Hide();
            }
        }

        //dam comboBox changed
        //set pairs accordingly 
        public async Task HandleDamSelectionChangedAsync(ComboBox damBox, ComboBox sireBox, ComboBox pairBox)
        {
            if (damBox.SelectedItem == null) return;

            try
            {
                _spinner.Show();
                var selectedDam = damBox.SelectedItem as AnimalDto;

                // Clear existing items
                pairBox.Items.Clear();


                //check if the sire combo box is populated
                if (sireBox.SelectedItem != null)
                {
                    var selectedSire = sireBox.SelectedItem as AnimalDto;

                    var pairs = await _dataManager.GetActivePairingsByDamandSire(selectedDam.Id, selectedSire.Id);


                    foreach (var pair in pairs) //FIXME IDs don't really make sense it should find the dam and sire based on pair or find the pair based on dam and sire i.e. it should be filtering the pairs based on dam and sire if they exist 
                    {
                        pairBox.Items.Add(pair);
                    }

                }
                else //sire isn't populated so just get the pairs for the dam 
                {
                    // Load active pairings for selected dam
                    var pairs = await _dataManager.GetActivePairingsByAnimalId(selectedDam.Id);

                    foreach (var pair in pairs) //FIXME IDs don't really make sense it should find the dam and sire based on pair or find the pair based on dam and sire i.e. it should be filtering the pairs based on dam and sire if they exist 
                    {
                        pairBox.Items.Add(pair);
                    }
                }
            }
            finally
            {
                _spinner.Hide();
            }
        }

        //sire combo box changed
        //set pairs accordingly 
        public async Task HandleSireSelectionChangedAsync(ComboBox sireBox, ComboBox damBox, ComboBox pairBox)
        {
            if (sireBox.SelectedItem == null) return;

            try
            {
                _spinner.Show();
                var selectedSire = sireBox.SelectedItem as AnimalDto;

                // Clear existing items
                pairBox.Items.Clear();


                //check if the sire combo box is populated
                if (damBox.SelectedItem != null)
                {
                    var selectedDam = damBox.SelectedItem as AnimalDto;

                    var pairs = await _dataManager.GetActivePairingsByDamandSire(selectedDam.Id, selectedSire.Id);


                    foreach (var pair in pairs) //FIXME IDs don't really make sense it should find the dam and sire based on pair or find the pair based on dam and sire i.e. it should be filtering the pairs based on dam and sire if they exist 
                    {
                        pairBox.Items.Add(pair);
                    }

                }
                else //sire isn't populated so just get the pairs for the dam 
                {
                    // Load active pairings for selected dam
                    var pairs = await _dataManager.GetActivePairingsByAnimalId(selectedSire.Id);

                    foreach (var pair in pairs) //FIXME IDs don't really make sense it should find the dam and sire based on pair or find the pair based on dam and sire i.e. it should be filtering the pairs based on dam and sire if they exist 
                    {
                        pairBox.Items.Add(pair);
                    }
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

        //public async Task HandleAddLitterClickAsync(string litterId, string litterName, int pairId, 
        //    DateTimePicker datePicker, TextBox numPups, TextBox numMales, TextBox numFemales, TextBox notes)
        //{
        //    try
        //    {
        //        _spinner.Show();

        //        // Validate inputs
        //        if (string.IsNullOrWhiteSpace(litterId) || string.IsNullOrWhiteSpace(litterName))
        //        {
        //            MessageBox.Show("Please fill in all required fields", "Validation Error", 
        //                MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            return;
        //        }

        //        var litter = new Litter
        //        {
        //            LitterId = litterId,
        //            Name = litterName,
        //            PairId = pairId,
        //            DateOfBirth = datePicker.Value,
        //            NumPups = !string.IsNullOrWhiteSpace(numPups.Text) ? int.Parse(numPups.Text) : null,
        //            NumMale = !string.IsNullOrWhiteSpace(numMales.Text) ? int.Parse(numMales.Text) : null,
        //            NumFemale = !string.IsNullOrWhiteSpace(numFemales.Text) ? int.Parse(numFemales.Text) : null,
        //            Notes = notes.Text,
        //            CreatedOn = DateTime.Now,
        //            LastUpdated = DateTime.Now
        //        };

        //        await _dataManager.SaveLitterAsync(litter);

        //        MessageBox.Show("Litter added successfully!", "Success", 
        //            MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    }
        //    finally
        //    {
        //        _spinner.Hide();
        //    }
        //}

        public void HandleAddPairingToGridClick(string id, ComboBox damBox, ComboBox sireBox, ComboBox projectBox, 
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
                    var project = (await _dataManager.GetProjectsAsync())
                        .First(p => p.Name == row.Cells["Project"].Value.ToString());

                    var pairing = new Pairing
                    {
                        pairingId = row.Cells["PairingID"].Value.ToString(),
                        DamId = int.Parse(row.Cells["Dam"].Value.ToString().Split('-')[0].Trim()),
                        SireId = int.Parse(row.Cells["Sire"].Value.ToString().Split('-')[0].Trim()),
                        ProjectId = project.Id,
                        PairingStartDate = DateTime.Parse(row.Cells["PairingDate"].Value.ToString()),
                        CreatedOn = DateTime.Now,
                        LastUpdated = DateTime.Now
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

        //public async Task HandleSaveAllLittersAsync(DataGridView grid)
        //{
        //    try
        //    {
        //        _spinner.Show();

        //        if (grid.Rows.Count == 0)
        //        {
        //            MessageBox.Show("Please add at least one litter to the list", "Validation Error", 
        //                MessageBoxButtons.OK, MessageBoxIcon.Error);
        //            return;
        //        }

        //        foreach (DataGridViewRow row in grid.Rows)
        //        {
        //            var litter = new Litter
        //            {
        //                LitterId = row.Cells["LitterID"].Value.ToString(),
        //                Name = row.Cells["Name"].Value.ToString(),
        //                PairId = int.Parse(row.Cells["PairID"].Value.ToString()),
        //                DateOfBirth = DateTime.Parse(row.Cells["BirthDate"].Value.ToString()),
        //                NumPups = !string.IsNullOrWhiteSpace(row.Cells["NumPups"].Value?.ToString()) 
        //                    ? int.Parse(row.Cells["NumPups"].Value.ToString()) : null,
        //                NumMale = !string.IsNullOrWhiteSpace(row.Cells["NumMales"].Value?.ToString()) 
        //                    ? int.Parse(row.Cells["NumMales"].Value.ToString()) : null,
        //                NumFemale = !string.IsNullOrWhiteSpace(row.Cells["NumFemales"].Value?.ToString()) 
        //                    ? int.Parse(row.Cells["NumFemales"].Value.ToString()) : null,
        //                Notes = row.Cells["Notes"].Value?.ToString(),
        //                CreatedOn = DateTime.Now,
        //                LastUpdated = DateTime.Now
        //            };

        //            await _dataManager.SaveLitterAsync(litter);
        //        }

        //        MessageBox.Show($"Successfully added {grid.Rows.Count} litters!", "Success", 
        //            MessageBoxButtons.OK, MessageBoxIcon.Information);

        //        grid.Rows.Clear();
        //    }
        //    finally
        //    {
        //        _spinner.Hide();
        //    }
        //}

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
