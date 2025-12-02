using Microsoft.EntityFrameworkCore;
using Kwikbooks.Data;
using Kwikbooks.Data.Models;

namespace Kwikbooks.Services;

/// <summary>
/// Represents a transaction in vendor history.
/// </summary>
public class VendorTransactionItem
{
    public int Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string TypeIcon { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Status { get; set; }
    public string? StatusClass { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public string? Memo { get; set; }
    public bool IsCredit { get; set; }
}

/// <summary>
/// Vendor balance summary.
/// </summary>
public class VendorBalanceSummary
{
    public decimal TotalBilled { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal OverdueAmount { get; set; }
    public int OpenBills { get; set; }
    public int OverdueBills { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public decimal LastPaymentAmount { get; set; }
    public DateTime? OldestOpenBillDate { get; set; }
}

/// <summary>
/// Service for managing vendors/suppliers.
/// </summary>
public interface IVendorService : IDataService<Vendor>
{
    /// <summary>
    /// Gets all active vendors.
    /// </summary>
    Task<IEnumerable<Vendor>> GetActiveVendorsAsync();
    
    /// <summary>
    /// Searches vendors by name, company, or email.
    /// </summary>
    Task<IEnumerable<Vendor>> SearchVendorsAsync(string searchTerm);
    
    /// <summary>
    /// Gets a vendor by their vendor number.
    /// </summary>
    Task<Vendor?> GetByVendorNumberAsync(string vendorNumber);
    
    /// <summary>
    /// Gets vendors with outstanding balances.
    /// </summary>
    Task<IEnumerable<Vendor>> GetVendorsWithBalanceAsync();
    
    /// <summary>
    /// Gets vendors eligible for 1099 reporting.
    /// </summary>
    Task<IEnumerable<Vendor>> Get1099VendorsAsync();
    
    /// <summary>
    /// Gets the next available vendor number.
    /// </summary>
    Task<string> GetNextVendorNumberAsync();
    
    /// <summary>
    /// Updates a vendor's balance.
    /// </summary>
    Task UpdateBalanceAsync(int vendorId, decimal amount);
    
    /// <summary>
    /// Gets total amount owed to all vendors (Accounts Payable).
    /// </summary>
    Task<decimal> GetTotalPayablesAsync();
    
    /// <summary>
    /// Gets the count of active vendors.
    /// </summary>
    Task<int> GetActiveVendorCountAsync();
    
    /// <summary>
    /// Gets transaction history for a vendor (bills and payments).
    /// </summary>
    Task<IEnumerable<VendorTransactionItem>> GetVendorTransactionsAsync(int vendorId, int? limit = null);
    
    /// <summary>
    /// Gets balance summary for a vendor.
    /// </summary>
    Task<VendorBalanceSummary> GetVendorBalanceSummaryAsync(int vendorId);
    
    /// <summary>
    /// Recalculates and updates vendor balance based on bills and payments.
    /// </summary>
    Task RecalculateBalanceAsync(int vendorId);
}

/// <summary>
/// Implementation of the vendor service.
/// </summary>
public class VendorService : BaseDataService<Vendor>, IVendorService
{
    public VendorService(KwikbooksDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Vendor>> GetAllAsync()
    {
        return await _dbSet
            .Include(v => v.DefaultExpenseAccount)
            .OrderBy(v => v.Name)
            .ToListAsync();
    }

    public override async Task<Vendor> CreateAsync(Vendor vendor)
    {
        if (string.IsNullOrEmpty(vendor.VendorNumber))
        {
            vendor.VendorNumber = await GetNextVendorNumberAsync();
        }
        
        return await base.CreateAsync(vendor);
    }

    public async Task<IEnumerable<Vendor>> GetActiveVendorsAsync()
    {
        return await _dbSet
            .Where(v => v.IsActive)
            .OrderBy(v => v.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Vendor>> SearchVendorsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        var term = searchTerm.ToLower();
        return await _dbSet
            .Where(v => v.Name.ToLower().Contains(term) ||
                        (v.CompanyName != null && v.CompanyName.ToLower().Contains(term)) ||
                        (v.Email != null && v.Email.ToLower().Contains(term)) ||
                        v.VendorNumber.ToLower().Contains(term))
            .OrderBy(v => v.Name)
            .ToListAsync();
    }

    public async Task<Vendor?> GetByVendorNumberAsync(string vendorNumber)
    {
        return await _dbSet
            .Include(v => v.DefaultExpenseAccount)
            .FirstOrDefaultAsync(v => v.VendorNumber == vendorNumber);
    }

    public async Task<IEnumerable<Vendor>> GetVendorsWithBalanceAsync()
    {
        return await _dbSet
            .Where(v => v.Balance > 0)
            .OrderByDescending(v => v.Balance)
            .ToListAsync();
    }

    public async Task<IEnumerable<Vendor>> Get1099VendorsAsync()
    {
        return await _dbSet
            .Where(v => v.IsEligibleFor1099 && v.IsActive)
            .OrderBy(v => v.Name)
            .ToListAsync();
    }

    public async Task<string> GetNextVendorNumberAsync()
    {
        var lastVendor = await _dbSet
            .OrderByDescending(v => v.VendorNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastVendor != null && !string.IsNullOrEmpty(lastVendor.VendorNumber))
        {
            var parts = lastVendor.VendorNumber.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[1], out int currentNumber))
            {
                nextNumber = currentNumber + 1;
            }
        }

        return $"VEND-{nextNumber:D4}";
    }

    public async Task UpdateBalanceAsync(int vendorId, decimal amount)
    {
        var vendor = await _dbSet.FindAsync(vendorId);
        if (vendor != null)
        {
            vendor.Balance += amount;
            vendor.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetTotalPayablesAsync()
    {
        return await _dbSet
            .Where(v => v.Balance > 0)
            .SumAsync(v => v.Balance);
    }

    public async Task<int> GetActiveVendorCountAsync()
    {
        return await _dbSet.CountAsync(v => v.IsActive);
    }

    public async Task<IEnumerable<VendorTransactionItem>> GetVendorTransactionsAsync(int vendorId, int? limit = null)
    {
        var transactions = new List<VendorTransactionItem>();
        
        // Get bills for vendor
        var bills = await _context.Bills
            .Where(b => b.VendorId == vendorId)
            .OrderByDescending(b => b.TransactionDate)
            .ToListAsync();
        
        foreach (var bill in bills)
        {
            var (status, statusClass) = GetBillStatusDisplay(bill.Status, bill.IsOverdue);
            transactions.Add(new VendorTransactionItem
            {
                Id = bill.Id,
                TransactionNumber = bill.TransactionNumber,
                Type = "Bill",
                TypeIcon = "📃",
                Date = bill.TransactionDate,
                DueDate = bill.DueDate,
                Status = status,
                StatusClass = statusClass,
                Amount = bill.Total,
                Balance = bill.BalanceDue,
                Memo = bill.Memo ?? bill.VendorInvoiceNumber,
                IsCredit = false
            });
        }
        
        // Get payments to vendor
        var payments = await _context.Payments
            .Where(p => p.VendorId == vendorId && p.Direction == PaymentDirection.Made)
            .OrderByDescending(p => p.TransactionDate)
            .ToListAsync();
        
        foreach (var payment in payments)
        {
            transactions.Add(new VendorTransactionItem
            {
                Id = payment.Id,
                TransactionNumber = payment.TransactionNumber,
                Type = "Payment",
                TypeIcon = "💸",
                Date = payment.TransactionDate,
                Status = "Paid",
                StatusClass = "badge-success",
                Amount = payment.Total,
                Balance = 0,
                Memo = payment.Memo ?? GetPaymentMethodName(payment.PaymentMethod),
                IsCredit = true
            });
        }
        
        // Sort by date descending
        var sorted = transactions.OrderByDescending(t => t.Date).ThenByDescending(t => t.Id);
        
        if (limit.HasValue)
            return sorted.Take(limit.Value).ToList();
            
        return sorted.ToList();
    }

    public async Task<VendorBalanceSummary> GetVendorBalanceSummaryAsync(int vendorId)
    {
        var vendor = await _dbSet.FindAsync(vendorId);
        if (vendor == null)
            return new VendorBalanceSummary();
        
        var bills = await _context.Bills
            .Where(b => b.VendorId == vendorId)
            .ToListAsync();
        
        var payments = await _context.Payments
            .Where(p => p.VendorId == vendorId && p.Direction == PaymentDirection.Made)
            .OrderByDescending(p => p.TransactionDate)
            .ToListAsync();
        
        var openBills = bills.Where(b => b.Status != BillStatus.Paid && b.Status != BillStatus.Void).ToList();
        var overdueBills = openBills.Where(b => b.IsOverdue).ToList();
        var lastPayment = payments.FirstOrDefault();
        
        return new VendorBalanceSummary
        {
            TotalBilled = bills.Where(b => b.Status != BillStatus.Void).Sum(b => b.Total),
            TotalPayments = payments.Sum(p => p.Total),
            CurrentBalance = vendor.Balance,
            OverdueAmount = overdueBills.Sum(b => b.BalanceDue),
            OpenBills = openBills.Count,
            OverdueBills = overdueBills.Count,
            LastPaymentDate = lastPayment?.TransactionDate,
            LastPaymentAmount = lastPayment?.Total ?? 0,
            OldestOpenBillDate = openBills.OrderBy(b => b.TransactionDate).FirstOrDefault()?.TransactionDate
        };
    }

    public async Task RecalculateBalanceAsync(int vendorId)
    {
        var vendor = await _dbSet.FindAsync(vendorId);
        if (vendor == null) return;
        
        // Sum of unpaid bill balances
        var billBalance = await _context.Bills
            .Where(b => b.VendorId == vendorId && 
                        b.Status != BillStatus.Paid && 
                        b.Status != BillStatus.Void)
            .SumAsync(b => b.Total - b.AmountPaid);
        
        vendor.Balance = billBalance;
        vendor.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private static (string Status, string Class) GetBillStatusDisplay(BillStatus status, bool isOverdue)
    {
        if (isOverdue)
            return ("Overdue", "badge-danger");
            
        return status switch
        {
            BillStatus.Draft => ("Draft", "badge-secondary"),
            BillStatus.Received => ("Received", "badge-info"),
            BillStatus.PartiallyPaid => ("Partial", "badge-warning"),
            BillStatus.Paid => ("Paid", "badge-success"),
            BillStatus.Void => ("Void", "badge-secondary"),
            _ => ("Unknown", "badge-secondary")
        };
    }

    private static string GetPaymentMethodName(PaymentMethod method)
    {
        return method switch
        {
            PaymentMethod.Cash => "Cash",
            PaymentMethod.Check => "Check",
            PaymentMethod.CreditCard => "Credit Card",
            PaymentMethod.DebitCard => "Debit Card",
            PaymentMethod.BankTransfer => "Bank Transfer",
            PaymentMethod.ACH => "ACH",
            PaymentMethod.Wire => "Wire Transfer",
            PaymentMethod.PayPal => "PayPal",
            PaymentMethod.Venmo => "Venmo",
            _ => "Other"
        };
    }
}
