using Microsoft.EntityFrameworkCore;
using Kwikbooks.Data;
using Kwikbooks.Services;

namespace Kwikbooks.Extensions;

/// <summary>
/// Extension methods for configuring dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Gets the path to the SQLite database file in the Data folder.
    /// </summary>
    public static string GetDatabasePath()
    {
        // Get the base directory of the application
        var baseDir = AppContext.BaseDirectory;
        
        // Navigate to the project Data folder (for development)
        // In production, this would be configured differently
        var dataFolder = Path.Combine(baseDir, "Data");
        
        // Ensure the Data directory exists
        if (!Directory.Exists(dataFolder))
        {
            Directory.CreateDirectory(dataFolder);
        }
        
        return Path.Combine(dataFolder, "kwikbooks.db");
    }

    /// <summary>
    /// Registers the database context with SQLite.
    /// </summary>
    public static IServiceCollection AddKwikbooksDatabase(this IServiceCollection services)
    {
        var dbPath = GetDatabasePath();
        
        services.AddDbContext<KwikbooksDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        return services;
    }

    /// <summary>
    /// Registers all application services.
    /// </summary>
    public static IServiceCollection AddKwikbooksServices(this IServiceCollection services)
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
