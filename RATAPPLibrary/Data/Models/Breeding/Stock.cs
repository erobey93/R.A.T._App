namespace RATAPPLibrary.Data.Models.Breeding
{
    public class Stock
    {
        public int Id { get; set; } // Assuming Id is an integer
        public int BreederId { get; set; } // Assuming BreederId is an integer
        public int SpeciesId { get; set; } // Assuming SpeciesId is an integer
        public string? Description { get; set; } // Assuming Description is a string

        // Navigation Properties for EF (Entity Framework)
        public Breeder? Breeder { get; set; } // Navigation property to Breeder table
        public Species? Species { get; set; } // Navigation property to Species table

        // Collections for related entities
        public ICollection<Animal>? Animals { get; set; } // Related Animals within the Stock
    }
}
