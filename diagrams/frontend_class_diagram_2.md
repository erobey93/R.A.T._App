classDiagram
    %% Base Forms
    class RATAppBaseForm {
        +InitializeComponent()
        #SetupForm()
        #ConfigureLayout()
        #HandleEvents()
    }

    %% Main Forms
    class LoginForm {
        -TextBox usernameTextBox
        -TextBox passwordTextBox
        -Button loginButton
        +InitializeComponent()
        +ValidateCredentials()
    }

    class CreateAccountForm {
        -TextBox usernameTextBox
        -TextBox emailTextBox
        -Button createButton
        +InitializeComponent()
        +CreateNewAccount()
    }

    class UpdateCredentialsForm {
        -TextBox currentPasswordBox
        -TextBox newPasswordBox
        -Button updateButton
        +InitializeComponent()
        +UpdateUserCredentials()
    }

    %% Animal Management Forms
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

    class LitterDetailsForm {
        -DataGridView litterGrid
        -Button addOffspringButton
        -Label litterStats
        +InitializeComponent()
        +LoadLitterDetails()
    }

    %% Panels
    class HomePanel {
        -DataGridView recentActivityGrid
        -Chart statisticsChart
        +InitializeComponent()
        +LoadDashboard()
        +UpdateStats()
    }

    class AnimalPanel {
        -DataGridView animalsGrid
        -Button addAnimalButton
        -ComboBox filterSpecies
        +InitializeComponent()
        +LoadAnimals()
        +FilterAnimals()
    }

    class PairingsandLittersPanel {
        -DataGridView pairingsGrid
        -DataGridView littersGrid
        -Button createPairingButton
        +InitializeComponent()
        +LoadPairings()
        +LoadLitters()
    }

    class GeneticsPanel {
        -ComboBox traitSelector
        -DataGridView genotypeGrid
        -Button calculateButton
        +InitializeComponent()
        +LoadGenetics()
        +CalculateBreeding()
    }

    class AncestryPanel {
        -TreeView pedigreeTree
        -Button exportButton
        +InitializeComponent()
        +LoadPedigree()
        +ExportPedigree()
    }

    class AdopterManagementPanel {
        -DataGridView adoptersGrid
        -Button addAdopterButton
        +InitializeComponent()
        +LoadAdopters()
    }

    %% Helpers
    class FormComponentFactory {
        +CreateButton()
        +CreateTextBox()
        +CreateComboBox()
        +CreateDataGridView()
    }

    class FormStyleHelper {
        +ApplyTheme()
        +SetFonts()
        +ConfigureColors()
    }

    class ValidationHelper {
        +ValidateInput()
        +ShowError()
        +ClearErrors()
    }

    class LineageVisualizer {
        +DrawPedigree()
        +ExportToImage()
        +CalculateLayout()
    }

    class LoadingSpinnerHelper {
        +ShowSpinner()
        +HideSpinner()
        +UpdateStatus()
    }

    class FormDataManager {
        +LoadFormData()
        +SaveFormData()
        +ValidateData()
    }

    class FormEventHandler {
        +AttachEvents()
        +HandleClick()
        +HandleChange()
    }

    %% Relationships
    RATAppBaseForm <|-- LoginForm
    RATAppBaseForm <|-- CreateAccountForm
    RATAppBaseForm <|-- UpdateCredentialsForm
    RATAppBaseForm <|-- AddLitterForm
    RATAppBaseForm <|-- PairingDetailsForm
    RATAppBaseForm <|-- LitterDetailsForm

    RATAppBaseForm --> FormComponentFactory : uses
    RATAppBaseForm --> FormStyleHelper : uses
    RATAppBaseForm --> ValidationHelper : uses
    RATAppBaseForm --> FormDataManager : uses
    RATAppBaseForm --> FormEventHandler : uses

    HomePanel --> LoadingSpinnerHelper : uses
    AnimalPanel --> LoadingSpinnerHelper : uses
    PairingsandLittersPanel --> LoadingSpinnerHelper : uses
    GeneticsPanel --> LoadingSpinnerHelper : uses
    AncestryPanel --> LoadingSpinnerHelper : uses
    AdopterManagementPanel --> LoadingSpinnerHelper : uses

    AncestryPanel --> LineageVisualizer : uses
