# Genetics Models and Relationships

This diagram shows how the genetics-related models work together in the R.A.T. App.

```mermaid
classDiagram
    class Animal {
        +int Id
        +string Name
        +ICollection~AnimalTrait~ Traits
        +ICollection~Genotype~ Genotypes
    }

    class Chromosome {
        +Guid ChromosomeId
        +string Name
        +int Number
        +int SpeciesId
        +string Description
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +Species Species
        +ICollection~ChromosomePair~ MaternalPairs
        +ICollection~ChromosomePair~ PaternalPairs
    }

    class ChromosomePair {
        +Guid PairId
        +Guid MaternalChromosomeId
        +Guid PaternalChromosomeId
        +string InheritancePattern
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +Chromosome MaternalChromosome
        +Chromosome PaternalChromosome
        +ICollection~Gene~ Genes
        +ICollection~Genotype~ Genotypes
    }

    class Gene {
        +Guid GeneId
        +Guid ChromosomePairId
        +string Name
        +string CommonName
        +int Position
        +string Description
        +string ExpressionAge
        +string Penetrance
        +string Expressivity
        +bool RequiresMonitoring
        +string Category
        +string ImpactLevel
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +ChromosomePair ChromosomePair
        +ICollection~Allele~ Alleles
    }

    class Allele {
        +Guid AlleleId
        +Guid GeneId
        +string Name
        +string Symbol
        +string Phenotype
        +bool IsWildType
        +string RiskLevel
        +string ManagementNotes
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +Gene Gene
    }

    class Genotype {
        +Guid GenotypeId
        +int AnimalId
        +Guid ChromosomePairId
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +Animal Animal
        +ChromosomePair ChromosomePair
    }

    class Species {
        +int Id
        +string CommonName
        +string ScientificName
    }

    class BreedingCalculation {
        +Guid CalculationId
        +int PairingId
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +ICollection~PossibleOffspring~ PossibleOffspring
    }

    class PossibleOffspring {
        +Guid OffspringId
        +Guid CalculationId
        +float Probability
        +string Phenotype
        +string GenotypeDescription
        +string MaternalAlleles
        +string PaternalAlleles
        +BreedingCalculation BreedingCalculation
    }

    Animal "1" -- "*" Genotype
    Genotype "*" -- "1" ChromosomePair
    ChromosomePair "1" -- "1" Chromosome : maternal
    ChromosomePair "1" -- "1" Chromosome : paternal
    ChromosomePair "1" -- "*" Gene
    Gene "1" -- "*" Allele
    Chromosome "*" -- "1" Species
    BreedingCalculation "1" -- "*" PossibleOffspring

```

## Key Components

1. Core Genetic Structure:
   - Species defines the base genetic structure
   - Chromosomes belong to specific species
   - ChromosomePairs link maternal and paternal chromosomes
   - Genes are positioned on chromosome pairs
   - Alleles represent variants of genes

2. Animal Genetics:
   - Animals have Genotypes
   - Genotypes link to ChromosomePairs
   - This tracks both maternal and paternal genetic information

3. Breeding Calculations:
   - BreedingCalculation tracks potential offspring outcomes
   - PossibleOffspring stores predicted genetic combinations
   - Includes probability and phenotype predictions

4. Genetic Properties:
   - Genes track expression age, penetrance, and impact
   - Alleles define specific variants with phenotypes
   - Risk levels and management notes for genetic traits

5. Temporal Tracking:
   - CreatedAt/UpdatedAt timestamps on genetic records
   - Helps track genetic data evolution over time
