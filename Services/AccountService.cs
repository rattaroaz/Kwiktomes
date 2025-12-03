using Microsoft.EntityFrameworkCore;
using Kwiktomes.Data;
using Kwiktomes.Data.Models;

namespace Kwiktomes.Services;

/// <summary>
/// Service for managing the Chart of Accounts.
/// </summary>
public interface IAccountService : IDataService<Account>
{
    /// <summary>
    /// Gets all accounts organized by type.
    /// </summary>
    Task<Dictionary<AccountType, List<Account>>> GetAccountsByTypeAsync();
    
    /// <summary>
    /// Gets accounts filtered by type.
    /// </summary>
    Task<IEnumerable<Account>> GetAccountsByTypeAsync(AccountType type);
    
    /// <summary>
    /// Gets accounts filtered by sub-type.
    /// </summary>
    Task<IEnumerable<Account>> GetAccountsBySubTypeAsync(AccountSubType subType);
    
    /// <summary>
    /// Gets only active accounts.
    /// </summary>
    Task<IEnumerable<Account>> GetActiveAccountsAsync();
    
    /// <summary>
    /// Gets an account by its account number.
    /// </summary>
    Task<Account?> GetByAccountNumberAsync(string accountNumber);
    
    /// <summary>
    /// Gets child accounts for a parent account.
    /// </summary>
    Task<IEnumerable<Account>> GetSubAccountsAsync(int parentAccountId);
    
    /// <summary>
    /// Checks if an account number is already in use.
    /// </summary>
    Task<bool> AccountNumberExistsAsync(string accountNumber, int? excludeId = null);
    
    /// <summary>
    /// Seeds the default chart of accounts if none exist.
    /// </summary>
    Task SeedDefaultAccountsAsync();
    
    /// <summary>
    /// Updates account balance.
    /// </summary>
    Task UpdateBalanceAsync(int accountId, decimal amount, bool isDebit);
    
    /// <summary>
    /// Gets the next available account number for a given type.
    /// </summary>
    Task<string> GetNextAccountNumberAsync(AccountType type);
}

/// <summary>
/// Implementation of the account service.
/// </summary>
public class AccountService : BaseDataService<Account>, IAccountService
{
    public AccountService(KwiktomesDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Account>> GetAllAsync()
    {
        return await _dbSet
            .Include(a => a.ParentAccount)
            .Include(a => a.SubAccounts)
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.DisplayOrder)
            .ThenBy(a => a.AccountNumber)
            .ToListAsync();
    }

