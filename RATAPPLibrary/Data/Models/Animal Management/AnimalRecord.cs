using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RATAPPLibrary.Data.Models
{
    public class AnimalRecord
    {
        public int Id { get; set; } // Assuming Id is an integer
        public int AnimalId { get; set; } // Assuming AnimalId is an integer
        public DateTime CreatedOn { get; set; } // The creation date of the record
        public DateTime LastUpdated { get; set; } // The last updated timestamp
        public string? Notes { get; set; } // Additional notes about the record

        // Navigation Property for EF
        public Animal? Animal { get; set; } // Navigation property to Animal table
    }
}
