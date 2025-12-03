using Microsoft.EntityFrameworkCore;
using Kwiktomes.Data;
using Kwiktomes.Data.Models;

namespace Kwiktomes.Services;

/// <summary>
/// Service for aggregating dashboard data.
/// </summary>
public interface IDashboardService
{
    Task<DashboardData> GetDashboardDataAsync();
}

/// <summary>
/// Dashboard data model.
/// </summary>
public class DashboardData
{
    // Summary Stats
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit => TotalRevenue - TotalExpenses;
    public decimal ProfitMargin => TotalRevenue > 0 ? (NetProfit / TotalRevenue) * 100 : 0;
    
    // Accounts Receivable
    public decimal TotalReceivables { get; set; }
    public int OpenInvoiceCount { get; set; }
    public int OverdueInvoiceCount { get; set; }
    public decimal OverdueAmount { get; set; }
    
    // Accounts Payable
    public decimal TotalPayables { get; set; }
    public int OpenBillCount { get; set; }
    public int OverdueBillCount { get; set; }
    public decimal OverduePayablesAmount { get; set; }
    
    // Cash & Bank
    public decimal CashBalance { get; set; }
    public decimal BankBalance { get; set; }
    public decimal TotalCashAndBank => CashBalance + BankBalance;
    
    // Inventory
    public decimal InventoryValue { get; set; }
    public int LowStockCount { get; set; }
    
    // Counts
    public int CustomerCount { get; set; }
    public int VendorCount { get; set; }
    public int ProductCount { get; set; }
    
    // Period comparison (this month vs last month)
    public decimal LastMonthRevenue { get; set; }
    public decimal LastMonthExpenses { get; set; }
    public decimal RevenueChange => LastMonthRevenue > 0 
        ? ((TotalRevenue - LastMonthRevenue) / LastMonthRevenue) * 100 
        : 0;
    public decimal ExpenseChange => LastMonthExpenses > 0 
        ? ((TotalExpenses - LastMonthExpenses) / LastMonthExpenses) * 100 
        : 0;
    
    // Recent activity
    public List<RecentActivity> RecentActivities { get; set; } = new();
    
    // Aging buckets
    public AgingBuckets ReceivablesAging { get; set; } = new();
    public AgingBuckets PayablesAging { get; set; } = new();
    
    // P&amp;L Statement data
    public List<PLCategory> PLIncomeCategories { get; set; } = new();
    public List<PLCategory> PLExpenseCategories { get; set; } = new();
    public decimal PLGrossProfit => PLIncomeCategories.Sum(c => c.Total) - PLDirectExpenses;
    public decimal PLNetProfit => PLGrossProfit - PLIndirectExpenses;
    public decimal PLDirectExpenses => PLExpenseCategories.Where(c => c.IsDirect).Sum(c => c.Total);
    public decimal PLIndirectExpenses => PLExpenseCategories.Where(c => !c.IsDirect).Sum(c => c.Total);
    public decimal PLGrossMargin => PLIncomeCategories.Sum(c => c.Total) > 0 ? (PLGrossProfit / PLIncomeCategories.Sum(c => c.Total)) * 100 : 0;
    
    // Cash flow data
    public decimal CashInflows { get; set; }
    public decimal CashOutflows { get; set; }
    public decimal NetCashFlow => CashInflows - CashOutflows;
    public List<CashFlowItem> CashFlowItems { get; set; } = new();
}

/// <summary>
/// Aging analysis buckets.
/// </summary>
public class AgingBuckets
{
    public decimal Current { get; set; }      // Not yet due
    public decimal Days1To30 { get; set; }    // 1-30 days overdue
    public decimal Days31To60 { get; set; }   // 31-60 days overdue
    public decimal Days61To90 { get; set; }   // 61-90 days overdue
    public decimal Over90Days { get; set; }   // 90+ days overdue
    public decimal Total => Current + Days1To30 + Days31To60 + Days61To90 + Over90Days;
}

