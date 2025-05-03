using System;
using System.ComponentModel.DataAnnotations;
using RATAPPLibrary.Data.Models;

namespace RATAPPLibrary.Data.Models.Genetics
{
    public class Genotype
    {
        [Key]
        public Guid GenotypeId { get; set; }

        [Required]
        public int AnimalId { get; set; }

        [Required]
        public Guid ChromosomePairId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual Animal Animal { get; set; }
        public virtual ChromosomePair ChromosomePair { get; set; }
    }
}
