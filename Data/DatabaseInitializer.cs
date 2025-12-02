using Microsoft.EntityFrameworkCore;
using Kwikbooks.Services;

namespace Kwikbooks.Data;

/// <summary>
/// Handles database initialization and migrations.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Ensures the database is created and applies any pending migrations.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<KwikbooksDbContext>();
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Apply any pending migrations (if using migrations)
        // await context.Database.MigrateAsync();
        
        // Seed default data
        await SeedDataAsync(scope.ServiceProvider);
    }

    /// <summary>
    /// Seeds initial data if the database is empty.
    /// </summary>
    public static async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        // Seed default chart of accounts
        var accountService = serviceProvider.GetRequiredService<IAccountService>();
        await accountService.SeedDefaultAccountsAsync();
        
        // Initialize company profile (creates default if none exists)
        var companyService = serviceProvider.GetRequiredService<ICompanyService>();
        await companyService.GetCompanyAsync();
    }
}
