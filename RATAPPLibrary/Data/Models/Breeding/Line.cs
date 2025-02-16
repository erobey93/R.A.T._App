namespace RATAPPLibrary.Data.Models.Breeding
{
    public class Line
    {
        public int Id { get; set; } // Assuming Id is an integer
        public int StockId { get; set; } // Assuming StockId is an integer
        public required string Name { get; set; } // The name of the line
        public string? Description { get; set; } // A description of the line
        public string? Notes { get; set; } // Additional notes about the line

        // Navigation Properties for EF
        public Stock? Stock { get; set; } // Navigation property to Stock table
        public ICollection<Animal>? Animals { get; set; } // Related animals within the line
    }
}
