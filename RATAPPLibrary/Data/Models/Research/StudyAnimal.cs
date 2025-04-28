using System.Collections.Generic;

namespace RATAPPLibrary.Data.Models.Research
{
    public class StudyAnimal
    {
        public int Id { get; set; }
        
        // Foreign keys
        public int StudyGroupId { get; set; }
        public int AnimalId { get; set; }
        
        // Navigation properties
        public virtual StudyGroup StudyGroup { get; set; }
        public virtual Animal Animal { get; set; }
        public virtual ICollection<ObservationData> Observations { get; set; }
        
        public StudyAnimal()
        {
            Observations = new HashSet<ObservationData>();
        }
    }
}
