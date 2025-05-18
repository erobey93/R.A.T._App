```mermaid
erDiagram
    Animal {
        int Id PK
        string Name
        datetime DateOfBirth
        string Species
        string Gender
        string ImagePath
        string Comments
        int LineId FK
        bool IsAlive
        decimal Price
        string Status
    }

    Line {
        int Id PK
        string Name
        string Description
        datetime CreatedDate
        bool IsActive
    }

    Trait {
        int Id PK
        string Name
        string Description
        string Category
        bool IsGenetic
    }

    AnimalTrait {
        int Id PK
        int AnimalId FK
        int TraitId FK
        datetime DateAssigned
    }

    Pairing {
        int Id PK
        int SireId FK
        int DamId FK
        datetime DatePaired
        string Status
        string Notes
        bool IsActive
    }

    Litter {
        int Id PK
        int PairingId FK
        datetime DateBorn
        int Size
        string Notes
        string ImagePath
    }

    Lineage {
        int Id PK
        int ParentId FK
        int ChildId FK
        string RelationType
    }

    Gene {
        int Id PK
        string Name
        string Description
        string InheritancePattern
        int SpeciesId FK
    }

    Species {
        int Id PK
        string Name
        string Description
        bool IsActive
    }

    Account {
        int Id PK
        string Username
        string PasswordHash
        string Email
        datetime CreatedDate
        bool IsActive
    }

    Animal ||--o{ AnimalTrait : "has"
    Trait ||--o{ AnimalTrait : "assigned to"
    Line ||--o{ Animal : "belongs to"
    Animal ||--o{ Lineage : "is parent in"
    Animal ||--o{ Lineage : "is child in"
    Animal ||--o{ Pairing : "is sire in"
    Animal ||--o{ Pairing : "is dam in"
    Pairing ||--o{ Litter : "produces"
    Species ||--o{ Animal : "categorizes"
    Species ||--o{ Gene : "has"