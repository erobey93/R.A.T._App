using System;

namespace RATAPPLibrary.Data.Models.Research
{
    public class ObservationData
    {
        public int Id { get; set; }
        public string Value { get; set; } // Stored as string, converted based on DataPoint.Type
        public DateTime Timestamp { get; set; }
        public string? Notes { get; set; }
        
        // Foreign keys
        public int DataPointId { get; set; }
        public int StudyAnimalId { get; set; }
        public int ObserverId { get; set; }
        
        // Navigation properties
        public virtual DataPoint DataPoint { get; set; }
        public virtual StudyAnimal StudyAnimal { get; set; }
        public virtual User Observer { get; set; }
    }
}
