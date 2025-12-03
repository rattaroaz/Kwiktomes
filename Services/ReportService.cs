using Microsoft.EntityFrameworkCore;
using Kwiktomes.Data;
using Kwiktomes.Data.Models;

namespace Kwiktomes.Services;

/// <summary>
/// Service for generating financial reports.
/// </summary>
public interface IReportService
{
    // Financial Statements
    Task<ProfitLossReport> GetProfitLossAsync(DateTime startDate, DateTime endDate);
    Task<BalanceSheetReport> GetBalanceSheetAsync(DateTime asOfDate);
    Task<TrialBalanceReport> GetTrialBalanceAsync(DateTime asOfDate);
    Task<CashFlowReport> GetCashFlowAsync(DateTime startDate, DateTime endDate);
    
    // Ledger Reports
    Task<GeneralLedgerReport> GetGeneralLedgerAsync(DateTime startDate, DateTime endDate, int? accountId = null);
    
    // Aging Reports
    Task<AgingReport> GetReceivablesAgingAsync(DateTime asOfDate);
    Task<AgingReport> GetPayablesAgingAsync(DateTime asOfDate);
    
    // Transaction Reports
    Task<SalesReport> GetSalesReportAsync(DateTime startDate, DateTime endDate);
    Task<ExpenseReport> GetExpenseReportAsync(DateTime startDate, DateTime endDate);
    Task<TaxSummaryReport> GetTaxSummaryAsync(DateTime startDate, DateTime endDate);
    
    // Statement Reports
    Task<CustomerStatementReport> GetCustomerStatementAsync(int customerId, DateTime startDate, DateTime endDate);
    Task<VendorStatementReport> GetVendorStatementAsync(int vendorId, DateTime startDate, DateTime endDate);
}

#region Report Models

/// <summary>
/// Profit and Loss (Income Statement) report.
/// </summary>
public class ProfitLossReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    
    public List<PLSection> IncomeSections { get; set; } = new();
    public List<PLSection> ExpenseSections { get; set; } = new();
    
    public decimal TotalIncome => IncomeSections.Sum(s => s.Total);
    public decimal TotalCOGS => ExpenseSections.Where(s => s.IsCOGS).Sum(s => s.Total);
    public decimal GrossProfit => TotalIncome - TotalCOGS;
    public decimal TotalOperatingExpenses => ExpenseSections.Where(s => !s.IsCOGS).Sum(s => s.Total);
    public decimal NetIncome => GrossProfit - TotalOperatingExpenses;
    public decimal GrossMarginPercent => TotalIncome > 0 ? (GrossProfit / TotalIncome) * 100 : 0;
    public decimal NetMarginPercent => TotalIncome > 0 ? (NetIncome / TotalIncome) * 100 : 0;
}

public class PLSection
{
    public string Name { get; set; } = string.Empty;
    public bool IsCOGS { get; set; }
    public List<PLLineItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Amount);
}

public class PLLineItem
{
    public int AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

/// <summary>
/// Balance Sheet report.
/// </summary>
public class BalanceSheetReport
{
    public DateTime AsOfDate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    
    public List<BSSection> AssetSections { get; set; } = new();
    public List<BSSection> LiabilitySections { get; set; } = new();
    public List<BSSection> EquitySections { get; set; } = new();
    
    public decimal TotalAssets => AssetSections.Sum(s => s.Total);
    public decimal TotalLiabilities => LiabilitySections.Sum(s => s.Total);
    public decimal TotalEquity => EquitySections.Sum(s => s.Total);
    public decimal LiabilitiesAndEquity => TotalLiabilities + TotalEquity;
    public bool IsBalanced => Math.Abs(TotalAssets - LiabilitiesAndEquity) < 0.01m;
}

public class BSSection
{
    public string Name { get; set; } = string.Empty;
    public List<BSLineItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Balance);
}

public class BSLineItem
{
    public int AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

/// <summary>
/// Trial Balance report.
/// </summary>
public class TrialBalanceReport
{
    public DateTime AsOfDate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    
    public List<TrialBalanceItem> Items { get; set; } = new();
    
    public decimal TotalDebits => Items.Sum(i => i.Debit);
    public decimal TotalCredits => Items.Sum(i => i.Credit);
    public bool IsBalanced => Math.Abs(TotalDebits - TotalCredits) < 0.01m;
}

public class TrialBalanceItem
{
    public int AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

/// <summary>
/// Cash Flow Statement report.
/// </summary>
public class CashFlowReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    
    public decimal BeginningCash { get; set; }
    public List<CashFlowSection> OperatingActivities { get; set; } = new();
    public List<CashFlowSection> InvestingActivities { get; set; } = new();
    public List<CashFlowSection> FinancingActivities { get; set; } = new();
    
