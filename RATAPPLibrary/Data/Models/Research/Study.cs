using System;
using System.Collections.Generic;
using RATAPPLibrary.Data.Models.Account_Management;

namespace RATAPPLibrary.Data.Models.Research
{
    public class Study
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EstimatedEndDate { get; set; }
        public StudyType Type { get; set; }
        public StudyStatus Status { get; set; }
        
        // Foreign keys
        public int ResearcherId { get; set; }
        
        // Navigation properties
        public virtual User Researcher { get; set; }
        public virtual ICollection<StudyGroup> StudyGroups { get; set; }
        public virtual ICollection<DataPoint> DataPoints { get; set; }
        
        public Study()
        {
            StudyGroups = new HashSet<StudyGroup>();
            DataPoints = new HashSet<DataPoint>();
        }
    }

    public enum StudyType
    {
        Genetics,
        Health,
        Behavior,
        Growth,
        Other
    }

    public enum StudyStatus
    {
        Planned,
        Active,
        Completed,
        Cancelled
    }
}
