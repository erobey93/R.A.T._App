using System;
using System.ComponentModel.DataAnnotations;
using RATAPPLibrary.Data.Models;

namespace RATAPPLibrary.Data.Models.Genetics
{
    public class GenericGenotype
    {
        [Key]
        public Guid GenotypeId { get; set; }

        [Required]
        public Guid ChromosomePairId { get; set; }

        [Required]
        public int TraitId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual ChromosomePair ChromosomePair { get; set; }
        public virtual Trait Trait { get; set; }
    }
}
