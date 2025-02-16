namespace RATAPPLibrary.Data.Models.Breeding
{
    public class Litter
    {
        public int Id { get; set; } // Unique identifier for the litter

        // Navigation property to the Pairing entity
        public Pairing? Pair { get; set; } // Represents the parental pairing for this litter

        // Timestamps for tracking creation and updates
        public DateTime CreatedOn { get; set; } // The date the litter record was created
        public DateTime LastUpdated { get; set; } // The date the litter record was last updated

        // Navigation property to the Breeders associated with this litter
        public ICollection<Breeder>? Breeders { get; set; } // Allows multiple breeders to be associated 
    }
}
