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
}
