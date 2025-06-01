namespace RATAPPLibrary.Data.Models.Breeding
{
    public class LitterImage
    {
        public int Id { get; set; } // Assuming Id is an integer
        public required int LitterId { get; set; } // Assuming AnimalId is an integer
        public required string ImageUrl { get; set; } //TODO url for now there will be a main animal image and then these will be the images in the carousol below 
        public DateTime CreatedOn { get; set; } // The creation date of the record
        public DateTime LastUpdated { get; set; } // The last updated timestamp

        // Navigation Property for EF
        public Litter? Litter { get; set; } // Navigation property to Animal table
    }
}
