namespace RATAPPLibrary.Data.Models.Genetics
{
    public class TraitType
    {
        public int Id { get; set; } // Primary key for TraitType
        public required string Type { get; set; } //Right now physical, medical, behavioral
        public required string Name { get; set; } // Trait type name (e.g., "Color", "Pattern"), helps to distinguish between traits of the same type i.e. physical could be color, coat type, etc
        public string? Description { get; set; } // Optional description of the trait type

        // Collections for related entities 
        public virtual ICollection<Trait>? Traits { get; set; } // Association with traits
    }
}
