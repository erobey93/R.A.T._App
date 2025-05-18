classDiagram
    %% Account Management Domain
    class User {
        +int Id
        +string Username
        +Individual Individual
        +AccountType AccountType
        +Credentials Credentials
    }

    class Individual {
        +int Id
        +string FirstName
        +string LastName
        +string Email
        +string Phone
    }

    class Credentials {
        +int Id
        +string PasswordHash
        +string Salt
        +DateTime LastLogin
    }

    class AccountType {
        +int Id
        +string Type
        +string Description
    }

    %% Animal Management Domain
    class Animal {
        +int Id
        +string Name
        +DateTime DateOfBirth
        +string Gender
        +bool IsAlive
        +decimal Price
        +string Status
        +string Comments
        +Species Species
        +List~AnimalImage~ Images
        +List~AnimalRecord~ Records
    }

    class Species {
        +int Id
        +string Name
        +string Description
        +bool IsActive
    }

    class AnimalImage {
        +int Id
        +int AnimalId
        +string ImagePath
        +DateTime DateAdded
    }

    class AnimalRecord {
        +int Id
        +int AnimalId
        +string RecordType
        +string Description
        +DateTime DateCreated
    }

    %% Ancestry Domain
    class AncestryRecord {
        +int Id
        +int AnimalId
        +DateTime DateCreated
        +List~AncestryRecordLink~ Links
    }

    class AncestryRecordLink {
        +int Id
        +int AncestryRecordId
        +int ParentId
        +int GenerationLevel
    }

    class Lineage {
        +int Id
        +int ParentId
        +int ChildId
        +string RelationType
    }

    class LineLineage {
        +int Id
        +int LineId
        +int AnimalId
        +DateTime DateAdded
    }

    %% Breeding Domain
    class Breeder {
        +int Id
        +string Name
        +string Location
        +List~BreederClub~ Clubs
    }

    class Club {
        +int Id
        +string Name
        +string Description
        +List~BreederClub~ Members
    }

    class BreederClub {
        +int Id
        +int BreederId
        +int ClubId
        +DateTime JoinDate
    }

    class Line {
        +int Id
        +string Name
        +string Description
        +DateTime CreatedDate
        +bool IsActive
    }

    class Pairing {
        +int Id
        +int SireId
        +int DamId
        +DateTime DatePaired
        +string Status
        +string Notes
        +bool IsActive
        +List~Litter~ Litters
    }

    class Litter {
        +int Id
        +int PairingId
        +DateTime DateBorn
        +int Size
        +string Notes
        +string ImagePath
    }

    class Project {
        +int Id
        +string Name
        +string Description
        +DateTime StartDate
        +DateTime? EndDate
        +string Status
    }

    class Stock {
        +int Id
        +string Name
        +string Description
        +int Quantity
        +decimal Price
    }

    %% Genetics Domain
    class Gene {
        +int Id
        +string Name
        +string Description
        +string InheritancePattern
        +int SpeciesId
        +List~Allele~ Alleles
    }

    class Allele {
        +int Id
        +int GeneId
        +string Name
        +string Symbol
        +bool IsDominant
    }

    class Trait {
        +int Id
        +string Name
        +string Description
        +TraitType Type
        +bool IsGenetic
        +List~AnimalTrait~ Animals
    }

    class AnimalTrait {
        +int Id
        +int AnimalId
        +int TraitId
        +DateTime DateAssigned
    }

    class TraitType {
        +int Id
        +string Name
        +string Description
    }

    class Chromosome {
        +int Id
        +string Name
        +List~Gene~ Genes
    }

    class ChromosomePair {
        +int Id
        +int AnimalId
        +int ChromosomeId
        +string MaternalAllele
        +string PaternalAllele
    }

    class Genotype {
        +int Id
        +int AnimalId
        +List~ChromosomePair~ ChromosomePairs
    }

    class BreedingCalculation {
        +int Id
        +int SireId
        +int DamId
        +DateTime CalculationDate
        +List~PossibleOffspring~ PossibleOffspring
    }

    class PossibleOffspring {
        +int Id
        +int BreedingCalculationId
        +string Genotype
        +decimal Probability
    }

    %% Relationships - Account Management
    User "1" -- "1" Individual
    User "1" -- "1" Credentials
    User "1" -- "1" AccountType

    %% Relationships - Animal Management
    Animal "1" -- "1" Species
    Animal "1" -- "*" AnimalImage
    Animal "1" -- "*" AnimalRecord

    %% Relationships - Ancestry
    Animal "1" -- "*" AncestryRecord
    AncestryRecord "1" -- "*" AncestryRecordLink
    Animal "1" -- "*" Lineage : "Parent"
    Animal "1" -- "*" Lineage : "Child"
    Line "1" -- "*" LineLineage
    Animal "1" -- "*" LineLineage

    %% Relationships - Breeding
    Breeder "1" -- "*" BreederClub
    Club "1" -- "*" BreederClub
    Animal "1" -- "*" Pairing : "Sire"
    Animal "1" -- "*" Pairing : "Dam"
    Pairing "1" -- "*" Litter

    %% Relationships - Genetics
    Species "1" -- "*" Gene
    Gene "1" -- "*" Allele
    Animal "1" -- "*" AnimalTrait
    Trait "1" -- "*" AnimalTrait
    Trait "1" -- "1" TraitType
    Chromosome "1" -- "*" Gene
    Animal "1" -- "1" Genotype
    Genotype "1" -- "*" ChromosomePair
    BreedingCalculation "1" -- "*" PossibleOffspring
