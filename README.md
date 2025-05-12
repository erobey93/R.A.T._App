# R.A.T. App (Rodent Animal Tracking Application)

> **⚠️ PROOF OF CONCEPT STATUS**: This application is currently a Proof of Concept (POC) with all areas under active development. Features, APIs, and data structures may change significantly as development progresses.

## Project Overview

R.A.T. App is a comprehensive animal management system designed specifically for tracking and managing rodent populations in research and breeding environments. This application provides tools for detailed animal tracking, breeding management, lineage visualization, and research data organization.

### Development Status

- 🚧 **Current Phase**: Proof of Concept
- 📅 **Last Updated**: May 2025
- 🎯 **Focus Areas**: Core functionality and data model validation

## Features

### Animal Management [In Development]
- Individual animal tracking with unique identifiers
- Trait and characteristic recording
- Image attachment support
- Health record maintenance
- **Known Limitations**: Some trait management features are in beta

### Breeding Management [In Development]
- Pairing tracking and management
- Litter documentation
- Breeding calculator
- **Known Limitations**: Advanced breeding predictions pending implementation

### Lineage & Genetics [Beta]
- Family tree visualization
- Genetic trait tracking
- Ancestry mapping
- **Known Limitations**: Complex genetic calculations in development

### Research & Reports [Planned]
- Data export capabilities
- Custom report generation
- Research project organization
- **Known Limitations**: Basic reporting only in current version

### User Management [Beta]
- Secure authentication
- Role-based access control
- Account management
- **Known Limitations**: Advanced permissions system planned

## Technology Stack

- **Desktop Application**: .NET/C# Windows Forms
- **Web Component**: React/TypeScript
- **Database**: SQL Server with Entity Framework Core
- **Testing**: MSTest framework

## Installation & Setup

### Prerequisites
- .NET 6.0 or later
- SQL Server 2019 or later
- Node.js 16+ (for web component)

### Database Setup
1. Ensure SQL Server is installed and running
2. Update connection string in `appsettings.json`
3. Run Entity Framework migrations:
   ```bash
   dotnet ef database update
   ```

### Application Setup
1. Clone the repository
2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
3. Build the solution:
   ```bash
   dotnet build
   ```

### First Run
1. Start the application
2. Create an initial admin account
3. Configure basic settings

## Usage Guide

### Getting Started
1. Launch the application
2. Log in or create a new account
3. Navigate through the main menu panels

### Core Workflows

#### Animal Management
- Add new animals with detailed information
- Track health records
- Manage traits and characteristics

#### Breeding Management
- Create and monitor pairings
- Document litters
- Track breeding outcomes

#### Research Tools
- Organize research projects
- Generate reports
- Export data

## Project Structure

### Core Components
- **RATAPP/**: Desktop application
- **RATAPPLibrary/**: Core business logic and data access
- **RATAPP_WEB/**: Web interface components
- **RATAPPLibraryUT/**: Unit tests

### Key Services
- AnimalService: Core animal management
- BreedingService: Pairing and litter management
- LineageService: Ancestry tracking
- ProjectService: Research project organization

## Development

### Setting Up Development Environment
1. Install required SDKs and tools
2. Clone repository
3. Set up local database
4. Configure development settings

### Building and Running
```bash
# Build solution
dotnet build

# Run desktop app
dotnet run --project RATAPP

# Run web component
cd RATAPP_WEB/ratapp_web.client
npm install
npm run dev
```

### Code Organization
- Services follow repository pattern
- UI components use MVVM pattern
- Database access via Entity Framework Core

## Testing

### Running Tests
```bash
dotnet test RATAPPLibraryUT
```

### Test Coverage
- Unit tests for core services
- Integration tests for database operations
- UI component testing

## Development Roadmap

### Short Term
- Complete core animal management features
- Enhance breeding management system
- Improve lineage visualization

### Medium Term
- Implement advanced genetic calculations
- Expand reporting capabilities
- Add data analysis tools

### Long Term
- Mobile application development
- Cloud synchronization
- Advanced research tools

## Contributing

### Getting Started
1. Fork the repository
2. Create a feature branch
3. Submit pull request

### Coding Standards
- Follow C# coding conventions
- Include unit tests
- Update documentation

### Pull Request Process
1. Ensure all tests pass
2. Update relevant documentation
3. Request code review

## License

[License information pending]

---

> 🚧 **Development Notice**: This application is under active development. Features and implementations are subject to change. Please report any issues or suggestions through the appropriate channels.
