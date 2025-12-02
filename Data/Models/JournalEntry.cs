namespace Kwikbooks.Data.Models;

/// <summary>
/// Represents a journal entry - the fundamental unit of double-entry accounting.
/// Each journal entry must have lines that balance (debits = credits).
/// </summary>
public class JournalEntry : BaseEntity
{
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; } = DateTime.Today;
    public string? Memo { get; set; }
    
    public TransactionStatus Status { get; set; } = TransactionStatus.Draft;
    
    /// <summary>
    /// Is this an adjusting entry?
    /// </summary>
    public bool IsAdjusting { get; set; } = false;
    
    /// <summary>
    /// Reference to source document (invoice #, bill #, etc.)
    /// </summary>
    public string? Reference { get; set; }
    
    /// <summary>
    /// The individual debit/credit lines.
    /// </summary>
    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
    
    /// <summary>
    /// Validates that debits equal credits.
    /// </summary>
    public bool IsBalanced()
    {
        var totalDebits = Lines.Sum(l => l.DebitAmount);
        var totalCredits = Lines.Sum(l => l.CreditAmount);
        return totalDebits == totalCredits;
    }
}

/// <summary>
/// A single line in a journal entry (either a debit or credit).
/// </summary>
public class JournalEntryLine : BaseEntity
{
    public int JournalEntryId { get; set; }
    public JournalEntry JournalEntry { get; set; } = null!;
    
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    
    public string? Description { get; set; }
    
    /// <summary>
    /// Debit amount (increases assets/expenses, decreases liabilities/equity/income).
    /// </summary>
    public decimal DebitAmount { get; set; } = 0m;
    
    /// <summary>
    /// Credit amount (decreases assets/expenses, increases liabilities/equity/income).
    /// </summary>
    public decimal CreditAmount { get; set; } = 0m;
}
