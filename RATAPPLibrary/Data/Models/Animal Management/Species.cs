using RATAPPLibrary.Data.Models.Breeding;

namespace RATAPPLibrary.Data.Models
{
    public class Species
    {
        public int Id { get; set; } // Assuming Id is an integer
        public required string CommonName { get; set; } // Assuming CommonName is a string
        public string? ScientificName { get; set; } // Assuming ScientificName is a string

        // Collections for related entities
        public virtual ICollection<Stock>? Stocks { get; set; } // Related Stocks associated with the Species
    }
}