    public decimal NetOperatingCashFlow => OperatingActivities.Sum(s => s.Total);
    public decimal NetInvestingCashFlow => InvestingActivities.Sum(s => s.Total);
    public decimal NetFinancingCashFlow => FinancingActivities.Sum(s => s.Total);
    public decimal NetCashChange => NetOperatingCashFlow + NetInvestingCashFlow + NetFinancingCashFlow;
    public decimal EndingCash => BeginningCash + NetCashChange;
}

public class CashFlowSection
{
    public string Name { get; set; } = string.Empty;
    public List<CashFlowLineItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Amount);
}

public class CashFlowLineItem
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

/// <summary>
/// General Ledger report.
/// </summary>
public class GeneralLedgerReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    
    public List<GLAccountSection> Accounts { get; set; } = new();
}

public class GLAccountSection
{
    public int AccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public decimal OpeningBalance { get; set; }
    public List<GLTransaction> Transactions { get; set; } = new();
    public decimal ClosingBalance { get; set; }
    public decimal TotalDebits => Transactions.Sum(t => t.Debit);
    public decimal TotalCredits => Transactions.Sum(t => t.Credit);
}

public class GLTransaction
{
    public DateTime Date { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
}

/// <summary>
/// Aging report for AR/AP.
/// </summary>
public class AgingReport
{
    public DateTime AsOfDate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public bool IsReceivables { get; set; }
    
    public List<AgingEntity> Entities { get; set; } = new();
    
    public decimal TotalCurrent => Entities.Sum(e => e.Current);
    public decimal Total1To30 => Entities.Sum(e => e.Days1To30);
    public decimal Total31To60 => Entities.Sum(e => e.Days31To60);
    public decimal Total61To90 => Entities.Sum(e => e.Days61To90);
    public decimal TotalOver90 => Entities.Sum(e => e.Over90Days);
    public decimal GrandTotal => Entities.Sum(e => e.Total);
}

public class AgingEntity
{
    public int EntityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Current { get; set; }
    public decimal Days1To30 { get; set; }
    public decimal Days31To60 { get; set; }
    public decimal Days61To90 { get; set; }
    public decimal Over90Days { get; set; }
    public decimal Total => Current + Days1To30 + Days31To60 + Days61To90 + Over90Days;
    public List<AgingInvoice> Invoices { get; set; } = new();
}

public class AgingInvoice
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public int DaysOverdue { get; set; }
}

/// <summary>
/// Sales report.
/// </summary>
public class SalesReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    
    public decimal TotalSales { get; set; }
    public decimal TotalTax { get; set; }
    public int InvoiceCount { get; set; }
    public int CustomerCount { get; set; }
    
    public List<SalesByCustomer> ByCustomer { get; set; } = new();
    public List<SalesByProduct> ByProduct { get; set; } = new();
    public List<SalesByMonth> ByMonth { get; set; } = new();
}

public class SalesByCustomer
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int InvoiceCount { get; set; }
    public decimal Percentage { get; set; }
}

public class SalesByProduct
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class SalesByMonth
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int InvoiceCount { get; set; }
}

/// <summary>
/// Expense report.
/// </summary>
public class ExpenseReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    
    public decimal TotalExpenses { get; set; }
    public int BillCount { get; set; }
    public int VendorCount { get; set; }
    
    public List<ExpenseByVendor> ByVendor { get; set; } = new();
    public List<ExpenseByCategory> ByCategory { get; set; } = new();
    public List<ExpenseByMonth> ByMonth { get; set; } = new();
}

public class ExpenseByVendor
{
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int BillCount { get; set; }
    public decimal Percentage { get; set; }
}

public class ExpenseByCategory
{
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class ExpenseByMonth
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int BillCount { get; set; }
}

/// <summary>
/// Tax summary report.
/// </summary>
public class TaxSummaryReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    
    public decimal TaxableIncome { get; set; }
    public decimal TaxCollected { get; set; }
    public decimal TaxPaid { get; set; }
    public decimal NetTaxLiability => TaxCollected - TaxPaid;
    
    public List<TaxLineItem> TaxCollectedItems { get; set; } = new();
    public List<TaxLineItem> TaxPaidItems { get; set; } = new();
}

public class TaxLineItem
{
    public string Description { get; set; } = string.Empty;
    public string TaxType { get; set; } = string.Empty;
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
}

