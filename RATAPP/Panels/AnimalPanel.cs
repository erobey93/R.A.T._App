using RATAPP.Forms;
using RATAPPLibrary.Data.DbContexts;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Genetics;
using RATAPPLibrary.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RATAPP.Panels
{
    public partial class AnimalPanel : Panel, INavigable
    {
        // Services
        private readonly RatAppDbContextFactory _contextFactory;
        private readonly AnimalService _animalService;
        private readonly LineageService _lineageService;
        private readonly TraitService _traitService;
        private readonly SpeciesService _speciesService;
        private readonly SemaphoreSlim _loadingSemaphore = new SemaphoreSlim(1, 1);

        // State
        private bool _isEditMode = false;
        private AnimalDto _animal;
        private AnimalDto _dam;
        private AnimalDto _sire;
        private AnimalDto[] _dams;
        private AnimalDto[] _sires;
        private AnimalDto[] _allAnimals;
        private readonly RATAppBaseForm _parentForm;

        // UI Controls
        private Label animalNameLabel, regNumLabel, speciesLabel, sexLabel, varietyLabel;
        private Label colorLabel, genotypeLabel, breederInfoLabel, commentsLabel;
        private Label damLabel, sireLabel, inbredLabel, earTypeLabel, markingsLabel;
        private Label weightLabel, cageNumberLabel, dobLabel, dodLabel, numGensLabel;

        private TextBox animalNameTextBox, regNumTextBox, speciesTextBox, sexTextBox;
        private TextBox varietyTextBox, colorTextBox, genotypeTextBox, breederInfoTextBox;
        private TextBox commentsTextBox, damTextBox, sireTextBox, inbredTextBox;
        private TextBox earTypeTextBox, markingsTextBox, dobTextBox, dodTextBox;
        private TextBox weightTextBox, cageNumberTextBox, numGensTextBox;

        private ComboBox speciesComboBox, sexComboBox, varietyComboBox, colorComboBox;
        private ComboBox genotypeComboBox, ancestryComboBox, damComboBox, sireComboBox;
        private ComboBox earTypeComboBox, markingComboBox, breederInfoComboBox;
        private ComboBox cageNumberComboBox;

        private PictureBox animalPhotoBox;
        private PictureBox loadingSpinner;
        private Button inbredButton, saveButton, updateButton, cancelButton;
        private Button documentsButton, healthButton, indAncestryButton;
        private Button prevButton, nextButton, geneticsButton, breedingHistoryButton;

        private FlowLayoutPanel thumbnailPanel;
        private TableLayoutPanel mainContainer;
        private Panel dataEntryPanel;
        private Panel imagePanel;
        private Panel actionButtonPanel;
        private Panel featureButtonPanel;

        // Test image paths (TODO: Move to configuration)
        private List<string> imagePaths = new List<string>
        {
            "C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\AnimalPics\\00S0S_e4sHXNFmkdY_0t20CI_1200x900.jpg",
            "C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\AnimalPics\\00v0v_hu4XyatVj1Q_0t20CI_1200x900.jpg",
            "C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\AnimalPics\\IMG_0214.JPG",
            "C:\\Users\\earob\\source\\repos\\RATAPP_2\\R.A.T._App\\RATAPP\\Resources\\AnimalPics\\IMG_5197.JPG"
        };

        public AnimalPanel(RATAppBaseForm parentForm, RatAppDbContextFactory contextFactory, AnimalDto[] allAnimals, AnimalDto currAnimal)
        {
            _contextFactory = contextFactory;
            _animalService = new AnimalService(contextFactory);
            _traitService = new TraitService(contextFactory);
            _speciesService = new SpeciesService(contextFactory);
            _lineageService = new LineageService(contextFactory);

            parentForm.SetActivePanel(this);
            _parentForm = parentForm;
            _allAnimals = allAnimals;

            InitializeComponent(currAnimal);
            InitializeLoadingSpinner();
        }

        private void InitializeComponent(AnimalDto animal)
        {
            _animal = animal;
            if (_animal == null)
            {
                _isEditMode = true;
            }

            this.Size = new Size(1200, 800);
            this.BackColor = Color.White;
            this.Padding = new Padding(20);

            IntializeButtons();
            InitializeBottomButtons();

            if (animal != null && !_isEditMode)
            {
                LoadAnimalDataAsync(animal.Id);
            }
            else
            {
                InitializeComboBoxes();
            }

            InitializeLabels();
            InitializePhotoBox();
            InitializeThumbnailPanel();
        }

        private async void LoadAnimalDataAsync(int animalId)
        {
            try
            {
                await _loadingSemaphore.WaitAsync();
                ShowLoadingIndicator();

                var animal = await _animalService.GetAnimalByIdAsync(animalId);
                _animal = animal;

                await SetDefaultDamAndSire();
                await GetAnimalDamAndSire();
                InitializeTextBoxes(_animal);
            }
            catch (Exception ex)
            {
                LogError(ex);
                ShowMessage("Error loading animal data. Please try again.");
            }
            finally
            {
                HideLoadingIndicator();
                _loadingSemaphore.Release();
            }
        }

        private async Task SetDefaultDamAndSire()
        {
            var defaultAnimal = await GetOrCreateDefaultAnimal(0);
            _dam = defaultAnimal;
            _sire = defaultAnimal;
        }

        private async Task GetAnimalDamAndSire()
        {
            if (_animal != null)
            {
                try
                {
                    await _loadingSemaphore.WaitAsync();
                    var dam = await _lineageService.GetDamByAnimalId(_animal.Id);
                    if (dam != null)
                    {
                        _dam = await _animalService.MapSingleAnimaltoDto(dam);
                    }

                    var sire = await _lineageService.GetSireByAnimalId(_animal.Id);
                    if (sire != null)
                    {
                        _sire = await _animalService.MapSingleAnimaltoDto(sire);
                    }
                }
                finally
                {
                    _loadingSemaphore.Release();
                }
            }
        }

        private async Task<AnimalDto> GetOrCreateDefaultAnimal(int? id)
        {
            if (id.HasValue && id.Value != 0)
            {
                try
                {
                    await _loadingSemaphore.WaitAsync();
                    var existingAnimal = await _animalService.GetAnimalByIdAsync(id.Value);
                    if (existingAnimal != null)
                    {
                        return existingAnimal;
                    }
                }
                finally
                {
                    _loadingSemaphore.Release();
                }
            }

            return new AnimalDto
            {
                Id = 0,
                regNum = "0",
                name = "Unknown",
                sex = "Unknown",
                DateOfBirth = DateTime.Now,
                DateOfDeath = DateTime.Now,
                species = "Unknown",
                comment = "Unknown",
                imageUrl = "",
                Line = "1",
                damId = null,
                sireId = null,
                variety = "Unknown",
                color = "Unknown",
                breeder = "TLDR",
                genotype = "XYZ"
            };
        }

        private async Task<List<string>> GetComboBoxItemsFromDatabase(Control control)
        {
            try
            {
                await _loadingSemaphore.WaitAsync();
                var options = new List<string>();
                string animalSpecies = GetAnimalSpecies();

                switch (control.Name)
                {
                    case "speciesComboBox":
                        var species = await _speciesService.GetAllSpeciesAsync();
                        options.AddRange((List<string>)species);
                        break;

                    case "sexComboBox":
                        options.AddRange(new[] { "Male", "Female", "Unknown", "Other" });
                        break;

                    case "varietyComboBox":
                        var animalCoats = await GetSpeciesTraitsByType(animalSpecies, "Coat Type");
                        options.AddRange(animalCoats);
                        break;

                    case "colorComboBox":
                        var animalColors = await GetSpeciesTraitsByType(animalSpecies, "Color");
                        options.AddRange(animalColors);
                        break;

                    case "damComboBox":
                        if (animalSpecies != "Unknown")
                        {
                            _dams = await _animalService.GetAnimalInfoBySexAndSpecies("Female", animalSpecies);
                        }
                        else
                        {
                            _dams = await _animalService.GetAnimalsBySex("Female");
                        }
                        foreach (var dam in _dams)
                        {
                            options.Add(dam.name);
                        }
                        break;

                    case "sireComboBox":
                        if (animalSpecies != "Unknown")
                        {
                            _sires = await _animalService.GetAnimalInfoBySexAndSpecies("Male", animalSpecies);
                        }
                        else
                        {
                            _sires = await _animalService.GetAnimalsBySex("Male");
                        }
                        foreach (var sire in _sires)
                        {
                            options.Add(sire.name);
                        }
                        break;

                    case "earTypeComboBox":
                        var animalEars = await GetSpeciesTraitsByType(animalSpecies, "Ear Type");
                        options.AddRange(animalEars);
                        break;

                    case "markingComboBox":
                        var animalMarkings = await GetSpeciesTraitsByType(animalSpecies, "Marking");
                        options.AddRange(animalMarkings);
                        break;

                    default:
                        options.AddRange(new[] { "Option1", "Option2", "Option3" });
                        break;
                }

                return options;
            }
            finally
            {
                _loadingSemaphore.Release();
            }
        }

        private async Task<List<string>> GetSpeciesTraitsByType(string species, string type)
        {
            try
            {
                await _loadingSemaphore.WaitAsync();
                return (List<string>)await _traitService.GetTraitsByTypeAndSpeciesAsync(type, species);
            }
            catch (Exception ex)
            {
                LogError(ex);
                ShowMessage("Failed to get traits for the species. Please try again.");
                return new List<string>();
            }
            finally
            {
                _loadingSemaphore.Release();
            }
        }

        public async Task RefreshDataAsync()
        {
            try
            {
                await _loadingSemaphore.WaitAsync();
                ShowLoadingIndicator();

                if (int.TryParse(regNumTextBox.Text, out int id))
                {
                    _animal = await _animalService.GetAnimalByIdAsync(_animal.Id);
                    ShowMessage("Data refresh complete");
                }
                else
                {
                    ShowMessage("Could not get animal");
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                ShowMessage($"An error occurred: {ex.Message}");
            }
            finally
            {
                HideLoadingIndicator();
                _loadingSemaphore.Release();
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingSpinner != null)
            {
                loadingSpinner.Visible = true;
                CenterLoadingSpinner();
                this.Refresh();
            }
        }

        private void HideLoadingIndicator()
        {
            if (loadingSpinner != null)
            {
                loadingSpinner.Visible = false;
                this.Refresh();
            }
        }

        private void CenterLoadingSpinner()
        {
            if (loadingSpinner != null)
            {
                loadingSpinner.Location = new Point(
                    (ClientSize.Width - loadingSpinner.Width) / 2,
                    (ClientSize.Height - loadingSpinner.Height) / 2
                );
            }
        }

        private void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }

        private void LogError(Exception ex)
        {
            // TODO: Implement proper logging
            Console.WriteLine(ex.Message);
        }

        // ... rest of the UI initialization methods remain unchanged
    }
}
