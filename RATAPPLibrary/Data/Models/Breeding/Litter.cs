namespace RATAPPLibrary.Data.Models.Breeding
{
    public class Litter
    {
        public required int Id { get; set; } // Unique identifier for the litter
        public required int PairId { get; set; } // The unique identifier for the parental pairing
        public required string Name { get; set; } // The name of the litter
        public int? NumPups { get; set; } // The number of pups in the litter
        public DateTime? DateOfBirth { get; set; } // The date the litter was born
        public int? NumFemale { get; set; } // The number of female pups in the litter
        public int? NumMale { get; set; } // The number of male pups in the litter 
        public string? Notes { get; set; } // A description of the litter

        // Navigation property to the Pairing entity
        public Pairing? Pair { get; set; } // Represents the parental pairing for this litter

        // Timestamps for tracking creation and updates
        public DateTime CreatedOn { get; set; } // The date the litter record was created
        public DateTime LastUpdated { get; set; } // The date the litter record was last updated

        // Navigation property to the Breeders associated with this litter
        public virtual ICollection<Breeder>? Breeders { get; set; } // Allows multiple breeders to be associated 
        public virtual ICollection<Animal>? Animals { get; set; } // The animals (offspring) in the litter
    }
}
