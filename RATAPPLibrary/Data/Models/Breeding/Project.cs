namespace RATAPPLibrary.Data.Models.Breeding
{
    public class Project
    {
        public int Id { get; set; } // Unique identifier for the project
        public required string Name { get; set; } // The name of the project

        // Foreign Keys
        public required int LineId { get; set; } // Foreign key for the associated line

        // Project Details
        public string? Description { get; set; } // A description of the project's purpose or goals
        public string? Notes { get; set; } // Any additional notes or observations about the project
        public DateTime CreatedOn { get; set; } // The date the project was created
        public DateTime LastUpdated { get; set; } // The date the project was last updated

        // Navigation Properties (nullable)
        public Line? Line { get; set; } // Navigation property for the associated line
    }
}
