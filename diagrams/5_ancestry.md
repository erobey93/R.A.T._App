classDiagram
    %% Ancestry Models
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

    %% Ancestry Forms/Panels
    class AncestryPanel {
        -TreeView pedigreeTree
        -Button exportButton
        +InitializeComponent()
        +LoadPedigree()
        +ExportPedigree()
    }

    class PedigreeForm {
        -Panel pedigreeContainer
        -Button printButton
        +InitializeComponent()
        +GeneratePedigree()
        +PrintPedigree()
    }

    class IndividualAnimalAncestry {
        -TreeView ancestryTree
        -Label generationCount
        +InitializeComponent()
        +LoadAncestry()
        +CalculateCoefficient()
    }

    %% Helper Classes
    class LineageVisualizer {
        +DrawPedigree()
        +ExportToImage()
        +CalculateLayout()
    }

    %% Relationships
    Animal "1" -- "*" AncestryRecord
    AncestryRecord "1" -- "*" AncestryRecordLink
    Animal "1" -- "*" Lineage : "Parent"
    Animal "1" -- "*" Lineage : "Child"
    Line "1" -- "*" LineLineage
    Animal "1" -- "*" LineLineage
    AncestryPanel --> LineageVisualizer : uses
    PedigreeForm --> LineageVisualizer : uses
    IndividualAnimalAncestry --> LineageVisualizer : uses
    AncestryPanel --> LoadingSpinnerHelper : uses
