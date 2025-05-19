classDiagram
    %% Breeding Models
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

    %% Breeding Forms/Panels
    class PairingsandLittersPanel {
        -DataGridView pairingsGrid
        -DataGridView littersGrid
        -Button createPairingButton
        +InitializeComponent()
        +LoadPairings()
        +LoadLitters()
    }

    class AddLitterForm {
        -ComboBox sireComboBox
        -ComboBox damComboBox
        -DateTimePicker birthDate
        -NumericUpDown litterSize
        +InitializeComponent()
        +SaveLitter()
    }

    class PairingDetailsForm {
        -DataGridView pairingsGrid
        -Button addPairingButton
        -Button viewLittersButton
        +InitializeComponent()
        +LoadPairings()
        +AddNewPairing()
    }

    %% Relationships
    Breeder "1" -- "*" BreederClub
    Club "1" -- "*" BreederClub
    Pairing "1" -- "*" Litter
    RATAppBaseForm <|-- AddLitterForm
    RATAppBaseForm <|-- PairingDetailsForm
    PairingsandLittersPanel --> LoadingSpinnerHelper : uses
