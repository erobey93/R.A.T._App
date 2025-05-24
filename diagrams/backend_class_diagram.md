# Backend Class Diagram for Genetics and Ancestry

```mermaid
classDiagram
    class Animal {
        +int Id
        +string registrationNumber
        +int LineId
        +string Sex
        +DateTime DateOfBirth
        +DateTime? DateOfDeath
        +int Age
        +string Name
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

    class Trait {
        +int Id
        +string CommonName
        +int TraitTypeId
        +string Genotype
        +int SpeciesID
    }

    class TraitType {
        +int Id
        +string Type
        +string Name
        +string Description
    }

    class AnimalTrait {
        +int AnimalId
        +int TraitId
        +Animal Animal
        +Trait Trait
    }

    class Lineage {
        +int AnimalId
        +int AncestorId
        +int Generation
        +int Sequence
        +string RelationshipType
        +DateTime RecordedAt
    }

    class TraitService {
        +Task~Trait~ GetTraitByNameAsync(string name)
        +Task~List~Trait~~ GetTraitsByTypeAndSpeciesAsync(string type, string species)
        +Task~Dictionary~string,List~string~~~ GetTraitMapForSingleAnimal(int animalId)
        +Task~AnimalTrait~ CreateAnimalTraitAsync(int traitId, int animalId)
    }

    class LineageService {
        +Task~Animal~ GetDamByAnimalId(int animalId)
        +Task~Animal~ GetSireByAnimalId(int animalId)
        +Task~bool~ AddLineageConnection(int animalId, int ancestorId, int generation, int sequence, string relationshipType)
        +Task~bool~ DoesAncestryConnectionExist(int animalId, int ancestorId)
    }

    Animal "1" -- "*" AnimalTrait
    AnimalTrait "*" -- "1" Trait
    Trait "*" -- "1" TraitType
    Animal "1" -- "*" Lineage
    TraitService -- Trait
    LineageService -- Lineage
