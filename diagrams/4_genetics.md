classDiagram
    %% Genetics Models
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

    %% Genetics Forms/Panels
    class GeneticsPanel {
        -ComboBox traitSelector
        -DataGridView genotypeGrid
        -Button calculateButton
        +InitializeComponent()
        +LoadGenetics()
        +CalculateBreeding()
    }

    %% Relationships
    Species "1" -- "*" Gene
    Gene "1" -- "*" Allele
    Animal "1" -- "*" AnimalTrait
    Trait "1" -- "*" AnimalTrait
    Trait "1" -- "1" TraitType
    Chromosome "1" -- "*" Gene
    Animal "1" -- "1" Genotype
    Genotype "1" -- "*" ChromosomePair
    BreedingCalculation "1" -- "*" PossibleOffspring
    GeneticsPanel --> LoadingSpinnerHelper : uses
