using PdfSharp.Pdf.Filters;
using RATAPPLibrary.Data.Models.Ancestry;
using RATAPPLibrary.Data.Models.Breeding;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RATAPPLibrary.Data.Models
{
    public class Animal
    {
        public int Id { get; set; } // Assuming Id is an integer
        public int StockId { get; set; } // Assuming StockId is an integer
        public int LineId { get; set; } // Assuming LineId is an integer
        public required string Sex { get; set; } // Assuming Sex is stored as a string (e.g., "Male", "Female", "Intersex", "Unknown")
        public DateTime DateOfBirth { get; set; } // Assuming DateOfBirth is stored as a DateTime
        public DateTime? DateOfDeath { get; set; } // Nullable DateTime for DateOfDeath
        public int? Age { get; set; } // Nullable integer for Age
        public string? Name { get; set; } // Assuming Name is a string

        // Navigation Properties for EF (Entity Framework) if relationships exist
        public Stock? Stock { get; set; } // Navigation property to Stock table
        public Line? Line { get; set; } // Navigation property to Line table

        // Collections for related entities, if applicable
        public ICollection<Litter>? Litters { get; set; } // Example navigation for related litters
    }
}