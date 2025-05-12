using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RATAPPLibrary.Data.Models.Genetics
{
    public class ChromosomePair
    {
        [Key]
        public Guid PairId { get; set; }

        [Required]
        public Guid MaternalChromosomeId { get; set; }

        [Required]
        public Guid PaternalChromosomeId { get; set; }

        [Required]
        [StringLength(50)]
        public string InheritancePattern { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual Chromosome MaternalChromosome { get; set; }
        public virtual Chromosome PaternalChromosome { get; set; }
        public virtual ICollection<Gene> Genes { get; set; }
        public virtual ICollection<Genotype> Genotypes { get; set; }
    }
}
