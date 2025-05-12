using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RATAPPLibrary.Data.Models.Genetics;

namespace RATAPPLibrary.Services.Genetics.Interfaces
{
    public class BreedingCompatibilityResult
    {
        public bool IsCompatible { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Risks { get; set; } = new List<string>();
    }

    public class InheritanceRisk
    {
        public string GeneName { get; set; }
        public string RiskLevel { get; set; }
        public string Description { get; set; }
        public string ManagementRecommendation { get; set; }
        public float Probability { get; set; }
    }

    /// <summary>
    /// Service for calculating breeding outcomes and analyzing genetic risks
    /// </summary>
    public interface IBreedingCalculationService
    {
        /// <summary>
        /// Creates a new breeding calculation for a pairing
        /// </summary>
        /// <param name="pairingId">ID of the pairing</param>
        /// <returns>The created breeding calculation</returns>
        /// <exception cref="InvalidOperationException">Thrown when pairing is invalid</exception>
        Task<BreedingCalculation> CreateBreedingCalculationAsync(int pairingId);

        /// <summary>
        /// Calculates possible offspring outcomes for a breeding calculation
        /// </summary>
        /// <param name="calculationId">ID of the breeding calculation</param>
        /// <returns>List of possible offspring with probabilities</returns>
        Task<List<PossibleOffspring>> CalculateOffspringProbabilitiesAsync(Guid calculationId);

        /// <summary>
        /// Gets all breeding calculations for a pairing
        /// </summary>
        /// <param name="pairingId">ID of the pairing</param>
        /// <returns>List of breeding calculations</returns>
        Task<List<BreedingCalculation>> GetBreedingCalculationsForPairingAsync(int pairingId);

        /// <summary>
        /// Gets trait probabilities for a breeding calculation
        /// </summary>
        /// <param name="calculationId">ID of the breeding calculation</param>
        /// <returns>Dictionary mapping trait names to probabilities</returns>
        Task<Dictionary<string, float>> GetTraitProbabilitiesAsync(Guid calculationId);

        /// <summary>
        /// Validates if two animals can be bred together
        /// </summary>
        /// <param name="damId">ID of the dam (female)</param>
        /// <param name="sireId">ID of the sire (male)</param>
        /// <returns>Breeding compatibility result with warnings and risks</returns>
        Task<BreedingCompatibilityResult> ValidateBreedingPairAsync(int damId, int sireId);

        /// <summary>
        /// Analyzes genetic risks for potential offspring
        /// </summary>
        /// <param name="calculationId">ID of the breeding calculation</param>
        /// <returns>List of inheritance risks with probabilities</returns>
        Task<List<InheritanceRisk>> AnalyzeGeneticRisksAsync(Guid calculationId);

        /// <summary>
        /// Gets a specific breeding calculation
        /// </summary>
        /// <param name="calculationId">ID of the breeding calculation</param>
        /// <returns>The breeding calculation if found, null otherwise</returns>
        Task<BreedingCalculation> GetBreedingCalculationAsync(Guid calculationId);

        /// <summary>
        /// Gets possible offspring for a breeding calculation
        /// </summary>
        /// <param name="calculationId">ID of the breeding calculation</param>
        /// <returns>List of possible offspring</returns>
        Task<List<PossibleOffspring>> GetPossibleOffspringAsync(Guid calculationId);
    }
}
