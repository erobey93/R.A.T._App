classDiagram
    %% Account Management Domain
    class User {
        +int Id
        +string Username
        +Individual Individual
        +AccountType AccountType
        +Credentials Credentials
    }

    class Individual {
        +int Id
        +string FirstName
        +string LastName
        +string Email
        +string Phone
    }

    class Credentials {
        +int Id
        +string PasswordHash
        +string Salt
        +DateTime LastLogin
    }

    class AccountType {
        +int Id
        +string Type
        +string Description
    }

    %% Account Management Forms
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

    %% Relationships
    User "1" -- "1" Individual
    User "1" -- "1" Credentials
    User "1" -- "1" AccountType
    RATAppBaseForm <|-- LoginForm
    RATAppBaseForm <|-- CreateAccountForm
    RATAppBaseForm <|-- UpdateCredentialsForm
