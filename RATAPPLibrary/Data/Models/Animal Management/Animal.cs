﻿using PdfSharp.Pdf.Filters;
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
        public int LineId { get; set; } // Assuming LineId is an integer
        public required string Sex { get; set; } // Assuming Sex is stored as a string (e.g., "Male", "Female", "Intersex", "Unknown")
        public DateTime DateOfBirth { get; set; } // Assuming DateOfBirth is stored as a DateTime
        public DateTime? DateOfDeath { get; set; } // Nullable DateTime for DateOfDeath
        public int Age { get; set; } // Nullable integer for Age
        public required string Name { get; set; } // Assuming Name is a string

        // Navigation Properties for EF
        public Line? Line { get; set; } // Navigation property to Line table
        public ICollection<Litter>? Litters { get; set; } // Navigation property for related litters
                                                          //may make sense to have genetics and ancestry here as well TODO - add these/think through logic 

        //data rules for animal

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
}