using System.ComponentModel.DataAnnotations;

namespace Kwikbooks.Data.Models;

/// <summary>
/// Represents a bank account for tracking banking transactions.
/// </summary>
public class BankAccount : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? BankName { get; set; }
    
    public BankAccountType AccountType { get; set; } = BankAccountType.Checking;
    
    [MaxLength(50)]
    public string? AccountNumber { get; set; }
    
    [MaxLength(20)]
    public string? RoutingNumber { get; set; }
    
    /// <summary>
    /// Current balance as of last reconciliation or manual entry.
    /// </summary>
    public decimal CurrentBalance { get; set; } = 0m;
    
    /// <summary>
    /// Opening balance when the account was added.
    /// </summary>
    public decimal OpeningBalance { get; set; } = 0m;
    
    /// <summary>
    /// Date of opening balance.
    /// </summary>
    public DateTime OpeningBalanceDate { get; set; } = DateTime.Today;
    
    /// <summary>
    /// Last reconciled date.
    /// </summary>
    public DateTime? LastReconciledDate { get; set; }
    
    /// <summary>
    /// Statement ending balance from last reconciliation.
    /// </summary>
    public decimal? LastReconciledBalance { get; set; }
    
    /// <summary>
    /// Linked Chart of Accounts account (Asset account).
    /// </summary>
    public int? LinkedAccountId { get; set; }
    public Account? LinkedAccount { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Bank transactions for this account.
    /// </summary>
    public ICollection<BankTransaction> Transactions { get; set; } = new List<BankTransaction>();
    
    // Computed
    public string MaskedAccountNumber => !string.IsNullOrEmpty(AccountNumber) && AccountNumber.Length > 4
        ? $"****{AccountNumber[^4..]}"
        : AccountNumber ?? "";
        
    public string AccountTypeDisplay => AccountType switch
    {
        BankAccountType.Checking => "Checking",
        BankAccountType.Savings => "Savings",
        BankAccountType.CreditCard => "Credit Card",
        BankAccountType.MoneyMarket => "Money Market",
        BankAccountType.LineOfCredit => "Line of Credit",
        BankAccountType.Other => "Other",
        _ => "Unknown"
    };
}

/// <summary>
/// Bank transaction record for deposits, withdrawals, and transfers.
/// </summary>
public class BankTransaction : BaseEntity
{
    public int BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;
    
    public DateTime TransactionDate { get; set; } = DateTime.Today;
    
    public BankTransactionType TransactionType { get; set; }
    
    [MaxLength(200)]
    public string? Payee { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// Positive for deposits, negative for withdrawals.
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Running balance after this transaction.
    /// </summary>
    public decimal RunningBalance { get; set; }
    
    /// <summary>
    /// Category account for this transaction.
    /// </summary>
    public int? CategoryAccountId { get; set; }
    public Account? CategoryAccount { get; set; }
    
    /// <summary>
    /// For transfers, the other bank account involved.
    /// </summary>
    public int? TransferAccountId { get; set; }
    public BankAccount? TransferAccount { get; set; }
    
    /// <summary>
    /// Linked vendor for expenses.
    /// </summary>
    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    
    /// <summary>
    /// Linked customer for deposits.
    /// </summary>
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    
    /// <summary>
    /// Whether this transaction has been reconciled.
    /// </summary>
    public bool IsReconciled { get; set; } = false;
    
    /// <summary>
    /// Date when the transaction was reconciled.
    /// </summary>
    public DateTime? ReconciledDate { get; set; }
    
    /// <summary>
    /// Whether this transaction was cleared by the bank.
    /// </summary>
    public bool IsCleared { get; set; } = false;
    
    // Computed
    public bool IsDeposit => Amount > 0;
    public bool IsWithdrawal => Amount < 0;
    public decimal AbsoluteAmount => Math.Abs(Amount);
    
    public string TransactionTypeDisplay => TransactionType switch
    {
        BankTransactionType.Deposit => "Deposit",
        BankTransactionType.Withdrawal => "Withdrawal",
        BankTransactionType.Transfer => "Transfer",
        BankTransactionType.Check => "Check",
        BankTransactionType.ATM => "ATM",
        BankTransactionType.DebitCard => "Debit Card",
        BankTransactionType.ACH => "ACH",
        BankTransactionType.Wire => "Wire Transfer",
        BankTransactionType.Fee => "Bank Fee",
        BankTransactionType.Interest => "Interest",
        BankTransactionType.Adjustment => "Adjustment",
        _ => "Other"
    };
}

/// <summary>
/// Types of bank accounts.
/// </summary>
public enum BankAccountType
{
    Checking = 1,
    Savings = 2,
    CreditCard = 3,
    MoneyMarket = 4,
    LineOfCredit = 5,
    Other = 99
}

/// <summary>
/// Types of bank transactions.
/// </summary>
public enum BankTransactionType
{
    Deposit = 1,
    Withdrawal = 2,
    Transfer = 3,
    Check = 4,
    ATM = 5,
    DebitCard = 6,
    ACH = 7,
    Wire = 8,
    Fee = 9,
    Interest = 10,
    Adjustment = 11,
    Other = 99
}
