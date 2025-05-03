using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Breeding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RATAPP
{

public static class DropdownHelper
{
    public class ListItem<T>
    {
        public string Display { get; set; }
        public T Value { get; set; }

        public override string ToString()
        {
            return Display;
        }
    }

    public static async Task PopulateDropdownAsync<T>(
        ComboBox comboBox,
        Func<Task<List<T>>> fetchDataFunc,
        string displayMember,
        string valueMember = null) where T : class
    {
        comboBox.Items.Clear();
        try
        {
            var data = await fetchDataFunc();
            foreach (var item in data)
            {
                var displayValue = item.GetType().GetProperty(displayMember)?.GetValue(item)?.ToString();
                object actualValue = item; // Default to the entire object if no valueMember

                if (!string.IsNullOrEmpty(valueMember))
                {
                    actualValue = item.GetType().GetProperty(valueMember)?.GetValue(item);
                }

                if (displayValue != null)
                {
                    comboBox.Items.Add(new ListItem<T> { Display = displayValue, Value = item });
                }
            }
            comboBox.DisplayMember = "Display";
            comboBox.ValueMember = "Value";
        }
        catch (Exception ex)
        {
            // Consider a more robust error handling mechanism (e.g., logging, user notification)
            MessageBox.Show($"Error populating dropdown: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Helper for fetching traits
    public static async Task PopulateTraitDropdownAsync(
        ComboBox comboBox,
        Func<string> getSpeciesFunc,
        Func<string, string, Task<List<string>>> getTraitsFunc,
        string traitType)
    {
        comboBox.Items.Clear();
        string species = getSpeciesFunc();
        if (species != "Unknown")
        {
            try
            {
                var traits = await getTraitsFunc(species, traitType);
                if (traits != null)
                {
                    comboBox.Items.AddRange(traits.ToArray());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching {traitType}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        else
        {
            // Optionally handle the "Unknown" species case here (e.g., clear the dropdown or show a message)
            comboBox.Items.Add("Please select a species first.");
            comboBox.Enabled = false; // Disable until a species is selected
        }
        comboBox.Enabled = true;
    }

    // Helper for fetching animals by sex and optionally species
    public static async Task<List<AnimalDto>> PopulateAnimalDropdownAsync( // Return the list
     ComboBox comboBox,
     Func<string> getSpeciesFunc,
     Func<string, string, Task<AnimalDto[]>> getAnimalsBySexAndSpeciesFunc,
     Func<string, Task<AnimalDto[]>> getAnimalsBySexFunc,
     string animalSex)
    {
        comboBox.Items.Clear();
        List<AnimalDto> animals = new List<AnimalDto>(); // Local list to build

        string species = getSpeciesFunc();

        try
        {
            if (species != "Unknown")
            {
                AnimalDto[] animalArray = await getAnimalsBySexAndSpeciesFunc(animalSex, species); //get as array 
                animals.AddRange(animalArray); //add to the list 
            }
            else
            {
                AnimalDto[] animalArray = await getAnimalsBySexFunc(animalSex);
                animals.AddRange(animalArray); //add to the list
            }

            if (animals != null)
            {
                foreach (var animal in animals)
                {
                    comboBox.Items.Add(new ListItem<AnimalDto> { Display = animal.name, Value = animal });
                }
                comboBox.DisplayMember = "Display";
                comboBox.ValueMember = "Value";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error fetching {animalSex} animals: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null; // Or an empty list to indicate failure
        }
        return animals; // Return the populated list
    }

    //Populate species dropdown async
    // Helper for fetching animals by sex and optionally species
    public static async Task <List<Species>> PopulateSpeciesDropdownAsync( // Return the list
     ComboBox comboBox,
      Func<Task<List<Species>>> getSpeciesFunc
        )
    {
        comboBox.Items.Clear();
        List<Species> species = await getSpeciesFunc(); // get the list of species from the get species func 

        try
        {
           

            if (species != null)
            {
                foreach (var specie in species)
                {
                    comboBox.Items.Add(new ListItem<Species> { Display = specie.CommonName, Value = specie });
                }
                comboBox.DisplayMember = "Display";
                comboBox.ValueMember = "Value";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error fetching species: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null; // Or an empty list to indicate failure
        }
        return species; // Return the populated list
    }

        //Populate projects dropdown async 
        public static async Task<List<Project>> PopulateProjectsDropdownAsync( // Return the list
     ComboBox comboBox,
      Func<Task<List<Project>>> getProjectsFunc
        )
        {
            comboBox.Items.Clear();
            List<Project> projects = await getProjectsFunc(); // get the list of species from the get species func 

            try
            {


                if (projects != null)
                {
                    foreach (var project in projects)
                    {
                        comboBox.Items.Add(new ListItem<Project> { Display = project.Name, Value = project });
                    }
                    comboBox.DisplayMember = "Display";
                    comboBox.ValueMember = "Value";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching species: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null; // Or an empty list to indicate failure
            }
            return projects; // Return the populated list
        }
        //Populate pairings dropdown async
        //Populate litters dropdown async 
    }

}