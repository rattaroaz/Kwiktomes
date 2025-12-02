using Microsoft.EntityFrameworkCore;
using Kwikbooks.Services;

namespace Kwikbooks.Data;

/// <summary>
/// Handles database initialization and migrations.
/// </summary>
public static class DatabaseInitializer
{
    // Increment this when schema changes to force database recreation
    private const int SchemaVersion = 2;
    
    /// <summary>
    /// Ensures the database is created and applies any pending migrations.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<KwikbooksDbContext>();
        
        // For development: check if we need to recreate the database
        var needsRecreate = await CheckSchemaVersionAsync(context);
        
        if (needsRecreate)
        {
            // Delete and recreate with new schema
            await context.Database.EnsureDeletedAsync();
        }
        
        // Create database with all tables
        await context.Database.EnsureCreatedAsync();
        
        // Update schema version
        await UpdateSchemaVersionAsync(context);
        
        // Seed default data
        await SeedDataAsync(scope.ServiceProvider);
    }
    
    /// <summary>
    /// Checks if the database schema version matches current version.
    /// Returns true if database needs to be recreated.
    /// </summary>
    private static async Task<bool> CheckSchemaVersionAsync(KwikbooksDbContext context)
    {
        try
        {
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            try
            {
                // Check if version table exists
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT Version FROM SchemaVersion LIMIT 1";
                var result = await cmd.ExecuteScalarAsync();
                
                if (result != null && int.TryParse(result.ToString(), out int version))
                {
                    return version < SchemaVersion;
                }
                
                return true; // No version found, recreate
            }
            catch
            {
                return true; // Table doesn't exist, needs recreation
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
        catch
        {
            return false; // Database doesn't exist yet
        }
    }
    
    /// <summary>
    /// Updates the schema version in the database.
    /// </summary>
    private static async Task UpdateSchemaVersionAsync(KwikbooksDbContext context)
    {
        try
        {
            var connection = context.Database.GetDbConnection();
            await connection.OpenAsync();
            
            try
            {
                // Create version table if not exists
                using var createCmd = connection.CreateCommand();
                createCmd.CommandText = "CREATE TABLE IF NOT EXISTS SchemaVersion (Version INTEGER NOT NULL)";
                await createCmd.ExecuteNonQueryAsync();
                
                // Clear and insert current version
                using var deleteCmd = connection.CreateCommand();
                deleteCmd.CommandText = "DELETE FROM SchemaVersion";
                await deleteCmd.ExecuteNonQueryAsync();
                
                using var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = $"INSERT INTO SchemaVersion (Version) VALUES ({SchemaVersion})";
                await insertCmd.ExecuteNonQueryAsync();
            }
            finally
            {
                await connection.CloseAsync();
            }
        }
        catch
        {
            // Ignore errors - not critical
        }
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
