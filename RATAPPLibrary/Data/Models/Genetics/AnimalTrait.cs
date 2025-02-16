namespace RATAPPLibrary.Data.Models.Genetics
{
    public class AnimalTrait
    {
        public int Id { get; set; } // Assuming Id is an integer
        public int AnimalId { get; set; } // Foreign key to the Animal table
        public int TraitId { get; set; } // Foreign key to the Trait table

        // Navigation Properties for EF
        public Animal? Animal { get; set; } // Navigation property to Animal
        public Trait? Trait { get; set; } // Navigation property to Trait
    }
}
