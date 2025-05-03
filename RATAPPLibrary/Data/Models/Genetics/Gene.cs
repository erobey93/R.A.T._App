using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RATAPPLibrary.Data.Models.Genetics
{
    public class Gene
    {
        [Key]
        public Guid GeneId { get; set; }

        [Required]
        public Guid ChromosomePairId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string CommonName { get; set; }

        [Required]
        public int Position { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [StringLength(20)]
        public string ExpressionAge { get; set; } // birth/juvenile/adult

        [Required]
        [StringLength(20)]
        public string Penetrance { get; set; } // complete/incomplete

        [Required]
        [StringLength(20)]
        public string Expressivity { get; set; } // variable/consistent

        [Required]
        public bool RequiresMonitoring { get; set; }

        [Required]
        [StringLength(20)]
        public string Category { get; set; } // physical/medical/behavioral

        [Required]
        [StringLength(20)]
        public string ImpactLevel { get; set; } // cosmetic/health/critical

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public virtual ChromosomePair ChromosomePair { get; set; }
        public virtual ICollection<Allele> Alleles { get; set; }
    }
}
