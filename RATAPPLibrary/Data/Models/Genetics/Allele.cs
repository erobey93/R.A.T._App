using System;
using System.ComponentModel.DataAnnotations;

namespace RATAPPLibrary.Data.Models.Genetics
{
    public class Allele
    {
        [Key]
        public Guid AlleleId { get; set; }

        [Required]
        public Guid GeneId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [Required]
        [StringLength(10)]
        public string Symbol { get; set; }

        [Required]
        [StringLength(500)]
        public string Phenotype { get; set; }

        [Required]
        public bool IsWildType { get; set; }

        [Required]
        [StringLength(20)]
        public string RiskLevel { get; set; } // none/low/medium/high

        [StringLength(1000)]
        public string ManagementNotes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public virtual Gene Gene { get; set; }
    }
}
