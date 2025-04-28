using System.Collections.Generic;

namespace RATAPPLibrary.Data.Models.Research
{
    public class DataPoint
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DataType Type { get; set; }
        public bool Required { get; set; }
        public string? Options { get; set; } // JSON array for selection type options
        
        // Foreign keys
        public int StudyId { get; set; }
        
        // Navigation properties
        public virtual Study Study { get; set; }
        public virtual ICollection<ObservationData> Observations { get; set; }
        
        public DataPoint()
        {
            Observations = new HashSet<ObservationData>();
        }
    }

    public enum DataType
    {
        Text,
        Number,
        Boolean,
        Date,
        Selection
    }
}