/// <summary>
/// Customer statement report.
/// </summary>
public class CustomerStatementReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? CustomerAddress { get; set; }
    
    public decimal OpeningBalance { get; set; }
    public List<StatementTransaction> Transactions { get; set; } = new();
    public decimal ClosingBalance { get; set; }
    
    public decimal TotalCharges => Transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
    public decimal TotalPayments => Transactions.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));
}

/// <summary>
/// Vendor statement report.
/// </summary>
public class VendorStatementReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string? VendorEmail { get; set; }
    public string? VendorAddress { get; set; }
    
    public decimal OpeningBalance { get; set; }
    public List<StatementTransaction> Transactions { get; set; } = new();
    public decimal ClosingBalance { get; set; }
    
    public decimal TotalCharges => Transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
    public decimal TotalPayments => Transactions.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));
}

public class StatementTransaction
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
}

#endregion

/// <summary>
/// Implementation of the report service.
/// </summary>
public class ReportService : IReportService
{
    private readonly KwiktomesDbContext _context;

    public ReportService(KwiktomesDbContext context)
    {
        _context = context;
    }

    #region Financial Statements

    public async Task<ProfitLossReport> GetProfitLossAsync(DateTime startDate, DateTime endDate)
    {
        var report = new ProfitLossReport
        {
            StartDate = startDate,
            EndDate = endDate
        };

        // Get income accounts
        var incomeAccounts = await _context.Accounts
            .Where(a => a.AccountType == AccountType.Income && a.IsActive)
            .OrderBy(a => a.SubType)
            .ThenBy(a => a.AccountNumber)
            .ToListAsync();

        // Group by subtype
        var incomeBySubType = incomeAccounts.GroupBy(a => a.SubType);
        foreach (var group in incomeBySubType)
        {
            var section = new PLSection
            {
                Name = FormatSubType(group.Key),
                Items = group.Select(a => new PLLineItem
                {
                    AccountId = a.Id,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.Name,
                    Amount = a.Balance
                }).ToList()
            };
            report.IncomeSections.Add(section);
        }

        // Get expense accounts
        var expenseAccounts = await _context.Accounts
            .Where(a => a.AccountType == AccountType.Expense && a.IsActive)
            .OrderBy(a => a.SubType)
            .ThenBy(a => a.AccountNumber)
            .ToListAsync();

        var expenseBySubType = expenseAccounts.GroupBy(a => a.SubType);
        foreach (var group in expenseBySubType)
        {
            var section = new PLSection
            {
                Name = FormatSubType(group.Key),
                IsCOGS = group.Key == AccountSubType.CostOfGoodsSold,
                Items = group.Select(a => new PLLineItem
                {
                    AccountId = a.Id,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.Name,
                    Amount = a.Balance
                }).ToList()
            };
            report.ExpenseSections.Add(section);
        }

        return report;
    }

    public async Task<BalanceSheetReport> GetBalanceSheetAsync(DateTime asOfDate)
    {
        var report = new BalanceSheetReport { AsOfDate = asOfDate };

        // Assets
        var assetAccounts = await _context.Accounts
            .Where(a => a.AccountType == AccountType.Asset && a.IsActive)
            .OrderBy(a => a.SubType)
            .ThenBy(a => a.AccountNumber)
            .ToListAsync();

        var assetsBySubType = assetAccounts.GroupBy(a => a.SubType);
        foreach (var group in assetsBySubType)
        {
            var section = new BSSection
            {
                Name = FormatSubType(group.Key),
                Items = group.Select(a => new BSLineItem
                {
                    AccountId = a.Id,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.Name,
                    Balance = a.Balance
                }).ToList()
            };
            report.AssetSections.Add(section);
        }

        // Liabilities
        var liabilityAccounts = await _context.Accounts
            .Where(a => a.AccountType == AccountType.Liability && a.IsActive)
            .OrderBy(a => a.SubType)
            .ThenBy(a => a.AccountNumber)
            .ToListAsync();

        var liabilitiesBySubType = liabilityAccounts.GroupBy(a => a.SubType);
        foreach (var group in liabilitiesBySubType)
        {
            var section = new BSSection
            {
                Name = FormatSubType(group.Key),
                Items = group.Select(a => new BSLineItem
                {
                    AccountId = a.Id,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.Name,
                    Balance = a.Balance
                }).ToList()
            };
            report.LiabilitySections.Add(section);
        }

        // Equity
        var equityAccounts = await _context.Accounts
            .Where(a => a.AccountType == AccountType.Equity && a.IsActive)
            .OrderBy(a => a.SubType)
            .ThenBy(a => a.AccountNumber)
            .ToListAsync();

        var equityBySubType = equityAccounts.GroupBy(a => a.SubType);
        foreach (var group in equityBySubType)
        {
            var section = new BSSection
            {
                Name = FormatSubType(group.Key),
                Items = group.Select(a => new BSLineItem
                {
                    AccountId = a.Id,
                    AccountNumber = a.AccountNumber,
                    AccountName = a.Name,
                    Balance = a.Balance
                }).ToList()
            };
            report.EquitySections.Add(section);
        }

        // Add retained earnings (net income from P&L)
        var netIncome = await CalculateNetIncomeAsync();
        if (Math.Abs(netIncome) > 0.01m)
        {
            var retainedEarningsSection = report.EquitySections.FirstOrDefault(s => s.Name.Contains("Retained"));
            if (retainedEarningsSection == null)
            {
                retainedEarningsSection = new BSSection { Name = "Retained Earnings" };
                report.EquitySections.Add(retainedEarningsSection);
            }
            retainedEarningsSection.Items.Add(new BSLineItem
            {
                AccountName = "Net Income (Current Period)",
                Balance = netIncome
            });
        }

        return report;
    }

