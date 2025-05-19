classDiagram
    %% Base Form
    class RATAppBaseForm {
        +InitializeComponent()
        #SetupForm()
        #ConfigureLayout()
        #HandleEvents()
    }

    %% UI Helper Classes
    class FormComponentFactory {
        +CreateButton()
        +CreateTextBox()
        +CreateComboBox()
        +CreateDataGridView()
        +CreateLabel()
        +CreatePanel()
    }

    class FormStyleHelper {
        +ApplyTheme()
        +SetFonts()
        +ConfigureColors()
        +SetupLayout()
        +ConfigureResponsiveness()
    }

    class ValidationHelper {
        +ValidateInput()
        +ShowError()
        +ClearErrors()
        +ValidateRequired()
        +ValidateNumeric()
    }

    class LoadingSpinnerHelper {
        +ShowSpinner()
        +HideSpinner()
        +UpdateStatus()
        +SetProgress()
    }

    class FormDataManager {
        +LoadFormData()
        +SaveFormData()
        +ValidateData()
        +HandleErrors()
        +ResetForm()
    }

    class FormEventHandler {
        +AttachEvents()
        +HandleClick()
        +HandleChange()
        +HandleSubmit()
        +HandleValidation()
    }

    class ResponsiveLayoutHelper {
        +CalculateLayout()
        +ResizeComponents()
        +AdjustForScreenSize()
        +HandleWindowResize()
    }

    class DropdownHelper {
        +PopulateComboBox()
        +SetDefaultSelection()
        +UpdateOptions()
        +ClearSelection()
    }

    %% Relationships
    RATAppBaseForm --> FormComponentFactory : uses
    RATAppBaseForm --> FormStyleHelper : uses
    RATAppBaseForm --> ValidationHelper : uses
    RATAppBaseForm --> FormDataManager : uses
    RATAppBaseForm --> FormEventHandler : uses
    RATAppBaseForm --> ResponsiveLayoutHelper : uses
    RATAppBaseForm --> DropdownHelper : uses
    FormComponentFactory --> FormStyleHelper : uses
    FormDataManager --> ValidationHelper : uses
    FormEventHandler --> ValidationHelper : uses
