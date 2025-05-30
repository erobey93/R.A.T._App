using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RATAPPLibrary.Data.Models.Animal_Management;

namespace RATAPPLibrary.Data.Models.Genetics
{
    public class Chromosome
    {
        [Key]
        public Guid ChromosomeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public int Number { get; set; }

        [Required]
        public int SpeciesId { get; set; }

        public char? Arm { get; set; }

        public int? Region { get; set; }

        public int? Band { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual Species Species { get; set; }
        public virtual ICollection<ChromosomePair> MaternalPairs { get; set; }
        public virtual ICollection<ChromosomePair> PaternalPairs { get; set; }
    }
}
