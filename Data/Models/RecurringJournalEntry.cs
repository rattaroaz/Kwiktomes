using System.ComponentModel.DataAnnotations;

namespace Kwiktomes.Data.Models;

/// <summary>
/// Represents a recurring journal entry template.
/// Used to automatically generate journal entries on a schedule.
/// </summary>
public class RecurringJournalEntry : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Memo { get; set; }
    
    /// <summary>
    /// How often to create the journal entry.
    /// </summary>
    public RecurrenceFrequency Frequency { get; set; } = RecurrenceFrequency.Monthly;
    
    /// <summary>
    /// Date when the recurrence starts.
    /// </summary>
    public DateTime StartDate { get; set; } = DateTime.Today;
    
    /// <summary>
    /// Optional end date for the recurrence.
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// Date of the next scheduled entry.
    /// </summary>
    public DateTime NextRunDate { get; set; } = DateTime.Today;
    
    /// <summary>
    /// Date of the last generated entry.
    /// </summary>
    public DateTime? LastRunDate { get; set; }
    
    /// <summary>
    /// Number of entries generated from this template.
    /// </summary>
    public int TimesGenerated { get; set; } = 0;
    
    /// <summary>
    /// Whether this recurring entry is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Whether to automatically post generated entries.
    /// </summary>
    public bool AutoPost { get; set; } = false;
    
    /// <summary>
    /// The template lines for this recurring entry.
    /// </summary>
    public ICollection<RecurringJournalEntryLine> Lines { get; set; } = new List<RecurringJournalEntryLine>();
    
    /// <summary>
    /// Calculates the next run date based on frequency.
    /// </summary>
    public DateTime CalculateNextRunDate(DateTime fromDate)
    {
        return Frequency switch
        {
            RecurrenceFrequency.Daily => fromDate.AddDays(1),
            RecurrenceFrequency.Weekly => fromDate.AddDays(7),
            RecurrenceFrequency.BiWeekly => fromDate.AddDays(14),
            RecurrenceFrequency.Monthly => fromDate.AddMonths(1),
            RecurrenceFrequency.Quarterly => fromDate.AddMonths(3),
            RecurrenceFrequency.Annually => fromDate.AddYears(1),
            _ => fromDate.AddMonths(1)
        };
    }
    
    /// <summary>
    /// Display text for frequency.
    /// </summary>
    public string FrequencyDisplay => Frequency switch
    {
        RecurrenceFrequency.Daily => "Daily",
        RecurrenceFrequency.Weekly => "Weekly",
        RecurrenceFrequency.BiWeekly => "Bi-Weekly",
        RecurrenceFrequency.Monthly => "Monthly",
        RecurrenceFrequency.Quarterly => "Quarterly",
        RecurrenceFrequency.Annually => "Annually",
        _ => "Unknown"
    };
}

/// <summary>
/// A template line for a recurring journal entry.
/// </summary>
public class RecurringJournalEntryLine : BaseEntity
{
    public int RecurringJournalEntryId { get; set; }
    public RecurringJournalEntry RecurringJournalEntry { get; set; } = null!;
    
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Debit amount for the template line.
    /// </summary>
    public decimal DebitAmount { get; set; } = 0m;
    
    /// <summary>
    /// Credit amount for the template line.
    /// </summary>
    public decimal CreditAmount { get; set; } = 0m;
}
