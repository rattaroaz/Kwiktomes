namespace Kwiktomes.Data.Models;

/// <summary>
/// Represents an account in the chart of accounts.
/// Foundation of double-entry accounting.
/// </summary>
public class Account : BaseEntity
{
    public string AccountNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public AccountType AccountType { get; set; }
    public AccountSubType SubType { get; set; }
    
    /// <summary>
    /// Parent account ID for sub-accounts.
    /// </summary>
    public int? ParentAccountId { get; set; }
    public Account? ParentAccount { get; set; }
    public ICollection<Account> SubAccounts { get; set; } = new List<Account>();
    
    /// <summary>
    /// Current balance of the account.
    /// Debits increase Assets/Expenses, Credits increase Liabilities/Equity/Income.
    /// </summary>
    public decimal Balance { get; set; } = 0m;
    
    /// <summary>
    /// Is this account active and available for transactions?
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Is this a system account that cannot be deleted?
    /// </summary>
    public bool IsSystemAccount { get; set; } = false;
    
    /// <summary>
    /// Display order for sorting.
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
}
