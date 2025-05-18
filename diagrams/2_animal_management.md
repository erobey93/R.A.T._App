classDiagram
    %% Animal Management Models
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

    %% Animal Management Forms/Panels
    class AnimalPanel {
        -DataGridView animalsGrid
        -Button addAnimalButton
        -ComboBox filterSpecies
        +InitializeComponent()
        +LoadAnimals()
        +FilterAnimals()
    }

    class HealthRecord {
        +int Id
        +int AnimalId
        +DateTime Date
        +string Type
        +string Notes
        +string Veterinarian
    }

    %% UI Components
    class FormComponentFactory {
        +CreateButton()
        +CreateTextBox()
        +CreateComboBox()
        +CreateDataGridView()
    }

    class ValidationHelper {
        +ValidateInput()
        +ShowError()
        +ClearErrors()
    }

    %% Relationships
    Animal "1" -- "1" Species
    Animal "1" -- "*" AnimalImage
    Animal "1" -- "*" AnimalRecord
    Animal "1" -- "*" HealthRecord
    AnimalPanel --> FormComponentFactory : uses
    AnimalPanel --> ValidationHelper : uses
    AnimalPanel --> LoadingSpinnerHelper : uses
