using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RATAPPLibrary.Data.Models.Ancestry
{
    //TODO I am still figuring out what makes the most sense i.e. using the AnimalRecord vs not so this is a simpler implementation for the time being to get the project moving
    //this is also using a different way of setting up database schema i.e. not in the context but by using headers in the model
    public class Lineage
    {
        public int Id { get; set; }
        public required int AnimalId { get; set; }
        public required int AncestorId { get; set; }
        public required int Generation { get; set; }
        public required int Sequence { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        public string? RelationshipType { get; set; }

        // Navigation Properties
        public Animal? Animal { get; set; }
        public Animal? Ancestor { get; set; }
    }
}
