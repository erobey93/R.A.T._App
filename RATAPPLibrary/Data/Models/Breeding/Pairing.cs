namespace RATAPPLibrary.Data.Models.Breeding
{
    public class Pairing
    {
        public int? Id { get; set; } // Unique identifier for the pairing
        public string pairingId { get; set; } //this id will be used to differentiate between pairings that are in the same project and with the same animals 

        // Foreign Keys
        public int SireId { get; set; } // Foreign key for the sire (male parent)
        public int DamId { get; set; } // Foreign key for the dam (female parent)
        public int ProjectId { get; set; } // Foreign key for the associated project TODO there maybe a better way to do this i.e. someone may not always want to have a seperate "project" maybe allow this to be nullable, or have generic "breeding" project preset?
        //the way that would work is that the app comes with a default project that is always there, and then you can add your own projects. This would be a good way to allow for the granularity of projects without forcing it on users 
        public DateTime? PairingStartDate { get; set; }
        public DateTime? PairingEndDate { get; set; }
        // Timestamps for tracking creation and updates
        public DateTime CreatedOn { get; set; } // The date the pairing was created
        public DateTime LastUpdated { get; set; } // The date the pairing was last updated

        // Navigation Properties (nullable)
        public Animal? Sire { get; set; } // Navigation property for the sire
        public Animal? Dam { get; set; } // Navigation property for the dam
        public Project? Project { get; set; } // Navigation property for the project

        //Append dam + sire + pairing start date to make which pairing we're dealing with more clear to the user 
        public string DisplayText
        {
            get => $"{Dam.Name} + {Sire.Name} ({PairingStartDate:yyyy-MM-dd})";
        }
    }
}
