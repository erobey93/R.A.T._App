using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RATAPPLibrary.Data.Models.Genetics;

namespace RATAPPLibrary.Services.Genetics.Interfaces
{
    /// <summary>
    /// Service for managing genes and alleles
    /// </summary>
    public class CreateGeneRequest
    {
        public string Name { get; set; }
        public string CommonName { get; set; }
        public Guid ChromosomePairId { get; set; }
        public int Position { get; set; }
        public string Category { get; set; }
        public string ImpactLevel { get; set; }
        public string ExpressionAge { get; set; }
        public string Penetrance { get; set; }
        public string Expressivity { get; set; }
        public bool RequiresMonitoring { get; set; }
    }

    public class CreateAlleleRequest
    {
        public Guid GeneId { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public bool IsWildType { get; set; }
        public string Phenotype { get; set; }
        public string RiskLevel { get; set; }
        public string ManagementNotes { get; set; }
    }

    public class AssignGenotypeRequest
    {
        public int AnimalId { get; set; }
        public Guid ChromosomePairId { get; set; }
        public Guid MaternalAlleleId { get; set; }
        public Guid PaternalAlleleId { get; set; }
        public int TraitId { get; set; }
        public string GenotypeCode { get; set; }    
    }

    public interface IGeneService
    {
        /// <summary>
        /// Creates a new gene on a chromosome pair
        /// </summary>
        /// <param name="request">Details for creating the gene</param>
        /// <returns>The created gene</returns>
        /// <exception cref="InvalidOperationException">Thrown when validation fails</exception>
        Task<Gene> CreateGeneAsync(CreateGeneRequest request);

        /// <summary>
        /// Creates a new allele for a gene
        /// </summary>
        /// <param name="request">Details for creating the allele</param>
        /// <returns>The created allele</returns>
        /// <exception cref="InvalidOperationException">Thrown when validation fails</exception>
        Task<Allele> CreateAlleleAsync(CreateAlleleRequest request);

        /// <summary>
        /// Gets all genes on a specific chromosome pair
        /// </summary>
        /// <param name="chromosomePairId">ID of the chromosome pair</param>
        /// <returns>List of genes on the chromosome pair</returns>
        Task<List<Gene>> GetGenesForChromosomePairAsync(Guid chromosomePairId);

        /// <summary>
        /// Gets all alleles for a specific gene
        /// </summary>
        /// <param name="geneId">ID of the gene</param>
        /// <returns>List of alleles for the gene</returns>
        Task<List<Allele>> GetAllelesForGeneAsync(Guid geneId);

        /// <summary>
        /// Gets a map of genes and their alleles for an animal
        /// </summary>
        /// <param name="animalId">ID of the animal</param>
        /// <returns>Dictionary mapping gene names to lists of allele names</returns>
        Task<Dictionary<string, List<string>>> GetGeneMapForAnimalAsync(int animalId);

        /// <summary>
        /// Assigns genotype information to an animal
        /// </summary>
        /// <param name="request">Details for assigning the genotype</param>
        /// <returns>The created genotype</returns>
        /// <exception cref="InvalidOperationException">Thrown when validation fails</exception>
        Task<Genotype> AssignGenotypeToAnimalAsync(AssignGenotypeRequest request);

        /// <summary>
        /// Gets all genes with a specific effect category and impact level
        /// </summary>
        /// <param name="category">Category of the gene (e.g., "physical", "medical", "behavioral")</param>
        /// <param name="impactLevel">Impact level (e.g., "cosmetic", "health", "critical")</param>
        /// <returns>List of genes matching the criteria</returns>
        Task<List<Gene>> GetGenesWithEffectAsync(string category, string impactLevel);

        /// <summary>
        /// Gets a gene by its name
        /// </summary>
        /// <param name="name">Name of the gene</param>
        /// <returns>The gene if found, null otherwise</returns>
        Task<Gene> GetGeneByNameAsync(string name);

        /// <summary>
        /// Gets an allele by its symbol
        /// </summary>
        /// <param name="geneId">ID of the gene</param>
        /// <param name="symbol">Symbol of the allele</param>
        /// <returns>The allele if found, null otherwise</returns>
        Task<Allele> GetAlleleBySymbolAsync(Guid geneId, string symbol);
    }
}
