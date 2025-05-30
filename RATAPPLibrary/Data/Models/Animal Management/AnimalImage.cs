namespace RATAPPLibrary.Data.Models.Animal_Management
{
    public class AnimalImage
    {
        public int Id { get; set; } // Assuming Id is an integer
        public required int AnimalId { get; set; } // Assuming AnimalId is an integer
        public required string ImageUrl { get; set; } //TODO url for now there will be a main animal image and then these will be the images in the carousol below 
        public DateTime CreatedOn { get; set; } // The creation date of the record
        public DateTime LastUpdated { get; set; } // The last updated timestamp

        // Navigation Property for EF
        public Animal? Animal { get; set; } // Navigation property to Animal table
    }
}
