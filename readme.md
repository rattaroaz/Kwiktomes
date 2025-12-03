# Kwiktomes

A cross-platform personal and small business accounting application built with .NET MAUI Blazor Hybrid.

## Features

- **Dashboard** - Overview of your financial status with key metrics and recent activity
- **Chart of Accounts** - Manage your account structure with support for sub-accounts
- **Customers** - Track customer information and contact details
- **Vendors** - Manage vendor/supplier records
- **Products & Services** - Catalog your products and services with pricing
- **Invoices** - Create and manage customer invoices with PDF export
- **Bills** - Track bills from vendors
- **Banking** - Import and categorize bank transactions
- **Journal Entries** - Record manual accounting entries with double-entry validation
- **Recurring Entries** - Set up automated recurring journal entries
- **Reports** - Generate financial reports (Balance Sheet, Income Statement, etc.)
- **Settings** - Configure company profile, backup/restore data, and import from other systems

## Requirements

- .NET 8.0 SDK or later
- Visual Studio 2022 (17.8+) with MAUI workload, or
- VS Code with C# Dev Kit extension

## Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/Kwiktomes.git
   cd Kwiktomes
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the application**
   ```bash
   dotnet build
   dotnet run
   ```

   Or open `Kwiktomes.slnx` in Visual Studio and press F5.

## Database

Kwiktomes uses SQLite for local data storage. The database file (`kwiktomes.db`) is stored in:

- **Windows**: `C:\Users\{username}\AppData\Local\Kwiktomes`
- **macOS**: `~/Library/Application Support/Kwiktomes`
- **iOS/Android**: App's sandboxed data directory

## Architecture

The app follows a layered architecture:

- **Presentation Layer** - Blazor components in `Components/`
- **Business Layer** - Services in `Services/`
- **Data Layer** - Entity Framework Core context and models in `Data/`

See [ARCHITECTURE.md](ARCHITECTURE.md) for more details.

## Backup & Restore

Use the Settings page to:
- **Backup** - Export your data to a `.kwiktomes` backup file
- **Restore** - Import data from a backup file

## License

This project is licensed under the GNU General Public License v3.0 or later - see the [License.txt](License.txt) file for details.