/// <summary>
/// Recent activity item.
/// </summary>
public class RecentActivity
{
    public string Type { get; set; } = string.Empty;
    public string Icon { get; set; } = "📄";
    public string Description { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Link { get; set; }
}

/// <summary>
/// P&amp;L category for grouping accounts.
/// </summary>
public class PLCategory
{
    public string Name { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public bool IsDirect { get; set; } // For expenses: direct (COGS) vs indirect (overhead)
    public List<PLAccount> Accounts { get; set; } = new();
}

/// <summary>
/// Individual account in P&amp;L category.
/// </summary>
public class PLAccount
{
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

/// <summary>
/// Cash flow item for categorizing inflows/outflows.
/// </summary>
public class CashFlowItem
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsInflow { get; set; } // true for inflows, false for outflows
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Implementation of the dashboard service.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly KwiktomesDbContext _context;

    public DashboardService(KwiktomesDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardData> GetDashboardDataAsync()
    {
        var today = DateTime.Today;
        var thisMonthStart = new DateTime(today.Year, today.Month, 1);
        var lastMonthStart = thisMonthStart.AddMonths(-1);
        var lastMonthEnd = thisMonthStart.AddDays(-1);

        var data = new DashboardData();

        // Get account balances by type
        var accounts = await _context.Accounts
            .Where(a => a.IsActive)
            .ToListAsync();

        // Cash and Bank balances
        data.CashBalance = accounts
            .Where(a => a.SubType == AccountSubType.Cash)
            .Sum(a => a.Balance);
        
        data.BankBalance = accounts
            .Where(a => a.SubType == AccountSubType.Bank)
            .Sum(a => a.Balance);

        // Receivables and Payables from accounts
        data.TotalReceivables = accounts
            .Where(a => a.SubType == AccountSubType.AccountsReceivable)
            .Sum(a => a.Balance);
        
        data.TotalPayables = accounts
            .Where(a => a.SubType == AccountSubType.AccountsPayable)
            .Sum(a => a.Balance);

        // Revenue and Expenses from accounts
        data.TotalRevenue = accounts
            .Where(a => a.AccountType == AccountType.Income)
            .Sum(a => Math.Abs(a.Balance));
        
        data.TotalExpenses = accounts
            .Where(a => a.AccountType == AccountType.Expense)
            .Sum(a => a.Balance);

        // Counts
        data.CustomerCount = await _context.Customers.CountAsync(c => c.IsActive);
        data.VendorCount = await _context.Vendors.CountAsync(v => v.IsActive);
        data.ProductCount = await _context.Products.CountAsync(p => p.IsActive);

        // Inventory
        var inventoryProducts = await _context.Products
            .Where(p => p.Type == ProductType.Inventory && p.IsActive)
            .ToListAsync();
        
        data.InventoryValue = inventoryProducts.Sum(p => p.QuantityOnHand * p.AverageCost);
        data.LowStockCount = inventoryProducts.Count(p => p.QuantityOnHand <= p.ReorderPoint);

        // Invoice stats - wrapped in try-catch for fresh databases
        try
        {
            var invoices = await _context.Invoices
                .Where(i => i.Status != InvoiceStatus.Void && i.Status != InvoiceStatus.Paid)
                .ToListAsync();
            
            data.OpenInvoiceCount = invoices.Count;
            data.OverdueInvoiceCount = invoices.Count(i => i.IsOverdue);
            data.OverdueAmount = invoices.Where(i => i.IsOverdue).Sum(i => i.BalanceDue);

            // Aging Analysis - Receivables
            data.ReceivablesAging = CalculateAgingBuckets(invoices.Select(i => new AgingItem
            {
                DueDate = i.DueDate ?? i.TransactionDate,
                Amount = i.BalanceDue
            }).ToList());
        }
        catch
        {
            // Tables may not exist yet
        }

        // Bill stats - wrapped in try-catch for fresh databases
        try
        {
            var bills = await _context.Bills
                .Where(b => b.Status != BillStatus.Void && b.Status != BillStatus.Paid)
                .ToListAsync();
            
            data.OpenBillCount = bills.Count;
            data.OverdueBillCount = bills.Count(b => b.IsOverdue);
            data.OverduePayablesAmount = bills.Where(b => b.IsOverdue).Sum(b => b.BalanceDue);

            // Aging Analysis - Payables
            data.PayablesAging = CalculateAgingBuckets(bills.Select(b => new AgingItem
            {
                DueDate = b.DueDate ?? b.TransactionDate,
                Amount = b.BalanceDue
            }).ToList());
        }
        catch
        {
            // Tables may not exist yet
        }

        // Recent activities
        data.RecentActivities = await GetRecentActivitiesAsync();

        // P&L Statement data
        (data.PLIncomeCategories, data.PLExpenseCategories) = await GetPLCategoriesAsync();

        // Cash flow data
        data.CashFlowItems = await GetCashFlowItemsAsync();
        data.CashInflows = data.CashFlowItems.Where(i => i.IsInflow).Sum(i => i.Amount);
        data.CashOutflows = data.CashFlowItems.Where(i => !i.IsInflow).Sum(i => i.Amount);

        return data;
    }

    private AgingBuckets CalculateAgingBuckets(List<AgingItem> items)
    {
        var today = DateTime.Today;
        var buckets = new AgingBuckets();

        foreach (var item in items)
        {
            var daysOverdue = (today - item.DueDate).Days;

            if (daysOverdue <= 0)
                buckets.Current += item.Amount;
            else if (daysOverdue <= 30)
                buckets.Days1To30 += item.Amount;
            else if (daysOverdue <= 60)
                buckets.Days31To60 += item.Amount;
            else if (daysOverdue <= 90)
                buckets.Days61To90 += item.Amount;
            else
                buckets.Over90Days += item.Amount;
        }

        return buckets;
    }

    private async Task<List<RecentActivity>> GetRecentActivitiesAsync()
    {
        var activities = new List<RecentActivity>();
        var cutoffDate = DateTime.Today.AddDays(-30);

        // Recent invoices - try-catch for fresh databases
        try
        {
            var recentInvoices = await _context.Invoices
                .Include(i => i.Customer)
                .Where(i => i.CreatedAt >= cutoffDate)
                .OrderByDescending(i => i.CreatedAt)
                .Take(5)
                .ToListAsync();

            foreach (var inv in recentInvoices)
            {
                activities.Add(new RecentActivity
                {
                    Type = "Invoice",
                    Icon = "📄",
                    Description = $"Invoice {inv.TransactionNumber} to {inv.Customer?.FullName ?? "Customer"}",
                    Amount = inv.Total,
                    Date = inv.CreatedAt,
                    Link = $"/invoices/{inv.Id}"
                });
            }
        }
        catch { /* Table may not exist yet */ }

        // Recent bills - try-catch for fresh databases
        try
        {
            var recentBills = await _context.Bills
                .Include(b => b.Vendor)
                .Where(b => b.CreatedAt >= cutoffDate)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToListAsync();

            foreach (var bill in recentBills)
            {
                activities.Add(new RecentActivity
                {
                    Type = "Bill",
                    Icon = "📋",
                    Description = $"Bill {bill.TransactionNumber} from {bill.Vendor?.FullName ?? "Vendor"}",
                    Amount = bill.Total,
                    Date = bill.CreatedAt,
                    Link = $"/bills/{bill.Id}"
                });
            }
        }
        catch { /* Table may not exist yet */ }

        // Recent payments - try-catch for fresh databases
        try
        {
            var recentPayments = await _context.Payments
                .Include(p => p.Customer)
                .Include(p => p.Vendor)
                .Where(p => p.CreatedAt >= cutoffDate)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            foreach (var pmt in recentPayments)
            {
                var name = pmt.Direction == PaymentDirection.Received 
                    ? pmt.Customer?.FullName ?? "Customer"
                    : pmt.Vendor?.FullName ?? "Vendor";
                
                activities.Add(new RecentActivity
                {
                    Type = pmt.Direction == PaymentDirection.Received ? "Payment Received" : "Payment Made",
                    Icon = pmt.Direction == PaymentDirection.Received ? "💵" : "💸",
                    Description = $"{(pmt.Direction == PaymentDirection.Received ? "Payment from" : "Payment to")} {name}",
                    Amount = pmt.Total,
                    Date = pmt.CreatedAt,
                    Link = $"/payments/{pmt.Id}"
                });
            }
        }
        catch { /* Table may not exist yet */ }

        // Recent journal entries
        try
        {
            var recentJournalEntries = await _context.JournalEntries
                .Where(j => j.CreatedAt >= cutoffDate && j.Status == TransactionStatus.Posted)
                .OrderByDescending(j => j.CreatedAt)
                .Take(3)
                .ToListAsync();

            foreach (var je in recentJournalEntries)
            {
                activities.Add(new RecentActivity
                {
                    Type = "Journal Entry",
                    Icon = "📝",
                    Description = $"{je.EntryNumber}: {je.Memo ?? "Journal Entry"}",
                    Amount = je.Lines?.Sum(l => l.DebitAmount) ?? 0,
                    Date = je.CreatedAt,
                    Link = $"/journal-entries/{je.Id}"
                });
            }
        }
        catch { /* Table may not exist yet */ }

        return activities.OrderByDescending(a => a.Date).Take(10).ToList();
    }

    private async Task<(List<PLCategory> income, List<PLCategory> expenses)> GetPLCategoriesAsync()
    {
        var incomeCategories = new List<PLCategory>();
        var expenseCategories = new List<PLCategory>();

        // Get all active income and expense accounts
        var incomeAccounts = await _context.Accounts
            .Where(a => a.IsActive && a.AccountType == AccountType.Income)
            .OrderBy(a => a.AccountNumber)
            .ToListAsync();

        var expenseAccounts = await _context.Accounts
            .Where(a => a.IsActive && a.AccountType == AccountType.Expense)
            .OrderBy(a => a.AccountNumber)
            .ToListAsync();

        // Group income accounts by category (using the first part of account number or name)
        var incomeGroups = incomeAccounts.GroupBy(a => GetAccountCategory(a));
        
        foreach (var group in incomeGroups)
        {
            var category = new PLCategory
            {
                Name = group.Key,
                IsDirect = false, // Income is typically not direct/indirect
                Accounts = group.Select(a => new PLAccount
                {
                    AccountName = a.Name,
                    AccountNumber = a.AccountNumber,
                    Balance = Math.Abs(a.Balance) // Income accounts show positive balances
                }).ToList()
            };
            category.Total = category.Accounts.Sum(acc => acc.Balance);
            incomeCategories.Add(category);
        }

        // Group expense accounts by category and determine if direct/indirect
        var expenseGroups = expenseAccounts.GroupBy(a => GetAccountCategory(a));
        
        foreach (var group in expenseGroups)
        {
            var category = new PLCategory
            {
                Name = group.Key,
                IsDirect = IsDirectExpenseCategory(group.Key),
                Accounts = group.Select(a => new PLAccount
                {
                    AccountName = a.Name,
                    AccountNumber = a.AccountNumber,
                    Balance = a.Balance // Expense accounts show negative balances, but we'll display as positive
                }).ToList()
            };
            category.Total = category.Accounts.Sum(acc => Math.Abs(acc.Balance));
            expenseCategories.Add(category);
        }

        return (incomeCategories.OrderByDescending(c => c.Total).ToList(), 
                expenseCategories.OrderByDescending(c => c.Total).ToList());
    }

    private async Task<List<CashFlowItem>> GetCashFlowItemsAsync()
    {
        var cashFlowItems = new List<CashFlowItem>();
        var cutoffDate = DateTime.Today.AddDays(-30); // Last 30 days

        // Get all journal entries that affect cash/bank accounts
        var cashJournalEntries = await _context.JournalEntryLines
            .Include(jel => jel.JournalEntry)
            .Include(jel => jel.Account)
            .Where(jel => jel.Account.SubType == AccountSubType.Cash || jel.Account.SubType == AccountSubType.Bank)
            .Where(jel => jel.JournalEntry.CreatedAt >= cutoffDate && jel.JournalEntry.Status == TransactionStatus.Posted)
            .OrderByDescending(jel => jel.JournalEntry.CreatedAt)
            .Take(20) // Limit to recent entries
            .ToListAsync();

        foreach (var cashLine in cashJournalEntries)
        {
            // For each cash line, find offsetting entries in the same journal entry
            var offsettingLines = await _context.JournalEntryLines
                .Include(jel => jel.Account)
                .Where(jel => jel.JournalEntryId == cashLine.JournalEntryId && jel.Id != cashLine.Id)
                .ToListAsync();

            foreach (var offsetLine in offsettingLines)
            {
                var category = CategorizeCashFlow(offsetLine.Account);
                var amount = Math.Abs(cashLine.DebitAmount + cashLine.CreditAmount);
                var isInflow = cashLine.DebitAmount > 0; // Debit to cash = inflow

                cashFlowItems.Add(new CashFlowItem
                {
                    Category = category,
                    Amount = amount,
                    IsInflow = isInflow,
                    Description = cashLine.JournalEntry.Memo ?? $"{offsetLine.Account.Name} transaction"
                });
            }
        }

        // Group by category and sum amounts
        var groupedItems = cashFlowItems
            .GroupBy(i => new { i.Category, i.IsInflow })
            .Select(g => new CashFlowItem
            {
                Category = g.Key.Category,
                Amount = g.Sum(i => i.Amount),
                IsInflow = g.Key.IsInflow,
                Description = g.First().Description
            })
            .OrderByDescending(i => i.Amount)
            .ToList();

        return groupedItems;
    }

    private string CategorizeCashFlow(Account account)
    {
        // Categorize based on account type
        if (account.AccountType == AccountType.Income)
            return "Sales Revenue";
        else if (account.AccountType == AccountType.Expense)
        {
            if (account.Name.Contains("Salary", StringComparison.OrdinalIgnoreCase) ||
                account.Name.Contains("Wage", StringComparison.OrdinalIgnoreCase))
                return "Payroll";
            else if (account.Name.Contains("Rent", StringComparison.OrdinalIgnoreCase))
                return "Rent & Utilities";
            else if (account.Name.Contains("Marketing", StringComparison.OrdinalIgnoreCase) ||
                     account.Name.Contains("Advertising", StringComparison.OrdinalIgnoreCase))
                return "Marketing";
            else if (account.Name.Contains("Office", StringComparison.OrdinalIgnoreCase) ||
                     account.Name.Contains("Supplies", StringComparison.OrdinalIgnoreCase))
                return "Office Expenses";
            else
                return "Operating Expenses";
        }
        else if (account.SubType == AccountSubType.AccountsReceivable)
            return "Customer Payments";
        else if (account.SubType == AccountSubType.AccountsPayable)
            return "Vendor Payments";
        else
            return "Other";
    }

    private string GetAccountCategory(Account account)
    {
        // Simple categorization based on account number prefix or keywords
        if (account.AccountNumber.StartsWith("4"))
            return "Sales Revenue";
        else if (account.AccountNumber.StartsWith("5"))
            return "Other Income";
        else if (account.Name.Contains("Cost of Goods", StringComparison.OrdinalIgnoreCase) || 
                 account.Name.Contains("COGS", StringComparison.OrdinalIgnoreCase))
            return "Cost of Goods Sold";
        else if (account.Name.Contains("Salary", StringComparison.OrdinalIgnoreCase) ||
                 account.Name.Contains("Wage", StringComparison.OrdinalIgnoreCase))
            return "Salaries & Wages";
        else if (account.Name.Contains("Rent", StringComparison.OrdinalIgnoreCase))
            return "Rent & Utilities";
        else if (account.Name.Contains("Marketing", StringComparison.OrdinalIgnoreCase) ||
                 account.Name.Contains("Advertising", StringComparison.OrdinalIgnoreCase))
            return "Marketing & Advertising";
        else if (account.Name.Contains("Office", StringComparison.OrdinalIgnoreCase) ||
                 account.Name.Contains("Supplies", StringComparison.OrdinalIgnoreCase))
            return "Office & Supplies";
        else if (account.Name.Contains("Insurance", StringComparison.OrdinalIgnoreCase))
            return "Insurance";
        else if (account.Name.Contains("Tax", StringComparison.OrdinalIgnoreCase))
            return "Taxes & Licenses";
        else if (account.Name.Contains("Interest", StringComparison.OrdinalIgnoreCase))
            return "Interest Expense";
        else if (account.Name.Contains("Depreciation", StringComparison.OrdinalIgnoreCase))
            return "Depreciation";
        else
            return "Other Expenses";
    }

    private bool IsDirectExpenseCategory(string categoryName)
    {
        // Cost of Goods Sold is considered direct expense
        return categoryName.Contains("Cost of Goods Sold", StringComparison.OrdinalIgnoreCase);
    }

    private class AgingItem
    {
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
    }
}
