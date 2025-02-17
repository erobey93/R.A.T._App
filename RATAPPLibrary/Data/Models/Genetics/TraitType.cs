namespace RATAPPLibrary.Data.Models.Genetics
{
    public class TraitType
    {
        public int Id { get; set; } // Primary key for TraitType
        public required string Name { get; set; } // Trait type name (e.g., "Color", "Pattern")
        public string? Description { get; set; } // Optional description of the trait type

        // Collections for related entities 
        public ICollection<Trait>? Traits { get; set; } // Association with traits
    }
}
