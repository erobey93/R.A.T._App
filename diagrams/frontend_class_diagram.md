# Frontend Class Diagram for Genetics and Ancestry

```mermaid
classDiagram
    class GeneticsPanel {
        +LoadTraits()
        +AssignTraitToAnimal()
        +ViewAnimalGenetics()
    }

    class AncestryPanel {
        +ViewPedigree()
        +TrackLineage()
        +DisplayFamilyTree()
    }

    class IndividualAnimalAncestry {
        +LoadAnimalDetails()
        +DisplayAncestors()
        +ShowGenerations()
    }

    class PedigreeForm {
        +GeneratePedigree()
        +DisplayAncestryTree()
        +ExportPedigree()
    }

    class LineageVisualizer {
        +RenderFamilyTree()
        +DisplayConnections()
        +UpdateVisualization()
    }

    class AddTraitForm {
        +CreateNewTrait()
        +SelectTraitType()
        +AssignToSpecies()
    }

    class AssignTraitForm {
        +SelectAnimal()
        +ChooseTrait()
        +SaveTraitAssignment()
    }

    GeneticsPanel -- AddTraitForm
    GeneticsPanel -- AssignTraitForm
    AncestryPanel -- IndividualAnimalAncestry
    AncestryPanel -- PedigreeForm
    AncestryPanel -- LineageVisualizer
