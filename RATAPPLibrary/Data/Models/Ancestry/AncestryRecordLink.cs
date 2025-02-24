namespace RATAPPLibrary.Data.Models.Ancestry
{
    public class AncestryRecordLink
    {
        public int AncestryRecordId { get; set; } // Assuming AncestryRecordId is an integer
        public int AnimalId { get; set; } // Assuming AnimalId is an integer

        // Navigation Properties for EF
        // These properties are used for Entity Framework to establish relationships
        // between the AncestryRecordLink and the related entities.
        // They are not required for the database schema but are useful for querying related data.
        // in this case, they are used to link the AncestryRecordLink to the AncestryRecord and Animal entities.
        // The virtual keyword allows for lazy loading of related entities.
        // This means that the related entities will only be loaded from the database when they are accessed,
        // which can improve performance in some cases.
        public virtual AncestryRecord? AncestryRecord { get; set; } // Navigation property to AncestryRecord table
        public virtual Animal? Animal { get; set; } // Navigation property to Animal tables
    }
}