    public async Task<TrialBalanceReport> GetTrialBalanceAsync(DateTime asOfDate)
    {
        var report = new TrialBalanceReport { AsOfDate = asOfDate };

        var accounts = await _context.Accounts
            .Where(a => a.IsActive && a.Balance != 0)
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.AccountNumber)
            .ToListAsync();

        foreach (var account in accounts)
        {
            var item = new TrialBalanceItem
            {
                AccountId = account.Id,
                AccountNumber = account.AccountNumber,
                AccountName = account.Name,
                AccountType = account.AccountType
            };

            // Debit balances: Assets, Expenses
            // Credit balances: Liabilities, Equity, Income
            bool isDebitNormal = account.AccountType == AccountType.Asset || 
                                 account.AccountType == AccountType.Expense;

            if (account.Balance >= 0)
            {
                if (isDebitNormal)
                    item.Debit = account.Balance;
                else
                    item.Credit = account.Balance;
            }
            else
            {
                if (isDebitNormal)
                    item.Credit = Math.Abs(account.Balance);
                else
                    item.Debit = Math.Abs(account.Balance);
            }

            report.Items.Add(item);
        }

        return report;
    }

    public async Task<CashFlowReport> GetCashFlowAsync(DateTime startDate, DateTime endDate)
    {
        var report = new CashFlowReport
        {
            StartDate = startDate,
            EndDate = endDate
        };

        // Get cash accounts
        var cashAccounts = await _context.Accounts
            .Where(a => a.SubType == AccountSubType.Cash || a.SubType == AccountSubType.Bank)
            .ToListAsync();

        report.BeginningCash = cashAccounts.Sum(a => a.Balance); // Simplified

        // Operating Activities
        var operatingSection = new CashFlowSection { Name = "Operating Activities" };
        
        // Net income
        var netIncome = await CalculateNetIncomeAsync();
        operatingSection.Items.Add(new CashFlowLineItem
        {
            Description = "Net Income",
            Amount = netIncome
        });

        // Changes in AR
        var arChange = await GetAccountBalanceChangeAsync(AccountSubType.AccountsReceivable, startDate, endDate);
        operatingSection.Items.Add(new CashFlowLineItem
        {
            Description = "Decrease (Increase) in Accounts Receivable",
            Amount = -arChange
        });

        // Changes in AP
        var apChange = await GetAccountBalanceChangeAsync(AccountSubType.AccountsPayable, startDate, endDate);
        operatingSection.Items.Add(new CashFlowLineItem
        {
            Description = "Increase (Decrease) in Accounts Payable",
            Amount = apChange
        });

        report.OperatingActivities.Add(operatingSection);

        // Investing Activities (simplified)
        var investingSection = new CashFlowSection { Name = "Investing Activities" };
        var fixedAssetChange = await GetAccountBalanceChangeAsync(AccountSubType.FixedAsset, startDate, endDate);
        if (Math.Abs(fixedAssetChange) > 0.01m)
        {
            investingSection.Items.Add(new CashFlowLineItem
            {
                Description = "Purchase of Fixed Assets",
                Amount = -fixedAssetChange
            });
        }
        report.InvestingActivities.Add(investingSection);

        // Financing Activities (simplified)
        var financingSection = new CashFlowSection { Name = "Financing Activities" };
        var equityChange = await GetEquityChangeAsync(startDate, endDate);
        if (Math.Abs(equityChange) > 0.01m)
        {
            financingSection.Items.Add(new CashFlowLineItem
            {
                Description = "Owner Contributions/Distributions",
                Amount = equityChange
            });
        }
        report.FinancingActivities.Add(financingSection);

        return report;
    }

    #endregion

    #region Ledger Reports

    public async Task<GeneralLedgerReport> GetGeneralLedgerAsync(DateTime startDate, DateTime endDate, int? accountId = null)
    {
        var report = new GeneralLedgerReport
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var accountsQuery = _context.Accounts
            .Where(a => a.IsActive);

        if (accountId.HasValue)
            accountsQuery = accountsQuery.Where(a => a.Id == accountId.Value);

        var accounts = await accountsQuery
            .OrderBy(a => a.AccountType)
            .ThenBy(a => a.AccountNumber)
            .ToListAsync();

        foreach (var account in accounts)
        {
            var section = new GLAccountSection
            {
                AccountId = account.Id,
                AccountNumber = account.AccountNumber,
                AccountName = account.Name,
                AccountType = account.AccountType,
                OpeningBalance = account.Balance // Simplified - would need historical calculation
            };

            // Get journal entry lines for this account
            var journalLines = await _context.JournalEntryLines
                .Include(l => l.JournalEntry)
                .Where(l => l.AccountId == account.Id && 
                           l.JournalEntry.EntryDate >= startDate && 
                           l.JournalEntry.EntryDate <= endDate &&
                           l.JournalEntry.Status == TransactionStatus.Posted)
                .OrderBy(l => l.JournalEntry.EntryDate)
                .ThenBy(l => l.JournalEntry.EntryNumber)
                .ToListAsync();

            decimal runningBalance = section.OpeningBalance;
            foreach (var line in journalLines)
            {
                bool isDebitNormal = account.AccountType == AccountType.Asset || 
                                     account.AccountType == AccountType.Expense;
                
                if (isDebitNormal)
                    runningBalance += line.DebitAmount - line.CreditAmount;
                else
                    runningBalance += line.CreditAmount - line.DebitAmount;

                section.Transactions.Add(new GLTransaction
                {
                    Date = line.JournalEntry.EntryDate,
                    Reference = line.JournalEntry.EntryNumber,
                    Description = line.Description ?? line.JournalEntry.Memo ?? "",
                    Debit = line.DebitAmount,
                    Credit = line.CreditAmount,
                    Balance = runningBalance
                });
            }

            section.ClosingBalance = runningBalance;
            report.Accounts.Add(section);
        }

        return report;
    }

    #endregion

    #region Aging Reports

    public async Task<AgingReport> GetReceivablesAgingAsync(DateTime asOfDate)
    {
        var report = new AgingReport
        {
            AsOfDate = asOfDate,
            IsReceivables = true
        };

        var openInvoices = await _context.Invoices
            .Include(i => i.Customer)
            .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Void)
            .ToListAsync();

        var customerGroups = openInvoices.GroupBy(i => i.CustomerId);

        foreach (var group in customerGroups)
        {
            var customer = group.First().Customer;
            var entity = new AgingEntity
            {
                EntityId = group.Key,
                Name = customer?.Name ?? "Unknown"
            };

            foreach (var invoice in group)
            {
                var dueDate = invoice.DueDate ?? invoice.TransactionDate;
                var daysOverdue = (asOfDate - dueDate).Days;
                var balance = invoice.BalanceDue;

                var agingInvoice = new AgingInvoice
                {
                    Id = invoice.Id,
                    Number = invoice.TransactionNumber,
                    Date = invoice.TransactionDate,
                    DueDate = dueDate,
                    Amount = invoice.Total,
                    Balance = balance,
                    DaysOverdue = Math.Max(0, daysOverdue)
                };
                entity.Invoices.Add(agingInvoice);

                if (daysOverdue <= 0)
                    entity.Current += balance;
                else if (daysOverdue <= 30)
                    entity.Days1To30 += balance;
                else if (daysOverdue <= 60)
                    entity.Days31To60 += balance;
                else if (daysOverdue <= 90)
                    entity.Days61To90 += balance;
                else
                    entity.Over90Days += balance;
            }

            report.Entities.Add(entity);
        }

        return report;
    }

    public async Task<AgingReport> GetPayablesAgingAsync(DateTime asOfDate)
    {
        var report = new AgingReport
        {
            AsOfDate = asOfDate,
            IsReceivables = false
        };

        var openBills = await _context.Bills
            .Include(b => b.Vendor)
            .Where(b => b.Status != BillStatus.Paid && b.Status != BillStatus.Void)
            .ToListAsync();

        var vendorGroups = openBills.GroupBy(b => b.VendorId);

        foreach (var group in vendorGroups)
        {
            var vendor = group.First().Vendor;
            var entity = new AgingEntity
            {
                EntityId = group.Key,
                Name = vendor?.Name ?? "Unknown"
            };

            foreach (var bill in group)
            {
                var dueDate = bill.DueDate ?? bill.TransactionDate;
                var daysOverdue = (asOfDate - dueDate).Days;
                var balance = bill.BalanceDue;

                var agingInvoice = new AgingInvoice
                {
                    Id = bill.Id,
                    Number = bill.TransactionNumber,
                    Date = bill.TransactionDate,
                    DueDate = dueDate,
                    Amount = bill.Total,
                    Balance = balance,
                    DaysOverdue = Math.Max(0, daysOverdue)
                };
                entity.Invoices.Add(agingInvoice);

                if (daysOverdue <= 0)
                    entity.Current += balance;
                else if (daysOverdue <= 30)
                    entity.Days1To30 += balance;
                else if (daysOverdue <= 60)
                    entity.Days31To60 += balance;
                else if (daysOverdue <= 90)
                    entity.Days61To90 += balance;
                else
                    entity.Over90Days += balance;
            }

            report.Entities.Add(entity);
        }

        return report;
    }

    #endregion

    #region Transaction Reports

    public async Task<SalesReport> GetSalesReportAsync(DateTime startDate, DateTime endDate)
    {
        var report = new SalesReport
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var invoices = await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Lines)
                .ThenInclude(l => l.Product)
            .Where(i => i.TransactionDate >= startDate && i.TransactionDate <= endDate &&
                       i.Status != InvoiceStatus.Void)
            .ToListAsync();

        report.TotalSales = invoices.Sum(i => i.Subtotal);
        report.TotalTax = invoices.Sum(i => i.TaxAmount);
        report.InvoiceCount = invoices.Count;
        report.CustomerCount = invoices.Select(i => i.CustomerId).Distinct().Count();

        // By Customer
        var byCustomer = invoices
            .GroupBy(i => new { i.CustomerId, CustomerName = i.Customer?.Name ?? "Unknown" })
            .Select(g => new SalesByCustomer
            {
                CustomerId = g.Key.CustomerId,
                CustomerName = g.Key.CustomerName,
                Amount = g.Sum(i => i.Subtotal),
                InvoiceCount = g.Count(),
                Percentage = report.TotalSales > 0 ? (g.Sum(i => i.Subtotal) / report.TotalSales) * 100 : 0
            })
            .OrderByDescending(c => c.Amount)
            .ToList();
        report.ByCustomer = byCustomer;

        // By Product
        var allLines = invoices.SelectMany(i => i.Lines).ToList();
        var byProduct = allLines
            .Where(l => l.ProductId.HasValue)
            .GroupBy(l => new { l.ProductId, ProductName = l.Product?.Name ?? "Unknown" })
            .Select(g => new SalesByProduct
            {
                ProductId = g.Key.ProductId ?? 0,
                ProductName = g.Key.ProductName,
                Quantity = g.Sum(l => l.Quantity),
                Amount = g.Sum(l => l.LineTotal),
                Percentage = report.TotalSales > 0 ? (g.Sum(l => l.LineTotal) / report.TotalSales) * 100 : 0
            })
            .OrderByDescending(p => p.Amount)
            .ToList();
        report.ByProduct = byProduct;

        // By Month
        var byMonth = invoices
            .GroupBy(i => new { i.TransactionDate.Year, i.TransactionDate.Month })
            .Select(g => new SalesByMonth
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                Amount = g.Sum(i => i.Subtotal),
                InvoiceCount = g.Count()
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();
        report.ByMonth = byMonth;

        return report;
    }

    public async Task<ExpenseReport> GetExpenseReportAsync(DateTime startDate, DateTime endDate)
    {
        var report = new ExpenseReport
        {
            StartDate = startDate,
            EndDate = endDate
        };

        var bills = await _context.Bills
            .Include(b => b.Vendor)
            .Include(b => b.Lines)
                .ThenInclude(l => l.Account)
            .Where(b => b.TransactionDate >= startDate && b.TransactionDate <= endDate &&
                       b.Status != BillStatus.Void)
            .ToListAsync();

        report.TotalExpenses = bills.Sum(b => b.Subtotal);
        report.BillCount = bills.Count;
        report.VendorCount = bills.Select(b => b.VendorId).Distinct().Count();

        // By Vendor
        var byVendor = bills
            .GroupBy(b => new { b.VendorId, VendorName = b.Vendor?.Name ?? "Unknown" })
            .Select(g => new ExpenseByVendor
            {
                VendorId = g.Key.VendorId,
                VendorName = g.Key.VendorName,
                Amount = g.Sum(b => b.Subtotal),
                BillCount = g.Count(),
                Percentage = report.TotalExpenses > 0 ? (g.Sum(b => b.Subtotal) / report.TotalExpenses) * 100 : 0
            })
            .OrderByDescending(v => v.Amount)
            .ToList();
        report.ByVendor = byVendor;

        // By Category (Account)
        var allLines = bills.SelectMany(b => b.Lines).ToList();
        var byCategory = allLines
            .Where(l => l.AccountId.HasValue)
            .GroupBy(l => new { l.AccountId, AccountName = l.Account?.Name ?? "Unknown" })
            .Select(g => new ExpenseByCategory
            {
                AccountId = g.Key.AccountId ?? 0,
                AccountName = g.Key.AccountName,
                Amount = g.Sum(l => l.LineTotal),
                Percentage = report.TotalExpenses > 0 ? (g.Sum(l => l.LineTotal) / report.TotalExpenses) * 100 : 0
            })
            .OrderByDescending(c => c.Amount)
            .ToList();
        report.ByCategory = byCategory;

        // By Month
        var byMonth = bills
            .GroupBy(b => new { b.TransactionDate.Year, b.TransactionDate.Month })
            .Select(g => new ExpenseByMonth
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                Amount = g.Sum(b => b.Subtotal),
                BillCount = g.Count()
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();
        report.ByMonth = byMonth;

        return report;
    }

    public async Task<TaxSummaryReport> GetTaxSummaryAsync(DateTime startDate, DateTime endDate)
    {
        var report = new TaxSummaryReport
        {
            StartDate = startDate,
            EndDate = endDate
        };

        // Get tax collected from invoices
        var invoices = await _context.Invoices
            .Where(i => i.TransactionDate >= startDate && i.TransactionDate <= endDate &&
                       i.Status != InvoiceStatus.Void)
            .ToListAsync();

        report.TaxableIncome = invoices.Sum(i => i.Subtotal);
        report.TaxCollected = invoices.Sum(i => i.TaxAmount);

        // Get tax paid from bills (simplified - would need tax tracking on bills)
        var bills = await _context.Bills
            .Where(b => b.TransactionDate >= startDate && b.TransactionDate <= endDate &&
                       b.Status != BillStatus.Void)
            .ToListAsync();

        report.TaxPaid = bills.Sum(b => b.TaxAmount);

        return report;
    }

    #endregion

    #region Statement Reports

    public async Task<CustomerStatementReport> GetCustomerStatementAsync(int customerId, DateTime startDate, DateTime endDate)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null)
            throw new InvalidOperationException("Customer not found");

        var report = new CustomerStatementReport
        {
            StartDate = startDate,
            EndDate = endDate,
            CustomerId = customerId,
            CustomerName = customer.Name,
            CustomerEmail = customer.Email,
            CustomerAddress = FormatAddress(customer.BillingAddress1, customer.BillingCity, customer.BillingState, customer.BillingPostalCode)
        };

        // Get invoices
        var invoices = await _context.Invoices
            .Where(i => i.CustomerId == customerId && 
                       i.TransactionDate >= startDate && 
                       i.TransactionDate <= endDate &&
                       i.Status != InvoiceStatus.Void)
            .OrderBy(i => i.TransactionDate)
            .ToListAsync();

        // Get payments
        var payments = await _context.Payments
            .Where(p => p.CustomerId == customerId && 
                       p.TransactionDate >= startDate && 
                       p.TransactionDate <= endDate)
            .OrderBy(p => p.TransactionDate)
            .ToListAsync();

        decimal runningBalance = 0;

        // Combine and sort transactions
        var transactions = new List<(DateTime Date, string Type, string Ref, string Desc, decimal Amount)>();
        
        foreach (var inv in invoices)
        {
            transactions.Add((inv.TransactionDate, "Invoice", inv.TransactionNumber, inv.Memo ?? "", inv.Total));
        }
        
        foreach (var pay in payments)
        {
            transactions.Add((pay.TransactionDate, "Payment", pay.TransactionNumber, pay.Memo ?? "", -pay.Total));
        }

        foreach (var trans in transactions.OrderBy(t => t.Date).ThenBy(t => t.Type))
        {
            runningBalance += trans.Amount;
            report.Transactions.Add(new StatementTransaction
            {
                Date = trans.Date,
                Type = trans.Type,
                Reference = trans.Ref,
                Description = trans.Desc,
                Amount = trans.Amount,
                Balance = runningBalance
            });
        }

        report.ClosingBalance = runningBalance;

        return report;
    }

    public async Task<VendorStatementReport> GetVendorStatementAsync(int vendorId, DateTime startDate, DateTime endDate)
    {
        var vendor = await _context.Vendors.FindAsync(vendorId);
        if (vendor == null)
            throw new InvalidOperationException("Vendor not found");

        var report = new VendorStatementReport
        {
            StartDate = startDate,
            EndDate = endDate,
            VendorId = vendorId,
            VendorName = vendor.Name,
            VendorEmail = vendor.Email,
            VendorAddress = FormatAddress(vendor.Address1, vendor.City, vendor.State, vendor.PostalCode)
        };

        // Get bills
        var bills = await _context.Bills
            .Where(b => b.VendorId == vendorId && 
                       b.TransactionDate >= startDate && 
                       b.TransactionDate <= endDate &&
                       b.Status != BillStatus.Void)
            .OrderBy(b => b.TransactionDate)
            .ToListAsync();

        // Get payments to vendor
        var payments = await _context.Payments
            .Where(p => p.VendorId == vendorId && 
                       p.TransactionDate >= startDate && 
                       p.TransactionDate <= endDate)
            .OrderBy(p => p.TransactionDate)
            .ToListAsync();

        decimal runningBalance = 0;

        // Combine and sort transactions
        var transactions = new List<(DateTime Date, string Type, string Ref, string Desc, decimal Amount)>();
        
        foreach (var bill in bills)
        {
            transactions.Add((bill.TransactionDate, "Bill", bill.TransactionNumber, bill.Memo ?? "", bill.Total));
        }
        
        foreach (var pay in payments)
        {
            transactions.Add((pay.TransactionDate, "Payment", pay.TransactionNumber, pay.Memo ?? "", -pay.Total));
        }

        foreach (var trans in transactions.OrderBy(t => t.Date).ThenBy(t => t.Type))
        {
            runningBalance += trans.Amount;
            report.Transactions.Add(new StatementTransaction
            {
                Date = trans.Date,
                Type = trans.Type,
                Reference = trans.Ref,
                Description = trans.Desc,
                Amount = trans.Amount,
                Balance = runningBalance
            });
        }

        report.ClosingBalance = runningBalance;

        return report;
    }

    #endregion

    #region Helpers

    private string FormatSubType(AccountSubType subType)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            subType.ToString(), 
            "([a-z])([A-Z])", 
            "$1 $2"
        );
    }

    private async Task<decimal> CalculateNetIncomeAsync()
    {
        var incomeTotal = await _context.Accounts
            .Where(a => a.AccountType == AccountType.Income)
            .SumAsync(a => a.Balance);

        var expenseTotal = await _context.Accounts
            .Where(a => a.AccountType == AccountType.Expense)
            .SumAsync(a => a.Balance);

        return incomeTotal - expenseTotal;
    }

    private async Task<decimal> GetAccountBalanceChangeAsync(AccountSubType subType, DateTime startDate, DateTime endDate)
    {
        // Simplified - returns current balance
        var accounts = await _context.Accounts
            .Where(a => a.SubType == subType)
            .ToListAsync();

        return accounts.Sum(a => a.Balance);
    }

    private async Task<decimal> GetEquityChangeAsync(DateTime startDate, DateTime endDate)
    {
        // Simplified
        var equityAccounts = await _context.Accounts
            .Where(a => a.AccountType == AccountType.Equity && a.SubType == AccountSubType.OwnersEquity)
            .ToListAsync();

        return equityAccounts.Sum(a => a.Balance);
    }

    private string? FormatAddress(string? street, string? city, string? state, string? zip)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(street)) parts.Add(street);
        
        var cityStateZip = new List<string>();
        if (!string.IsNullOrEmpty(city)) cityStateZip.Add(city);
        if (!string.IsNullOrEmpty(state)) cityStateZip.Add(state);
        if (!string.IsNullOrEmpty(zip)) cityStateZip.Add(zip);
        
        if (cityStateZip.Any())
            parts.Add(string.Join(", ", cityStateZip));

        return parts.Any() ? string.Join("\n", parts) : null;
    }

    #endregion
}
