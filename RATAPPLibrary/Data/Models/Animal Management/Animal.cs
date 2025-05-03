using PdfSharp.Pdf.Filters;
using RATAPPLibrary.Data.Models.Ancestry;
using RATAPPLibrary.Data.Models.Breeding;
using RATAPPLibrary.Data.Models.Genetics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RATAPPLibrary.Data.Models
{
    public class Animal
    {
        public int Id { get; set; } // Unique identifier for the animal
        public string? registrationNumber { get; set; } // User defined registration number for the animal (optional) 
        public int LineId { get; set; } // Assuming LineId is an integer
        public required string Sex { get; set; } // Assuming Sex is stored as a string (e.g., "Male", "Female", "Intersex", "Unknown")
        public DateTime DateOfBirth { get; set; } // Date of birth is currently required but I'm setting it in my RATAPP to the current date what I'd like to do is to autoset it to the day that the animal is created on until the user updates it in this library
        public DateTime? DateOfDeath { get; set; } // Nullable DateTime for DateOfDeath
        public int Age { get; set; } //age should be auto calculated or set by user and then DOB calculated 
        public required string Name { get; set; } 
        public int StockId { get; set; } // FIXME this should not be here but EF if fing up need to research shadow properties more and better understand EF
        public string? imageUrl { get; set; } 
        public string?[] additionalImageUrls { get; set; }
        public string? comment { get; set; } 
        public int? weight { get; set; } //FIXME not convinced weight should be in here, but for now its fine 

        // Navigation Properties for EF
        public virtual Line? Line { get; set; } // Navigation property to Line table
        public virtual ICollection<Litter>? Litters { get; set; } // Navigation property for related litters
                                                          //may make sense to have genetics and ancestry here as well TODO - add these/think through logic 
        public virtual ICollection<AnimalTrait>? Traits { get; set; } // Navigation property for all traits for a specific animal
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
        public string regNum { get; set; }
        public required string name { get; set; }
        public required string sex { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime? DateOfDeath { get; set; }
        public required string species { get; set; }
        public string? imageUrl { get; set; } //FIXME this should probably be using AnimalImage
        public string?[] additionalImageUrls { get; set; } //FIXME this should probably be using AnimalImage
        public required string Line { get; set; } //this is weird FIXME how to handle line given the messed up db that EF won't let me change LINE SHOULD BE CREATED BASED ON VARIETY FOR NOW 
        public string? earType { get; set; }
        public string? markings { get; set; }
        public string? variety { get; set; } = string.Empty;
        public string? color { get; set; }
        public int? damId { get; set; }
        public int? sireId { get; set; }
        public required string breeder { get; set; }
        public string? genotype { get; set; } = string.Empty;
        public string? comment { get; set; } = string.Empty;
    }
}