classDiagram
    %% Base Classes and Interfaces
    class BaseService {
        #_dbContext: RatAppDbContext
        #_cache: CacheService
        +BaseService(dbContext, cache)
    }

    %% Core Services
    class AnimalService {
        +GetAllAnimals()
        +GetAnimalById(id)
        +CreateAnimal(animal)
        +UpdateAnimal(animal)
        +DeleteAnimal(id)
    }

    class BreedingService {
        +GetAllPairings()
        +CreatePairing(pairing)
        +GetPairingById(id)
        +UpdatePairing(pairing)
    }

    class LineageService {
        +GetAncestors(animalId)
        +GetDescendants(animalId)
        +AddLineageRelation(parentId, childId)
    }

    %% Genetics Related Services
    class BreedingCalculationService {
        +CalculateOffspringProbabilities()
        +PredictTraits()
    }

    class GeneService {
        +GetGenesBySpecies()
        +AddGene()
        +UpdateGene()
    }

    %% Authentication & User Management
    class AccountService {
        +CreateAccount()
        +UpdateAccount()
        +DeleteAccount()
    }

    class LoginService {
        +ValidateCredentials()
        +GenerateToken()
        +RefreshToken()
    }

    %% Core Data Models
    class Animal {
        +Id: int
        +Name: string
        +DateOfBirth: DateTime
        +Species: string
        +Gender: string
        +ImagePath: string
        +Comments: string
    }

    class Pairing {
        +Id: int
        +SireId: int
        +DamId: int
        +DatePaired: DateTime
        +Status: string
    }

    class Litter {
        +Id: int
        +PairingId: int
        +DateBorn: DateTime
        +Size: int
        +Notes: string
    }

    %% Relationships
    BaseService <|-- AnimalService
    BaseService <|-- BreedingService
    BaseService <|-- LineageService
    BaseService <|-- AccountService
    BaseService <|-- LoginService

    AnimalService --> Animal
    BreedingService --> Pairing
    BreedingService --> Litter
    LineageService --> Animal

    BreedingCalculationService --> GeneService
    BreedingService --> BreedingCalculationService
