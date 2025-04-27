using RATAPPLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
    public static async Task<List<Animal>> PopulateAnimalDropdownAsync( // Return the list
     ComboBox comboBox,
     Func<string> getSpeciesFunc,
     Func<string, string, Task<List<Animal>>> getAnimalsBySexAndSpeciesFunc,
     Func<string, Task<List<Animal>>> getAnimalsBySexFunc,
     string animalSex)
    {
        comboBox.Items.Clear();
        List<Animal> animals = new List<Animal>(); // Local list to build

        string species = getSpeciesFunc();

        try
        {
            if (species != "Unknown")
            {
                animals = await getAnimalsBySexAndSpeciesFunc(animalSex, species);
            }
            else
            {
                animals = await getAnimalsBySexFunc(animalSex);
            }

            if (animals != null)
            {
                foreach (var animal in animals)
                {
                    comboBox.Items.Add(new ListItem<Animal> { Display = animal.Name, Value = animal });
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
}