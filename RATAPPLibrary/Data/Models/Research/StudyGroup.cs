using System.Collections.Generic;

namespace RATAPPLibrary.Data.Models.Research
{
    public class StudyGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public GroupType Type { get; set; }
        
        // Foreign keys
        public int StudyId { get; set; }
        
        // Navigation properties
        public virtual Study Study { get; set; }
        public virtual ICollection<StudyAnimal> StudyAnimals { get; set; }
        
        public StudyGroup()
        {
            StudyAnimals = new HashSet<StudyAnimal>();
        }
    }

    public enum GroupType
    {
        Control,
        Experimental
    }
}
