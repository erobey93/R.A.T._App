namespace RATAPPLibrary.Data.Models.Ancestry
{
    public class AncestryRecord
    {
        public int Id { get; set; } // Assuming Id is an integer
        public int AnimalRecordId { get; set; } // Foreign key to the associated AnimalRecord
        public int ParentAnimalId { get; set; } // Foreign key to the parent animal
        public required string ParentType { get; set; } // Indicates maternal or paternal lineage
        public int Generation { get; set; } // Generation level (e.g., 1 for immediate parents)
        public int Sequence { get; set; } // Sequence order for ancestry visualization
        public DateTime CreatedOn { get; set; } // Record creation date
        public DateTime LastUpdated { get; set; } // Last updated timestamp

        // Navigation Properties for EF
        public AnimalRecord? AnimalRecord { get; set; } // Navigation property to AnimalRecord
        public Animal? ParentAnimal { get; set; } // Navigation property to the parent Animal
    }
}
