using System;
using System.Linq;
using System.Windows.Forms;
using RATAPPLibrary.Data.Models;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Data.Models.Animal_Management;

namespace RATAPP.Helpers
{
    /// <summary>
    /// Provides centralized validation logic for form inputs.
    /// Extend this helper for different form validation needs.
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Generic validation for required fields
        /// Can be used in any form with required inputs
        /// </summary>
        /// <param name="controls">Array of controls to validate</param>
        /// <returns>True if all controls have non-empty values</returns>
        public static bool ValidateRequiredFields(params Control[] controls)
        {
            if (controls == null || controls.Length == 0)
                return false;

            return controls.All(c => {
                if (c is ComboBox combo)
                    return combo.SelectedItem != null;
                if (c is TextBox text)
                    return !string.IsNullOrWhiteSpace(text.Text);
                return true;
            });
        }

        /// <summary>
        /// Validates a single pairing entry
        /// </summary>
        /// <param name="pairingId">The pairing ID to validate</param>
        /// <param name="dam">The selected dam</param>
        /// <param name="sire">The selected sire</param>
        /// <param name="project">The selected project</param>
        /// <param name="errorMessage">Output parameter containing error message if validation fails</param>
        /// <returns>True if validation passes, false otherwise</returns>
        public static bool ValidatePairingInputs(
            string pairingId,
            AnimalDto dam,
            AnimalDto sire,
            Project project,
            out string errorMessage)
        {
            errorMessage = string.Empty;

            // Check for null values
            if (string.IsNullOrWhiteSpace(pairingId))
            {
                errorMessage = "Pairing ID is required";
                return false;
            }

            if (dam == null)
            {
                errorMessage = "Dam selection is required";
                return false;
            }

            if (sire == null)
            {
                errorMessage = "Sire selection is required";
                return false;
            }

            if (project == null)
            {
                errorMessage = "Project selection is required";
                return false;
            }

            // Validate pairing ID format (can be customized based on requirements)
            if (!ValidatePairingIdFormat(pairingId))
            {
                errorMessage = "Invalid Pairing ID format";
                return false;
            }

            // Validate dam and sire are different animals
            if (dam.Id == sire.Id)
            {
                errorMessage = "Dam and Sire cannot be the same animal";
                return false;
            }

            // Validate dam and sire are of the same species
            if (dam.SpeciesId != sire.SpeciesId)
            {
                errorMessage = "Dam and Sire must be of the same species";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates multiple pairings for bulk operations
        /// </summary>
        /// <param name="pairings">List of pairings to validate</param>
        /// <param name="errorMessage">Output parameter containing error message if validation fails</param>
        /// <returns>True if all pairings are valid, false otherwise</returns>
        public static bool ValidateMultiplePairings(DataGridView pairingsGrid, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (pairingsGrid.Rows.Count == 0)
            {
                errorMessage = "At least one pairing is required";
                return false;
            }

            // Check for duplicate pairing IDs
            var pairingIds = pairingsGrid.Rows
                .Cast<DataGridViewRow>()
                .Select(r => r.Cells["PairingID"].Value?.ToString())
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList();

            if (pairingIds.Count != pairingIds.Distinct().Count())
            {
                errorMessage = "Duplicate Pairing IDs found";
                return false;
            }

            // Validate each row
            foreach (DataGridViewRow row in pairingsGrid.Rows)
            {
                var pairingId = row.Cells["PairingID"].Value?.ToString();
                if (!ValidatePairingIdFormat(pairingId))
                {
                    errorMessage = $"Invalid Pairing ID format: {pairingId}";
                    return false;
                }

                // Add additional row validations as needed
            }

            return true;
        }

        /// <summary>
        /// Validates the format of a pairing ID
        /// Customize this method based on your pairing ID requirements
        /// </summary>
        private static bool ValidatePairingIdFormat(string pairingId)
        {
            if (string.IsNullOrWhiteSpace(pairingId))
                return false;

            // Add your pairing ID format validation logic here
            // For example: must be alphanumeric and between 3-10 characters
            return System.Text.RegularExpressions.Regex.IsMatch(pairingId, @"^[a-zA-Z0-9]{3,10}$");
        }

        /// <summary>
        /// Shows an error message to the user
        /// </summary>
        /// <param name="message">The error message to display</param>
        public static void ShowError(string message)
        {
            MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Shows a success message to the user
        /// </summary>
        /// <param name="message">The success message to display</param>
        public static void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
