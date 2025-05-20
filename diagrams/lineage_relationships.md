# Lineage and Line Ancestry Relationships

This diagram shows how Animal and Lineage work together to track ancestry relationships in the R.A.T. App.

```mermaid
classDiagram
    class Animal {
        +int Id
        +string? registrationNumber
        +int LineId
        +string Sex
        +DateTime DateOfBirth
        +DateTime? DateOfDeath
        +int Age
        +string Name
        +int StockId
        +string? imageUrl
        +string? comment
        +int? weight
        +Line? Line
        +ICollection~Litter~ Litters
        +ICollection~AnimalTrait~ Traits
        +ICollection~Genotype~ Genotypes
        +int? AgeInMonths
        +string? AgeAsString
        +string DisplayName
    }

    class Lineage {
        +int Id
        +int AnimalId
        +int AncestorId
        +int Generation
        +int Sequence
        +DateTime RecordedAt
        +string? RelationshipType
        +Animal? Animal
        +Animal? Ancestor
    }

    class Line {
        +int Id
        +string Name
        +int StockId
        +Stock Stock
        +ICollection~Animal~ Animals
    }

    class Stock {
        +int Id
        +int BreederId
        +int SpeciesId
        +Breeder Breeder
        +Species Species
        +ICollection~Line~ Lines
    }

    class Species {
        +int Id
        +string CommonName
        +string ScientificName
    }

    Animal "1" -- "*" Lineage : appears as descendant in
    Animal "1" -- "*" Lineage : appears as ancestor in
    Animal "*" -- "1" Line : belongs to
    Line "*" -- "1" Stock : belongs to
    Stock "*" -- "1" Species : belongs to
```

## Key Components

1. Animal-Lineage Relationships:
   - Each Animal can appear in multiple Lineage records as either:
     * A descendant (via AnimalId)
     * An ancestor (via AncestorId)
   - Lineage records store:
     * Generation number (1 for parents, 2 for grandparents, etc.)
     * Sequence number (1=dam side, 2=sire side)
     * RelationshipType ("Maternal" or "Paternal")
     * Timestamp of when the relationship was recorded

2. Animal-Line-Stock Hierarchy:
   - Animals belong to Lines
   - Lines belong to Stocks
   - Stocks belong to Species
   - This hierarchy helps organize breeding programs and track varieties

3. Navigation Properties:
   - Lineage.Animal: The descendant animal in the relationship
   - Lineage.Ancestor: The ancestor animal in the relationship
   - Animal.Line: The line this animal belongs to
   - Line.Stock: The stock this line belongs to
   - Stock.Species: The species this stock represents
