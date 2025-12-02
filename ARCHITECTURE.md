# Kwikbooks Architecture

## Project Structure

```
Kwikbooks/
├── Components/              # Blazor UI components
│   ├── Layout/             # App layout components (MainLayout, NavMenu)
│   ├── Pages/              # Routable page components
│   ├── Shared/             # Reusable UI components (Card, PageHeader, etc.)
│   ├── Routes.razor        # Router configuration
│   └── _Imports.razor      # Global using statements for Razor components
│
├── Data/                   # Data layer
│   ├── Models/             # Entity models (BaseEntity, domain models)
│   └── KwikbooksDbContext.cs  # EF Core database context
│
├── Services/               # Business logic layer
│   ├── IDataService.cs     # Generic data service interface
│   └── BaseDataService.cs  # Base CRUD implementation
│
├── Platforms/              # Platform-specific code (Android, iOS, Windows, Mac)
├── Resources/              # App resources (icons, images, fonts, raw assets)
├── wwwroot/                # Static web assets (CSS, JS)
├── MauiProgram.cs          # App entry point & DI configuration
└── Kwikbooks.csproj        # Project configuration
```

## Architecture Patterns

### Layered Architecture
- **Presentation Layer**: Blazor components in `Components/`
- **Business Layer**: Services in `Services/`
- **Data Layer**: EF Core context and models in `Data/`

### Key Design Principles
1. **Separation of Concerns**: Clear boundaries between UI, business logic, and data
2. **Dependency Injection**: All services registered in `MauiProgram.cs`
3. **Repository Pattern**: `BaseDataService` provides generic CRUD operations
4. **Double-Entry Accounting**: All transactions must balance (debits = credits)

## Technology Stack
- **.NET MAUI Blazor Hybrid**: Cross-platform desktop/mobile app
- **Entity Framework Core**: ORM for database access
- **SQLite**: Local database storage
- **Blazor Components**: Interactive UI

## Naming Conventions
- **Models**: PascalCase, singular (e.g., `Customer`, `Invoice`)
- **Services**: I{Name}Service interface, {Name}Service implementation
- **Components**: PascalCase.razor (e.g., `CustomerList.razor`)
- **Pages**: Named by feature (e.g., `Customers.razor`, `Dashboard.razor`)
