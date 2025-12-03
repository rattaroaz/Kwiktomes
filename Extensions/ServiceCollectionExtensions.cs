using Microsoft.EntityFrameworkCore;
using Kwiktomes.Data;
using Kwiktomes.Services;

namespace Kwiktomes.Extensions;

/// <summary>
/// Extension methods for configuring dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Gets the path to the SQLite database file in the app's local data folder.
    /// Uses the platform-appropriate location for persistent data storage.
    /// </summary>
    public static string GetDatabasePath()
    {
        // Use the app's local data directory for cross-platform compatibility
        // This resolves to the appropriate location on each platform:
        // - Windows: C:\Users\{user}\AppData\Local\{app}
        // - macOS: ~/Library/Application Support/{app}
        // - iOS: {app}/Library
        // - Android: /data/data/{app}/files
        var dataFolder = FileSystem.AppDataDirectory;
        
        // Ensure the directory exists
        if (!Directory.Exists(dataFolder))
        {
            Directory.CreateDirectory(dataFolder);
        }
        
        return Path.Combine(dataFolder, "kwiktomes.db");
    }

    /// <summary>
    /// Registers the database context with SQLite.
    /// </summary>
    public static IServiceCollection AddKwiktomesDatabase(this IServiceCollection services)
    {
        var dbPath = GetDatabasePath();
        
        services.AddDbContext<KwiktomesDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        return services;
    }

    /// <summary>
    /// Registers all application services.
    /// </summary>
    public static IServiceCollection AddKwiktomesServices(this IServiceCollection services)
    {
        // Company profile service
        services.AddScoped<ICompanyService, CompanyService>();
        
        // Chart of Accounts service
        services.AddScoped<IAccountService, AccountService>();
        
        // Customer service
        services.AddScoped<ICustomerService, CustomerService>();
        
        // Vendor service
        services.AddScoped<IVendorService, VendorService>();
        
        // Product service
        services.AddScoped<IProductService, ProductService>();
        
        // Journal Entry service
        services.AddScoped<IJournalEntryService, JournalEntryService>();
        
        // Dashboard service
        services.AddScoped<IDashboardService, DashboardService>();
        
        // Invoice service
        services.AddScoped<IInvoiceService, InvoiceService>();
        
        // Bill service
        services.AddScoped<IBillService, BillService>();
        
        // Invoice PDF service
        services.AddScoped<IInvoicePdfService, InvoicePdfService>();
        
        // Bank service
        services.AddScoped<IBankService, BankService>();
        
        // Report service
        services.AddScoped<IReportService, ReportService>();
        
        // User preferences service
        services.AddScoped<IUserPreferencesService, UserPreferencesService>();
        
        // Backup service
        services.AddScoped<IBackupService, BackupService>();
        
        // Import service (QuickBooks file import)
        services.AddScoped<IImportService, ImportService>();

        return services;
    }
}
