using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RATAPPLibrary.Data.Models.Breeding;

namespace RATAPPLibrary.Data.Models.Genetics
{
    public class BreedingCalculation
    {
        [Key]
        public Guid CalculationId { get; set; }

        [Required]
        public int PairingId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual Pairing Pairing { get; set; }
        public virtual ICollection<PossibleOffspring> PossibleOffspring { get; set; }
    }

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