    public async Task<Dictionary<AccountType, List<Account>>> GetAccountsByTypeAsync()
    {
        var accounts = await _dbSet
            .Where(a => a.ParentAccountId == null) // Top-level accounts only
            .Include(a => a.SubAccounts)
            .OrderBy(a => a.DisplayOrder)
            .ThenBy(a => a.AccountNumber)
            .ToListAsync();

        return accounts
            .GroupBy(a => a.AccountType)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    public async Task<IEnumerable<Account>> GetAccountsByTypeAsync(AccountType type)
    {
        return await _dbSet
            .Where(a => a.AccountType == type)
            .OrderBy(a => a.DisplayOrder)
            .ThenBy(a => a.AccountNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<Account>> GetAccountsBySubTypeAsync(AccountSubType subType)
    {
        return await _dbSet
            .Where(a => a.SubType == subType)
            .OrderBy(a => a.DisplayOrder)
            .ThenBy(a => a.AccountNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<Account>> GetActiveAccountsAsync()
    {
        return await _dbSet
            .Where(a => a.IsActive)
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.DisplayOrder)
            .ThenBy(a => a.AccountNumber)
            .ToListAsync();
    }

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber)
    {
        return await _dbSet
            .Include(a => a.SubAccounts)
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
    }

    public async Task<IEnumerable<Account>> GetSubAccountsAsync(int parentAccountId)
    {
        return await _dbSet
            .Where(a => a.ParentAccountId == parentAccountId)
            .OrderBy(a => a.DisplayOrder)
            .ThenBy(a => a.AccountNumber)
            .ToListAsync();
    }

    public async Task<bool> AccountNumberExistsAsync(string accountNumber, int? excludeId = null)
    {
        var query = _dbSet.Where(a => a.AccountNumber == accountNumber);
        
        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);
            
        return await query.AnyAsync();
    }

    public async Task UpdateBalanceAsync(int accountId, decimal amount, bool isDebit)
    {
        var account = await _dbSet.FindAsync(accountId);
        if (account == null) return;

        // Apply double-entry accounting rules:
        // Assets & Expenses: Debits increase, Credits decrease
        // Liabilities, Equity & Income: Credits increase, Debits decrease
        bool normalDebitBalance = account.AccountType == AccountType.Asset || 
                                   account.AccountType == AccountType.Expense;
        
        if (normalDebitBalance)
        {
            account.Balance += isDebit ? amount : -amount;
        }
        else
        {
            account.Balance += isDebit ? -amount : amount;
        }

        account.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<string> GetNextAccountNumberAsync(AccountType type)
    {
        // Account number ranges by type
        int baseNumber = type switch
        {
            AccountType.Asset => 1000,
            AccountType.Liability => 2000,
            AccountType.Equity => 3000,
            AccountType.Income => 4000,
            AccountType.Expense => 5000,
            _ => 9000
        };

        var maxNumber = await _dbSet
            .Where(a => a.AccountType == type)
            .Select(a => a.AccountNumber)
            .ToListAsync();

        if (!maxNumber.Any())
            return baseNumber.ToString();

        var maxNumeric = maxNumber
            .Where(n => int.TryParse(n, out _))
            .Select(n => int.Parse(n))
            .DefaultIfEmpty(baseNumber - 1)
            .Max();

        return (maxNumeric + 1).ToString();
    }

    public async Task SeedDefaultAccountsAsync()
    {
        // Check if accounts already exist
        if (await _dbSet.AnyAsync())
            return;

        var defaultAccounts = GetDefaultAccounts();
        
        await _dbSet.AddRangeAsync(defaultAccounts);
        await _context.SaveChangesAsync();
    }

    private List<Account> GetDefaultAccounts()
    {
        var now = DateTime.UtcNow;
        
        return new List<Account>
        {
            // ===== ASSET ACCOUNTS (1000s) =====
            new Account
            {
                AccountNumber = "1000",
                Name = "Cash on Hand",
                Description = "Physical cash held by the business",
                AccountType = AccountType.Asset,
                SubType = AccountSubType.Cash,
                IsSystemAccount = true,
                DisplayOrder = 1,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "1010",
                Name = "Checking Account",
                Description = "Primary business checking account",
                AccountType = AccountType.Asset,
                SubType = AccountSubType.Bank,
                IsSystemAccount = true,
                DisplayOrder = 2,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "1020",
                Name = "Savings Account",
                Description = "Business savings account",
                AccountType = AccountType.Asset,
                SubType = AccountSubType.Bank,
                DisplayOrder = 3,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "1100",
                Name = "Accounts Receivable",
                Description = "Money owed by customers",
                AccountType = AccountType.Asset,
                SubType = AccountSubType.AccountsReceivable,
                IsSystemAccount = true,
                DisplayOrder = 10,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "1200",
                Name = "Inventory",
                Description = "Products held for sale",
                AccountType = AccountType.Asset,
                SubType = AccountSubType.Inventory,
                DisplayOrder = 20,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "1300",
                Name = "Prepaid Expenses",
                Description = "Expenses paid in advance",
                AccountType = AccountType.Asset,
                SubType = AccountSubType.OtherCurrentAsset,
                DisplayOrder = 30,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "1500",
                Name = "Equipment",
                Description = "Business equipment and machinery",
                AccountType = AccountType.Asset,
                SubType = AccountSubType.FixedAsset,
                DisplayOrder = 50,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "1510",
                Name = "Accumulated Depreciation - Equipment",
                Description = "Accumulated depreciation on equipment",
                AccountType = AccountType.Asset,
                SubType = AccountSubType.FixedAsset,
                DisplayOrder = 51,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "1600",
                Name = "Vehicles",
                Description = "Business vehicles",
                AccountType = AccountType.Asset,
                SubType = AccountSubType.FixedAsset,
                DisplayOrder = 60,
                CreatedAt = now
            },

            // ===== LIABILITY ACCOUNTS (2000s) =====
            new Account
            {
                AccountNumber = "2000",
                Name = "Accounts Payable",
                Description = "Money owed to vendors",
                AccountType = AccountType.Liability,
                SubType = AccountSubType.AccountsPayable,
                IsSystemAccount = true,
                DisplayOrder = 1,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "2100",
                Name = "Credit Card",
                Description = "Business credit card balance",
                AccountType = AccountType.Liability,
                SubType = AccountSubType.CreditCard,
                DisplayOrder = 10,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "2200",
                Name = "Sales Tax Payable",
                Description = "Sales tax collected but not yet remitted",
                AccountType = AccountType.Liability,
                SubType = AccountSubType.CurrentLiability,
                DisplayOrder = 20,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "2300",
                Name = "Payroll Liabilities",
                Description = "Payroll taxes and withholdings",
                AccountType = AccountType.Liability,
                SubType = AccountSubType.CurrentLiability,
                DisplayOrder = 30,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "2500",
                Name = "Line of Credit",
                Description = "Business line of credit",
                AccountType = AccountType.Liability,
                SubType = AccountSubType.CurrentLiability,
                DisplayOrder = 50,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "2700",
                Name = "Loan Payable",
                Description = "Long-term business loans",
                AccountType = AccountType.Liability,
                SubType = AccountSubType.LongTermLiability,
                DisplayOrder = 70,
                CreatedAt = now
            },

            // ===== EQUITY ACCOUNTS (3000s) =====
            new Account
            {
                AccountNumber = "3000",
                Name = "Owner's Equity",
                Description = "Owner's investment in the business",
                AccountType = AccountType.Equity,
                SubType = AccountSubType.OwnersEquity,
                IsSystemAccount = true,
                DisplayOrder = 1,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "3100",
                Name = "Owner's Draw",
                Description = "Withdrawals by owner",
                AccountType = AccountType.Equity,
                SubType = AccountSubType.OwnersEquity,
                DisplayOrder = 10,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "3200",
                Name = "Retained Earnings",
                Description = "Accumulated profits retained in business",
                AccountType = AccountType.Equity,
                SubType = AccountSubType.RetainedEarnings,
                IsSystemAccount = true,
                DisplayOrder = 20,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "3900",
                Name = "Opening Balance Equity",
                Description = "Used for opening balance entries",
                AccountType = AccountType.Equity,
                SubType = AccountSubType.OpeningBalance,
                IsSystemAccount = true,
                DisplayOrder = 90,
                CreatedAt = now
            },

            // ===== INCOME ACCOUNTS (4000s) =====
            new Account
            {
                AccountNumber = "4000",
                Name = "Sales Revenue",
                Description = "Income from product sales",
                AccountType = AccountType.Income,
                SubType = AccountSubType.Sales,
                IsSystemAccount = true,
                DisplayOrder = 1,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "4100",
                Name = "Service Revenue",
                Description = "Income from services rendered",
                AccountType = AccountType.Income,
                SubType = AccountSubType.ServiceIncome,
                DisplayOrder = 10,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "4200",
                Name = "Discounts Given",
                Description = "Sales discounts provided to customers",
                AccountType = AccountType.Income,
                SubType = AccountSubType.Sales,
                DisplayOrder = 20,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "4300",
                Name = "Returns and Allowances",
                Description = "Product returns and price adjustments",
                AccountType = AccountType.Income,
                SubType = AccountSubType.Sales,
                DisplayOrder = 30,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "4800",
                Name = "Interest Income",
                Description = "Interest earned on bank accounts",
                AccountType = AccountType.Income,
                SubType = AccountSubType.OtherIncome,
                DisplayOrder = 80,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "4900",
                Name = "Other Income",
                Description = "Miscellaneous income",
                AccountType = AccountType.Income,
                SubType = AccountSubType.OtherIncome,
                DisplayOrder = 90,
                CreatedAt = now
            },

            // ===== EXPENSE ACCOUNTS (5000s) =====
            new Account
            {
                AccountNumber = "5000",
                Name = "Cost of Goods Sold",
                Description = "Direct cost of products sold",
                AccountType = AccountType.Expense,
                SubType = AccountSubType.CostOfGoodsSold,
                IsSystemAccount = true,
                DisplayOrder = 1,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "5100",
                Name = "Purchases",
                Description = "Inventory purchases",
                AccountType = AccountType.Expense,
                SubType = AccountSubType.CostOfGoodsSold,
                DisplayOrder = 10,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "6000",
                Name = "Advertising & Marketing",
                Description = "Marketing and promotional expenses",
                AccountType = AccountType.Expense,
                SubType = AccountSubType.OperatingExpense,
                DisplayOrder = 100,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "6100",
                Name = "Bank Charges",
                Description = "Bank fees and charges",
                AccountType = AccountType.Expense,
                SubType = AccountSubType.OperatingExpense,
                DisplayOrder = 110,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "6200",
                Name = "Insurance",
                Description = "Business insurance premiums",
                AccountType = AccountType.Expense,
                SubType = AccountSubType.OperatingExpense,
                DisplayOrder = 120,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "6300",
                Name = "Interest Expense",
                Description = "Interest paid on loans and credit",
                AccountType = AccountType.Expense,
                SubType = AccountSubType.OperatingExpense,
                DisplayOrder = 130,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "6400",
                Name = "Office Supplies",
                Description = "Office supplies and materials",
                AccountType = AccountType.Expense,
                SubType = AccountSubType.OperatingExpense,
                DisplayOrder = 140,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "6500",
                Name = "Professional Fees",
                Description = "Legal, accounting, and consulting fees",
                AccountType = AccountType.Expense,
                SubType = AccountSubType.OperatingExpense,
                DisplayOrder = 150,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "6600",
                Name = "Rent Expense",
                Description = "Office or facility rent",
                AccountType = AccountType.Expense,
                SubType = AccountSubType.OperatingExpense,
                DisplayOrder = 160,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "6700",
                Name = "Repairs & Maintenance",
                Description = "Equipment and facility repairs",
                AccountType = AccountType.Expense,
                SubType = AccountSubType.OperatingExpense,
                DisplayOrder = 170,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "6800",
                Name = "Telephone & Internet",
                Description = "Communication expenses",
                AccountType = AccountType.Expense,
                SubType = AccountSubType.OperatingExpense,
                DisplayOrder = 180,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "6900",
                Name = "Travel & Entertainment",
                Description = "Business travel and entertainment",
                AccountType = AccountType.Expense,
                SubType = AccountSubType.OperatingExpense,
                DisplayOrder = 190,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "7000",
                Name = "Utilities",
                Description = "Electricity, gas, water",
                AccountType = AccountType.Expense,
                SubType = AccountSubType.OperatingExpense,
                DisplayOrder = 200,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "7100",
                Name = "Wages & Salaries",
                Description = "Employee compensation",
                AccountType = AccountType.Expense,
                SubType = AccountSubType.Payroll,
                DisplayOrder = 210,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "7200",
                Name = "Payroll Taxes",
                Description = "Employer payroll tax expenses",
                AccountType = AccountType.Expense,
                SubType = AccountSubType.Payroll,
                DisplayOrder = 220,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "7300",
                Name = "Employee Benefits",
                Description = "Health insurance, retirement contributions",
                AccountType = AccountType.Expense,
                SubType = AccountSubType.Payroll,
                DisplayOrder = 230,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "7500",
                Name = "Depreciation Expense",
                Description = "Depreciation of fixed assets",
                AccountType = AccountType.Expense,
                SubType = AccountSubType.OperatingExpense,
                DisplayOrder = 250,
                CreatedAt = now
            },
            new Account
            {
                AccountNumber = "9000",
                Name = "Miscellaneous Expense",
                Description = "Other business expenses",
                AccountType = AccountType.Expense,
                SubType = AccountSubType.OtherExpense,
                DisplayOrder = 900,
                CreatedAt = now
            }
        };
    }
}
