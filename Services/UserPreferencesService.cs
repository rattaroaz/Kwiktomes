using Microsoft.EntityFrameworkCore;
using Kwikbooks.Data;
using Kwikbooks.Data.Models;

namespace Kwikbooks.Services;

/// <summary>
/// Service for managing user preferences.
/// </summary>
public interface IUserPreferencesService
{
    /// <summary>
    /// Gets the user preferences. Creates defaults if none exist.
    /// </summary>
    Task<UserPreferences> GetPreferencesAsync();
    
    /// <summary>
    /// Updates the user preferences.
    /// </summary>
    Task<UserPreferences> UpdatePreferencesAsync(UserPreferences preferences);
    
    /// <summary>
    /// Resets preferences to default values.
    /// </summary>
    Task<UserPreferences> ResetToDefaultsAsync();
}

/// <summary>
/// Implementation of the user preferences service.
/// </summary>
public class UserPreferencesService : IUserPreferencesService
{
    private readonly KwikbooksDbContext _context;
    private UserPreferences? _cachedPreferences;

    public UserPreferencesService(KwikbooksDbContext context)
    {
        _context = context;
    }

    public async Task<UserPreferences> GetPreferencesAsync()
    {
        if (_cachedPreferences != null)
            return _cachedPreferences;

        var preferences = await _context.UserPreferences.FirstOrDefaultAsync();
        
        if (preferences == null)
        {
            preferences = new UserPreferences
            {
                CreatedAt = DateTime.UtcNow
            };
            
            _context.UserPreferences.Add(preferences);
            await _context.SaveChangesAsync();
        }

        _cachedPreferences = preferences;
        return preferences;
    }

    public async Task<UserPreferences> UpdatePreferencesAsync(UserPreferences preferences)
    {
        preferences.UpdatedAt = DateTime.UtcNow;
        _context.UserPreferences.Update(preferences);
        await _context.SaveChangesAsync();
        
        _cachedPreferences = preferences;
        return preferences;
    }

    public async Task<UserPreferences> ResetToDefaultsAsync()
    {
        var preferences = await GetPreferencesAsync();
        
        // Reset all preferences to defaults
        preferences.Theme = ApplicationTheme.System;
        preferences.SidebarCollapsed = false;
        preferences.DefaultPageSize = 25;
        preferences.DefaultLandingPage = "/";
        preferences.DefaultReportDaysBack = 30;
        preferences.DefaultExportFormat = ExportFormat.PDF;
        preferences.ShowOverdueInvoiceAlerts = true;
        preferences.ShowOverdueBillAlerts = true;
        preferences.ShowLowInventoryAlerts = true;
        preferences.UpcomingDueAlertDays = 7;
        preferences.DateFormat = "MM/dd/yyyy";
        preferences.TimeFormat = TimeFormatPreference.TwelveHour;
        preferences.NumberFormat = NumberFormatPreference.USFormat;
        preferences.ConfirmBeforeDelete = true;
        preferences.ConfirmBeforeVoid = true;
        preferences.AutoSaveDrafts = true;
        preferences.ShowProfitLossWidget = true;
        preferences.ShowCashFlowWidget = true;
        preferences.ShowReceivablesWidget = true;
        preferences.ShowPayablesWidget = true;
        preferences.ShowRecentTransactionsWidget = true;
        
        return await UpdatePreferencesAsync(preferences);
    }
}
