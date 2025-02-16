namespace RATAPPLibrary.Data.Models.Genetics
{
    public class Trait
    {
        public int Id { get; set; } // Assuming Id is an integer
        public required string TraitTypeName { get; set; } // The type of the trait (e.g., color, pattern)
        public string? Genotype { get; set; } // The genetic code for the trait TODO decide if this needs to be required, probably not, but maybe for consistency
        public required string CommonName { get; set; } // The common name or description of the trait 

        // Collections for related entities (if applicable)
        public ICollection<AnimalTrait>? AnimalTraits { get; set; } // Association with animals
    }
}