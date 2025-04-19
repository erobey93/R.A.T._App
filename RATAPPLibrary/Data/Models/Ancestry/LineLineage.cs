using RATAPPLibrary.Data.Models.Breeding;

namespace RATAPPLibrary.Data.Models.Ancestry
{
    public class LineLineage
    {
        public int LineageId { get; set; } // Primary Key
        public required int NewLineId { get; set; } // Foreign Key to Line
        public required int ParentLine1Id { get; set; } // Foreign Key to Line
        public int? ParentLine2Id { get; set; } // Foreign Key to Line, nullable
        public DateTime CrossDate { get; set; }
        public string? Notes { get; set; }

        // Navigation Properties (Assuming you have a 'Line' class)
        public Line? NewLine { get; set; }
        public Line? ParentLine1 { get; set; }
        public Line? ParentLine2 { get; set; }
    }
}
