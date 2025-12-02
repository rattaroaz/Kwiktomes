using System.ComponentModel.DataAnnotations;

namespace Kwikbooks.Data.Models;

/// <summary>
/// Represents user preferences and application settings.
/// </summary>
public class UserPreferences : BaseEntity
{
    // Display Settings
    /// <summary>
    /// Application theme: Light, Dark, or System
    /// </summary>
    public ApplicationTheme Theme { get; set; } = ApplicationTheme.System;
    
    /// <summary>
    /// Sidebar collapsed by default
    /// </summary>
    public bool SidebarCollapsed { get; set; } = false;
    
    /// <summary>
    /// Default page size for lists
    /// </summary>
    [Range(10, 100)]
    public int DefaultPageSize { get; set; } = 25;
    
    // Default Views
    /// <summary>
    /// Default dashboard page on login
    /// </summary>
    [MaxLength(100)]
    public string DefaultLandingPage { get; set; } = "/";
    
    /// <summary>
    /// Default date range for reports (days back from today)
    /// </summary>
    public int DefaultReportDaysBack { get; set; } = 30;
    
    /// <summary>
    /// Default report format for exports
    /// </summary>
    public ExportFormat DefaultExportFormat { get; set; } = ExportFormat.PDF;
    
    // Notification Settings
    /// <summary>
    /// Show overdue invoice alerts
    /// </summary>
    public bool ShowOverdueInvoiceAlerts { get; set; } = true;
    
    /// <summary>
    /// Show overdue bill alerts
    /// </summary>
    public bool ShowOverdueBillAlerts { get; set; } = true;
    
    /// <summary>
    /// Show low inventory alerts
    /// </summary>
    public bool ShowLowInventoryAlerts { get; set; } = true;
    
    /// <summary>
    /// Days before due date to show upcoming due alerts
    /// </summary>
    [Range(0, 30)]
    public int UpcomingDueAlertDays { get; set; } = 7;
    
    // Number/Date Formatting
    /// <summary>
    /// Date format preference
    /// </summary>
    [MaxLength(20)]
    public string DateFormat { get; set; } = "MM/dd/yyyy";
    
    /// <summary>
    /// Time format preference (12h or 24h)
    /// </summary>
    public TimeFormatPreference TimeFormat { get; set; } = TimeFormatPreference.TwelveHour;
    
    /// <summary>
    /// Number format for currency display
    /// </summary>
    public NumberFormatPreference NumberFormat { get; set; } = NumberFormatPreference.USFormat;
    
    // Confirmation Settings
    /// <summary>
    /// Confirm before deleting records
    /// </summary>
    public bool ConfirmBeforeDelete { get; set; } = true;
    
    /// <summary>
    /// Confirm before voiding transactions
    /// </summary>
    public bool ConfirmBeforeVoid { get; set; } = true;
    
    /// <summary>
    /// Auto-save draft transactions
    /// </summary>
    public bool AutoSaveDrafts { get; set; } = true;
    
    // Dashboard Preferences
    /// <summary>
    /// Show profit/loss widget on dashboard
    /// </summary>
    public bool ShowProfitLossWidget { get; set; } = true;
    
    /// <summary>
    /// Show cash flow widget on dashboard
    /// </summary>
    public bool ShowCashFlowWidget { get; set; } = true;
    
    /// <summary>
    /// Show receivables widget on dashboard
    /// </summary>
    public bool ShowReceivablesWidget { get; set; } = true;
    
    /// <summary>
    /// Show payables widget on dashboard
    /// </summary>
    public bool ShowPayablesWidget { get; set; } = true;
    
    /// <summary>
    /// Show recent transactions widget on dashboard
    /// </summary>
    public bool ShowRecentTransactionsWidget { get; set; } = true;
}

/// <summary>
/// Application theme options
/// </summary>
public enum ApplicationTheme
{
    Light = 1,
    Dark = 2,
    System = 3  // Follow system preference
}

/// <summary>
/// Export format options
/// </summary>
public enum ExportFormat
{
    PDF = 1,
    Excel = 2,
    CSV = 3
}

/// <summary>
/// Time format preferences
/// </summary>
public enum TimeFormatPreference
{
    TwelveHour = 1,  // 12:00 PM
    TwentyFourHour = 2  // 14:00
}

/// <summary>
/// Number format preferences for currency display
/// </summary>
public enum NumberFormatPreference
{
    USFormat = 1,       // 1,234.56
    EuropeanFormat = 2, // 1.234,56
    IndianFormat = 3,   // 1,23,456.78
    SwissFormat = 4     // 1'234.56
}
