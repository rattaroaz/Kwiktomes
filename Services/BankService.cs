using Microsoft.EntityFrameworkCore;
using Kwiktomes.Data;
using Kwiktomes.Data.Models;

namespace Kwiktomes.Services;

/// <summary>
/// Service for managing bank accounts and transactions.
/// </summary>
public interface IBankService
{
    // Bank Account operations
    Task<IEnumerable<BankAccount>> GetAllAccountsAsync();
    Task<BankAccount?> GetAccountByIdAsync(int id);
    Task<BankAccount?> GetAccountWithTransactionsAsync(int id, int? limit = null);
    Task<BankAccount> CreateAccountAsync(BankAccount account);
    Task<BankAccount> UpdateAccountAsync(BankAccount account);
    Task DeleteAccountAsync(int id);
    Task<IEnumerable<BankAccount>> GetActiveAccountsAsync();
    
    // Transaction operations
    Task<IEnumerable<BankTransaction>> GetTransactionsAsync(int accountId, DateTime? startDate = null, DateTime? endDate = null);
    Task<BankTransaction?> GetTransactionByIdAsync(int id);
    Task<BankTransaction> CreateTransactionAsync(BankTransaction transaction);
    Task<BankTransaction> UpdateTransactionAsync(BankTransaction transaction);
    Task DeleteTransactionAsync(int id);
    
    // Transfer
    Task<(BankTransaction from, BankTransaction to)> CreateTransferAsync(int fromAccountId, int toAccountId, decimal amount, DateTime date, string? description = null);
    
    // Reconciliation
    Task<ReconciliationSummary> GetReconciliationSummaryAsync(int accountId, decimal statementBalance, DateTime statementDate);
    Task ReconcileTransactionsAsync(int accountId, List<int> transactionIds, decimal statementBalance, DateTime statementDate);
    Task ClearTransactionAsync(int transactionId);
    Task UnclearTransactionAsync(int transactionId);
    
    // Import
    Task<ImportResult> ImportTransactionsAsync(int accountId, List<ImportedTransaction> transactions);
    
    // Summary
    Task<BankingSummary> GetBankingSummaryAsync();
    
    // Recalculate balances
    Task RecalculateBalancesAsync(int accountId);
}

/// <summary>
/// Summary of banking across all accounts.
/// </summary>
public class BankingSummary
{
    public int TotalAccounts { get; set; }
    public decimal TotalCashBalance { get; set; }
    public decimal TotalCreditBalance { get; set; }
    public int UnreconciledTransactions { get; set; }
    public int RecentTransactions { get; set; }
}

/// <summary>
/// Summary for reconciliation.
/// </summary>
public class ReconciliationSummary
{
    public decimal BeginningBalance { get; set; }
    public decimal ClearedDeposits { get; set; }
    public decimal ClearedWithdrawals { get; set; }
    public decimal ClearedBalance { get; set; }
    public decimal StatementBalance { get; set; }
    public decimal Difference { get; set; }
    public int ClearedCount { get; set; }
    public int UnclearedCount { get; set; }
    public List<BankTransaction> UnclearedTransactions { get; set; } = new();
}

/// <summary>
/// Result of import operation.
/// </summary>
public class ImportResult
{
    public int ImportedCount { get; set; }
    public int DuplicateCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Transaction data from import.
/// </summary>
public class ImportedTransaction
{
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public BankTransactionType Type { get; set; } = BankTransactionType.Other;
}

/// <summary>
/// Implementation of the bank service.
/// </summary>
public class BankService : IBankService
{
    private readonly KwiktomesDbContext _context;

    public BankService(KwiktomesDbContext context)
    {
        _context = context;
    }

    #region Bank Account Operations

