using PdfSharp.Pdf.Filters;
using RATAPPLibrary.Data.Models.Ancestry;
using RATAPPLibrary.Data.Models.Breeding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RATAPPLibrary.Data.Models
{
    public class Animal
    {
        public int Id { get; set; } // Assuming Id is an integer
        public int LineId { get; set; } // Assuming LineId is an integer
        public required string Sex { get; set; } // Assuming Sex is stored as a string (e.g., "Male", "Female", "Intersex", "Unknown")
        public DateTime DateOfBirth { get; set; } // Assuming DateOfBirth is stored as a DateTime
        public DateTime? DateOfDeath { get; set; } // Nullable DateTime for DateOfDeath
        public int Age { get; set; } // Nullable integer for Age
        public required string Name { get; set; } // Assuming Name is a string
        //public int StockId { get; set; } // Assuming StockId is an integer FIXME this should not be here but EF if fing up 

        // Navigation Properties for EF
        public Line? Line { get; set; } // Navigation property to Line table
        public ICollection<Litter>? Litters { get; set; } // Navigation property for related litters
                                                          //may make sense to have genetics and ancestry here as well TODO - add these/think through logic 

        //data rules for animal

        //TODO allow user to set age and then calculate approximate DOB based on that 
        //get age in months and years
        public int? AgeInMonths
        {
            get
            {
                if (DateOfDeath.HasValue)
                {
                    return (int)((DateOfDeath.Value - DateOfBirth).TotalDays / 30.44); // Approximation of months
                }
                return (int)((DateTime.UtcNow - DateOfBirth).TotalDays / 30.44);
            }
        }

        //get age as string for using as animal name if no name is entered 
        public string? AgeAsString
        {
            get
            {
                if (AgeInMonths == null) return null;

                if (AgeInMonths < 12)
                {
                    return $"{AgeInMonths} months";
                }

                double ageInYears = Math.Round(AgeInMonths.Value / 12.0, 1); // Convert to years with 1 decimal
                return $"{ageInYears} years";
            }
        }

        // Default Name Handling
        public string DisplayName => Name ?? Id.ToString();

        // Override ToString() for Display/logging purposes 
        public override string ToString()
        {
            return $"{DisplayName} ({Sex}, Age: {AgeAsString})";
        }
    }

    public class AnimalDto // DTO to return string types for animal related entities
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Sex { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime? DateOfDeath { get; set; }
        public required string Species { get; set; }
        //public required string Line { get; set; } //this is weird FIXME how to handle line given the messed up db that EF won't let me change LINE SHOULD BE CREATED BASED ON VARIETY FOR NOW 
        public string Dam { get; set; } = string.Empty;
        public string Sire { get; set; } = string.Empty;
        public string Variety { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public required string Breeder { get; set; }
        public string Genotype { get; set; } = string.Empty;
    }
}