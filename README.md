# R.A.T. App (Rodent Analytics & Tracking)

> **⚠️ PROOF OF CONCEPT STATUS**: This application is currently a Proof of Concept (POC) with all areas under active development. Features, APIs, Tests and data structures may change significantly as development progresses.

## Project Overview

R.A.T. App is a comprehensive animal management system designed specifically for tracking and managing rodent populations in hobby breeding environments. This application provides tools for detailed animal tracking, breeding management, lineage visualization, genetics tracking and calculations, adopter management and personal research data organization.

### Development Status

- 🚧 **Current Phase**: Proof of Concept
- 📅 **Last Updated**: May 2025
- 🎯 **Focus Areas**: Core Desktop functionality, data model validation and endpoint development

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

### Lineage & Genetics [In Development]
- Family tree visualization
- Genetic trait tracking
- Ancestry mapping
- **Known Limitations**: Complex genetic calculations in development

### Research & Reports [Planned]
- Data export capabilities
- Custom report generation
- Research project organization
- **Known Limitations**: Basic reporting only in current version

### User Management [In Development]
- Secure authentication
- Role-based access control
- Account management
- **Known Limitations**: Advanced permissions system planned

## Technology Stack

- **Desktop Application**: .NET/C# Windows Forms
- **Web Component**: React/TypeScript
- **Mobile Component**: TBD
- **Database**: SQL Server with Entity Framework Core
- **Testing**: MSTest framework, XUnit 

## Installation & Setup

### Prerequisites
- .NET 6.0 or later
- SQL Server 2019 or later
- Node.js 16+ (for web component) - Web componenet within this project will be deprecated, new web project is in React.ts 

### Database Setup
1. Ensure SQL Server is installed and running
2. Update connection string in `appsettings.json`
3. Run Entity Framework migrations:
   ```bash
   dotnet ef database update
   ```

### API Setup
1. Configure the database connection string in `RATAPP.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=your_server;Database=your_db;..."
     }
   }
   ```

2. Run the API:
   ```bash
   dotnet run --project RATAPP.API
   ```

3. Access the API:
   - HTTP endpoint: http://localhost:5053
   - HTTPS endpoint: https://localhost:7155
   - Swagger UI (in development): https://localhost:7155/swagger

4. Available Endpoints:
   - GET /api/animal - List all animals
   - GET /api/animal/{id} - Get specific animal
   - POST /api/animal - Create new animal
   - PUT /api/animal/{id} - Update animal
   - DELETE /api/animal/{id} - Delete animal

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
2. Create an initial admin account - [In Development]
3. Configure basic settings - [In Development]

## Usage Guide

### Getting Started
1. Launch the application
2. Log in or create a new account
3. Navigate through the main menu panels

### Core Workflows

#### Animal Management
- Add new animals with detailed information
- Track health records [In Development]
- Manage traits and characteristics

#### Breeding Management
- Create and monitor pairings
- Document litters
- Track breeding outcomes

#### Research Tools [In Development]
- Organize research projects
- Generate reports on animal collections
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
- TODO need to update service list 

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
- Complete core breeding management system
- Complete core lineage/ancestry system
- Complete basic genetics calculations, rules and overall system
- Get user feedback on core desktop functionality

### Medium Term
- Complete REST API project (currently only animal management endpoints developed)
- Implement Web App (identical to desktop but with subscriber tiers and enhanced security)
- Implement advanced genetics calculations
- Expand reporting capabilities
- Add research page with data analysis tools
- Add financial management page
- Expand data import/export capabilities (bulk import, bulk export, connect to web app)

### Long Term
- Mobile application development
- Cloud synchronization
- Advanced research tools
- Integration with email, AI and other business related APIs to improved adopter management system
- Auto-create user websites from data

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