    public async Task<IEnumerable<BankAccount>> GetAllAccountsAsync()
    {
        return await _context.BankAccounts
            .Include(a => a.LinkedAccount)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<BankAccount?> GetAccountByIdAsync(int id)
    {
        return await _context.BankAccounts
            .Include(a => a.LinkedAccount)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<BankAccount?> GetAccountWithTransactionsAsync(int id, int? limit = null)
    {
        var query = _context.BankAccounts
            .Include(a => a.LinkedAccount)
            .Include(a => a.Transactions.OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.Id))
            .AsSplitQuery();

        var account = await query.FirstOrDefaultAsync(a => a.Id == id);
        
        if (account != null && limit.HasValue)
        {
            account.Transactions = account.Transactions.Take(limit.Value).ToList();
        }
        
        return account;
    }

    public async Task<BankAccount> CreateAccountAsync(BankAccount account)
    {
        account.CurrentBalance = account.OpeningBalance;
        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync();
        
        // Create opening balance transaction if there's an opening balance
        if (account.OpeningBalance != 0)
        {
            var openingTransaction = new BankTransaction
            {
                BankAccountId = account.Id,
                TransactionDate = account.OpeningBalanceDate,
                TransactionType = account.OpeningBalance > 0 ? BankTransactionType.Deposit : BankTransactionType.Withdrawal,
                Description = "Opening Balance",
                Amount = account.OpeningBalance,
                RunningBalance = account.OpeningBalance,
                IsCleared = true,
                IsReconciled = true
            };
            _context.BankTransactions.Add(openingTransaction);
            await _context.SaveChangesAsync();
        }
        
        return account;
    }

    public async Task<BankAccount> UpdateAccountAsync(BankAccount account)
    {
        var existing = await _context.BankAccounts.FindAsync(account.Id);
        if (existing == null)
            throw new InvalidOperationException("Bank account not found");

        existing.Name = account.Name;
        existing.BankName = account.BankName;
        existing.AccountType = account.AccountType;
        existing.AccountNumber = account.AccountNumber;
        existing.RoutingNumber = account.RoutingNumber;
        existing.LinkedAccountId = account.LinkedAccountId;
        existing.Description = account.Description;
        existing.IsActive = account.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteAccountAsync(int id)
    {
        var account = await _context.BankAccounts.FindAsync(id);
        if (account != null)
        {
            _context.BankAccounts.Remove(account);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<BankAccount>> GetActiveAccountsAsync()
    {
        return await _context.BankAccounts
            .Where(a => a.IsActive)
            .OrderBy(a => a.Name)
            .ToListAsync();
    }

    #endregion

    #region Transaction Operations

    public async Task<IEnumerable<BankTransaction>> GetTransactionsAsync(int accountId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.BankTransactions
            .Include(t => t.CategoryAccount)
            .Include(t => t.TransferAccount)
            .Where(t => t.BankAccountId == accountId);

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        return await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.Id)
            .ToListAsync();
    }

    public async Task<BankTransaction?> GetTransactionByIdAsync(int id)
    {
        return await _context.BankTransactions
            .Include(t => t.BankAccount)
            .Include(t => t.CategoryAccount)
            .Include(t => t.TransferAccount)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<BankTransaction> CreateTransactionAsync(BankTransaction transaction)
    {
        // Get account
        var account = await _context.BankAccounts.FindAsync(transaction.BankAccountId);
        if (account == null)
            throw new InvalidOperationException("Bank account not found");

        // Update running balance
        account.CurrentBalance += transaction.Amount;
        transaction.RunningBalance = account.CurrentBalance;
        account.UpdatedAt = DateTime.UtcNow;

        _context.BankTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return transaction;
    }

    public async Task<BankTransaction> UpdateTransactionAsync(BankTransaction transaction)
    {
        var existing = await _context.BankTransactions.FindAsync(transaction.Id);
        if (existing == null)
            throw new InvalidOperationException("Transaction not found");

        // If amount changed, need to recalculate balances
        var amountDiff = transaction.Amount - existing.Amount;

        existing.TransactionDate = transaction.TransactionDate;
        existing.TransactionType = transaction.TransactionType;
        existing.Payee = transaction.Payee;
        existing.Description = transaction.Description;
        existing.ReferenceNumber = transaction.ReferenceNumber;
        existing.Amount = transaction.Amount;
        existing.CategoryAccountId = transaction.CategoryAccountId;
        existing.VendorId = transaction.VendorId;
        existing.CustomerId = transaction.CustomerId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Recalculate balances if amount changed
        if (amountDiff != 0)
        {
            await RecalculateBalancesAsync(existing.BankAccountId);
        }

        return existing;
    }

    public async Task DeleteTransactionAsync(int id)
    {
        var transaction = await _context.BankTransactions.FindAsync(id);
        if (transaction != null)
        {
            var accountId = transaction.BankAccountId;
            _context.BankTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
            await RecalculateBalancesAsync(accountId);
        }
    }

    #endregion

    #region Transfer Operations

    public async Task<(BankTransaction from, BankTransaction to)> CreateTransferAsync(
        int fromAccountId, int toAccountId, decimal amount, DateTime date, string? description = null)
    {
        var fromAccount = await _context.BankAccounts.FindAsync(fromAccountId);
        var toAccount = await _context.BankAccounts.FindAsync(toAccountId);

        if (fromAccount == null || toAccount == null)
            throw new InvalidOperationException("Invalid account(s)");

        var transferDesc = description ?? $"Transfer to {toAccount.Name}";
        var transferDescTo = description ?? $"Transfer from {fromAccount.Name}";

        // Create withdrawal from source
        var fromTransaction = new BankTransaction
        {
            BankAccountId = fromAccountId,
            TransactionDate = date,
            TransactionType = BankTransactionType.Transfer,
            Description = transferDesc,
            Amount = -Math.Abs(amount),
            TransferAccountId = toAccountId
        };

        // Create deposit to destination
        var toTransaction = new BankTransaction
        {
            BankAccountId = toAccountId,
            TransactionDate = date,
            TransactionType = BankTransactionType.Transfer,
            Description = transferDescTo,
            Amount = Math.Abs(amount),
            TransferAccountId = fromAccountId
        };

        // Update balances
        fromAccount.CurrentBalance -= Math.Abs(amount);
        toAccount.CurrentBalance += Math.Abs(amount);
        
        fromTransaction.RunningBalance = fromAccount.CurrentBalance;
        toTransaction.RunningBalance = toAccount.CurrentBalance;

        fromAccount.UpdatedAt = DateTime.UtcNow;
        toAccount.UpdatedAt = DateTime.UtcNow;

        _context.BankTransactions.Add(fromTransaction);
        _context.BankTransactions.Add(toTransaction);
        await _context.SaveChangesAsync();

        return (fromTransaction, toTransaction);
    }

    #endregion

    #region Reconciliation

    public async Task<ReconciliationSummary> GetReconciliationSummaryAsync(int accountId, decimal statementBalance, DateTime statementDate)
    {
        var account = await _context.BankAccounts.FindAsync(accountId);
        if (account == null)
            throw new InvalidOperationException("Account not found");

        var transactions = await _context.BankTransactions
            .Where(t => t.BankAccountId == accountId && t.TransactionDate <= statementDate)
            .ToListAsync();

        var clearedTransactions = transactions.Where(t => t.IsCleared || t.IsReconciled).ToList();
        var unclearedTransactions = transactions.Where(t => !t.IsCleared && !t.IsReconciled).ToList();

        var beginningBalance = account.LastReconciledBalance ?? account.OpeningBalance;
        var clearedDeposits = clearedTransactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
        var clearedWithdrawals = Math.Abs(clearedTransactions.Where(t => t.Amount < 0).Sum(t => t.Amount));
        var clearedBalance = beginningBalance + clearedDeposits - clearedWithdrawals;

        return new ReconciliationSummary
        {
            BeginningBalance = beginningBalance,
            ClearedDeposits = clearedDeposits,
            ClearedWithdrawals = clearedWithdrawals,
            ClearedBalance = clearedBalance,
            StatementBalance = statementBalance,
            Difference = clearedBalance - statementBalance,
            ClearedCount = clearedTransactions.Count,
            UnclearedCount = unclearedTransactions.Count,
            UnclearedTransactions = unclearedTransactions.OrderByDescending(t => t.TransactionDate).ToList()
        };
    }

    public async Task ReconcileTransactionsAsync(int accountId, List<int> transactionIds, decimal statementBalance, DateTime statementDate)
    {
        var account = await _context.BankAccounts.FindAsync(accountId);
        if (account == null)
            throw new InvalidOperationException("Account not found");

        var transactions = await _context.BankTransactions
            .Where(t => transactionIds.Contains(t.Id))
            .ToListAsync();

        foreach (var transaction in transactions)
        {
            transaction.IsReconciled = true;
            transaction.ReconciledDate = DateTime.UtcNow;
            transaction.IsCleared = true;
        }

        account.LastReconciledDate = statementDate;
        account.LastReconciledBalance = statementBalance;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task ClearTransactionAsync(int transactionId)
    {
        var transaction = await _context.BankTransactions.FindAsync(transactionId);
        if (transaction != null)
        {
            transaction.IsCleared = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UnclearTransactionAsync(int transactionId)
    {
        var transaction = await _context.BankTransactions.FindAsync(transactionId);
        if (transaction != null && !transaction.IsReconciled)
        {
            transaction.IsCleared = false;
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Import

    public async Task<ImportResult> ImportTransactionsAsync(int accountId, List<ImportedTransaction> transactions)
    {
        var result = new ImportResult();
        var account = await _context.BankAccounts.FindAsync(accountId);
        
        if (account == null)
        {
            result.Errors.Add("Account not found");
            result.ErrorCount = 1;
            return result;
        }

        // Get existing transactions to check for duplicates
        var existingTransactions = await _context.BankTransactions
            .Where(t => t.BankAccountId == accountId)
            .ToListAsync();

        foreach (var imported in transactions)
        {
            // Check for duplicate (same date, amount, and description)
            var isDuplicate = existingTransactions.Any(t =>
                t.TransactionDate.Date == imported.Date.Date &&
                t.Amount == imported.Amount &&
                t.Description == imported.Description);

            if (isDuplicate)
            {
                result.DuplicateCount++;
                continue;
            }

            try
            {
                var transaction = new BankTransaction
                {
                    BankAccountId = accountId,
                    TransactionDate = imported.Date,
                    TransactionType = imported.Type,
                    Description = imported.Description,
                    ReferenceNumber = imported.ReferenceNumber,
                    Amount = imported.Amount
                };

                account.CurrentBalance += imported.Amount;
                transaction.RunningBalance = account.CurrentBalance;

                _context.BankTransactions.Add(transaction);
                result.ImportedCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error importing transaction: {ex.Message}");
                result.ErrorCount++;
            }
        }

        account.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return result;
    }

    #endregion

    #region Summary

    public async Task<BankingSummary> GetBankingSummaryAsync()
    {
        var accounts = await _context.BankAccounts.Where(a => a.IsActive).ToListAsync();
        var recentDate = DateTime.Today.AddDays(-30);

        var unreconciledCount = await _context.BankTransactions
            .Where(t => !t.IsReconciled && t.BankAccount.IsActive)
            .CountAsync();

        var recentCount = await _context.BankTransactions
            .Where(t => t.TransactionDate >= recentDate && t.BankAccount.IsActive)
            .CountAsync();

        return new BankingSummary
        {
            TotalAccounts = accounts.Count,
            TotalCashBalance = accounts
                .Where(a => a.AccountType != BankAccountType.CreditCard && a.AccountType != BankAccountType.LineOfCredit)
                .Sum(a => a.CurrentBalance),
            TotalCreditBalance = accounts
                .Where(a => a.AccountType == BankAccountType.CreditCard || a.AccountType == BankAccountType.LineOfCredit)
                .Sum(a => a.CurrentBalance),
            UnreconciledTransactions = unreconciledCount,
            RecentTransactions = recentCount
        };
    }

    #endregion

    #region Balance Recalculation

    public async Task RecalculateBalancesAsync(int accountId)
    {
        var account = await _context.BankAccounts.FindAsync(accountId);
        if (account == null) return;

        var transactions = await _context.BankTransactions
            .Where(t => t.BankAccountId == accountId)
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.Id)
            .ToListAsync();

        decimal runningBalance = account.OpeningBalance;

        foreach (var transaction in transactions)
        {
            runningBalance += transaction.Amount;
            transaction.RunningBalance = runningBalance;
        }

        account.CurrentBalance = runningBalance;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    #endregion
}
