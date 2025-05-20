# Lineage and Line Ancestry Relationships

This diagram shows how Animal, Lineage, and LineLineage work together to track both individual animal ancestry and line ancestry in the R.A.T. App.

```mermaid
classDiagram
    class Animal {
        +int Id
        +string Name
        +int LineId
        +Line Line
        +ICollection~Lineage~ Ancestors
        +ICollection~Lineage~ Descendants
    }

    class Lineage {
        +int Id
        +int AnimalId
        +int AncestorId
        +int Generation
        +int Sequence
        +string? RelationshipType
        +DateTime RecordedAt
        +Animal Animal
        +Animal Ancestor
    }

    class Line {
        +int Id
        +string Name
        +int StockId
        +Stock Stock
        +ICollection~Animal~ Animals
        +ICollection~LineLineage~ NewLineages
        +ICollection~LineLineage~ ParentLineages1
        +ICollection~LineLineage~ ParentLineages2
    }

    class LineLineage {
        +int LineageId
        +int NewLineId
        +int ParentLine1Id
        +int? ParentLine2Id
        +DateTime CrossDate
        +string? Notes
        +Line NewLine
        +Line ParentLine1
        +Line ParentLine2
    }

    Animal "1" -- "*" Lineage : has ancestors
    Animal "1" -- "*" Lineage : is ancestor of
    Animal "*" -- "1" Line : belongs to
    Line "1" -- "*" LineLineage : new line
    Line "1" -- "*" LineLineage : parent line 1
    Line "1" -- "*" LineLineage : parent line 2
```

## Key Components

1. Individual Animal Ancestry (Animal-Lineage):
- Each Animal can have multiple Lineage records as both a descendant and ancestor
- Lineage tracks generation and sequence numbers for precise relationship mapping
- RelationshipType distinguishes between maternal and paternal lines

2. Line Ancestry (Line-LineLineage):
- Lines can be created from crossing other lines
- LineLineage tracks the creation of new lines from parent lines
- Supports both single-parent and two-parent line creation
- Records the date of line creation and optional notes

The connection between these systems:
- Animals belong to Lines
- When new lines are created through breeding, the LineLineage system tracks the line relationships
- Individual animal ancestry is tracked separately through the Lineage system
- This allows tracking both individual animal pedigrees and the broader line development history
