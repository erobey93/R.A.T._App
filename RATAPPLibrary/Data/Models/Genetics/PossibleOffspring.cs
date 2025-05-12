using System;
using System.ComponentModel.DataAnnotations;

namespace RATAPPLibrary.Data.Models.Genetics
{
    public class PossibleOffspring
    {
        [Key]
        public Guid OffspringId { get; set; }

        [Required]
        public Guid CalculationId { get; set; }

        [Required]
        [Range(0, 1)]
        public float Probability { get; set; }

        [Required]
        [StringLength(200)]
        public string Phenotype { get; set; }

        [Required]
        [StringLength(100)]
        public string GenotypeDescription { get; set; }

        [Required]
        [StringLength(100)]
        public string MaternalAlleles { get; set; }

        [Required]
        [StringLength(100)]
        public string PaternalAlleles { get; set; }

        // Navigation property
        public virtual BreedingCalculation BreedingCalculation { get; set; }
    }
}
