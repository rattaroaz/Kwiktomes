using Microsoft.EntityFrameworkCore;
using Kwiktomes.Data;
using Kwiktomes.Data.Models;

namespace Kwiktomes.Services;

/// <summary>
/// Service for managing bills and expenses.
/// </summary>
public interface IBillService : IDataService<Bill>
{
    /// <summary>
    /// Gets all bills with related data.
    /// </summary>
    Task<IEnumerable<Bill>> GetAllWithDetailsAsync();
    
    /// <summary>
    /// Gets a bill by ID with all related data.
    /// </summary>
    Task<Bill?> GetByIdWithDetailsAsync(int id);
    
    /// <summary>
    /// Gets bills by status.
    /// </summary>
    Task<IEnumerable<Bill>> GetByStatusAsync(BillStatus status);
    
    /// <summary>
    /// Gets bills for a specific vendor.
    /// </summary>
    Task<IEnumerable<Bill>> GetByVendorAsync(int vendorId);
    
    /// <summary>
    /// Gets overdue bills.
    /// </summary>
    Task<IEnumerable<Bill>> GetOverdueBillsAsync();
    
    /// <summary>
    /// Gets the next bill number.
    /// </summary>
    Task<string> GetNextBillNumberAsync();
    
    /// <summary>
    /// Creates a bill with line items.
    /// </summary>
    Task<Bill> CreateWithLinesAsync(Bill bill, List<BillLine> lines);
    
    /// <summary>
    /// Updates a bill with line items.
    /// </summary>
    Task<Bill> UpdateWithLinesAsync(Bill bill, List<BillLine> lines);
    
    /// <summary>
    /// Updates bill status.
    /// </summary>
    Task UpdateStatusAsync(int billId, BillStatus status);
    
    /// <summary>
    /// Applies a payment to a bill.
    /// </summary>
    Task ApplyPaymentAsync(int billId, decimal amount, int paymentId);
    
    /// <summary>
    /// Gets bill summary statistics.
    /// </summary>
    Task<BillSummary> GetBillSummaryAsync();
    
    /// <summary>
    /// Searches bills.
    /// </summary>
    Task<IEnumerable<Bill>> SearchBillsAsync(string searchTerm);
    
    /// <summary>
    /// Voids a bill.
    /// </summary>
    Task VoidBillAsync(int billId);
    
    /// <summary>
    /// Gets all expense categories (unique account names used in bills).
    /// </summary>
    Task<IEnumerable<Account>> GetExpenseAccountsAsync();
    
    /// <summary>
    /// Creates a quick expense (single-line bill).
    /// </summary>
    Task<Bill> CreateQuickExpenseAsync(int vendorId, int accountId, decimal amount, string description, DateTime date);
}

/// <summary>
/// Bill summary statistics.
/// </summary>
public class BillSummary
{
    public int TotalBills { get; set; }
    public int DraftCount { get; set; }
    public int ReceivedCount { get; set; }
    public int OverdueCount { get; set; }
    public int PaidCount { get; set; }
    public decimal TotalOutstanding { get; set; }
    public decimal TotalOverdue { get; set; }
    public decimal TotalPaidThisMonth { get; set; }
}

/// <summary>
/// Implementation of the bill service.
/// </summary>
public class BillService : BaseDataService<Bill>, IBillService
{
    public BillService(KwiktomesDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Bill>> GetAllAsync()
    {
        return await _dbSet
            .Include(b => b.Vendor)
            .Include(b => b.Lines)
            .OrderByDescending(b => b.TransactionDate)
            .ThenByDescending(b => b.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Bill>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(b => b.Vendor)
            .Include(b => b.Lines)
                .ThenInclude(l => l.Product)
            .Include(b => b.Lines)
                .ThenInclude(l => l.Account)
            .Include(b => b.PaymentApplications)
            .OrderByDescending(b => b.TransactionDate)
            .ThenByDescending(b => b.Id)
            .ToListAsync();
    }

    public async Task<Bill?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(b => b.Vendor)
            .Include(b => b.Lines)
                .ThenInclude(l => l.Product)
            .Include(b => b.Lines)
                .ThenInclude(l => l.Account)
            .Include(b => b.PaymentApplications)
                .ThenInclude(pa => pa.Payment)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Bill>> GetByStatusAsync(BillStatus status)
    {
        return await _dbSet
            .Include(b => b.Vendor)
            .Where(b => b.Status == status)
            .OrderByDescending(b => b.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Bill>> GetByVendorAsync(int vendorId)
    {
        return await _dbSet
            .Include(b => b.Lines)
            .Where(b => b.VendorId == vendorId)
            .OrderByDescending(b => b.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Bill>> GetOverdueBillsAsync()
    {
        var today = DateTime.Today;
        return await _dbSet
            .Include(b => b.Vendor)
            .Where(b => b.Status != BillStatus.Paid && 
                        b.Status != BillStatus.Void &&
                        b.DueDate.HasValue && 
                        b.DueDate.Value < today)
            .OrderBy(b => b.DueDate)
            .ToListAsync();
    }

    public async Task<string> GetNextBillNumberAsync()
    {
        var lastBill = await _dbSet
            .OrderByDescending(b => b.TransactionNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1001;
        if (lastBill != null && !string.IsNullOrEmpty(lastBill.TransactionNumber))
        {
            var parts = lastBill.TransactionNumber.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[1], out int currentNumber))
            {
                nextNumber = currentNumber + 1;
            }
        }

        return $"BILL-{nextNumber:D4}";
    }

    public async Task<Bill> CreateWithLinesAsync(Bill bill, List<BillLine> lines)
    {
        if (string.IsNullOrEmpty(bill.TransactionNumber))
        {
            bill.TransactionNumber = await GetNextBillNumberAsync();
        }

        // Calculate totals
        decimal subtotal = lines.Sum(l => l.LineTotal);

        bill.Subtotal = subtotal;
        bill.Total = subtotal + bill.TaxAmount - bill.DiscountAmount;
        bill.Lines = lines;

        foreach (var line in lines)
        {
            line.Bill = bill;
        }

        _dbSet.Add(bill);
        await _context.SaveChangesAsync();

        // Update vendor balance
        await UpdateVendorBalanceAsync(bill.VendorId, bill.Total);

        return bill;
    }

    public async Task<Bill> UpdateWithLinesAsync(Bill bill, List<BillLine> lines)
    {
        var existingBill = await _dbSet
            .Include(b => b.Lines)
            .FirstOrDefaultAsync(b => b.Id == bill.Id);

        if (existingBill == null)
            throw new InvalidOperationException("Bill not found");

        var oldTotal = existingBill.Total;

        // Remove existing lines
        _context.BillLines.RemoveRange(existingBill.Lines);

        // Update bill properties
        existingBill.VendorId = bill.VendorId;
        existingBill.TransactionDate = bill.TransactionDate;
        existingBill.DueDate = bill.DueDate;
        existingBill.VendorInvoiceNumber = bill.VendorInvoiceNumber;
        existingBill.ReceivedDate = bill.ReceivedDate;
        existingBill.Memo = bill.Memo;
        existingBill.TaxAmount = bill.TaxAmount;
        existingBill.DiscountAmount = bill.DiscountAmount;
        existingBill.UpdatedAt = DateTime.UtcNow;

        // Calculate totals
        decimal subtotal = lines.Sum(l => l.LineTotal);

        existingBill.Subtotal = subtotal;
        existingBill.Total = subtotal + existingBill.TaxAmount - existingBill.DiscountAmount;

        foreach (var line in lines)
        {
            line.BillId = bill.Id;
            _context.BillLines.Add(line);
        }

        await _context.SaveChangesAsync();

        // Update vendor balance (adjust for difference)
        var balanceAdjustment = existingBill.Total - oldTotal;
        if (balanceAdjustment != 0)
        {
            await UpdateVendorBalanceAsync(existingBill.VendorId, balanceAdjustment);
        }

        return existingBill;
    }

    public async Task UpdateStatusAsync(int billId, BillStatus status)
    {
        var bill = await _dbSet.FindAsync(billId);
        if (bill == null) return;

        bill.Status = status;
        bill.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task ApplyPaymentAsync(int billId, decimal amount, int paymentId)
    {
        var bill = await _dbSet.FindAsync(billId);
        if (bill == null) return;

        bill.AmountPaid += amount;
        
        if (bill.AmountPaid >= bill.Total)
        {
            bill.Status = BillStatus.Paid;
        }
        else if (bill.AmountPaid > 0)
        {
            bill.Status = BillStatus.PartiallyPaid;
        }

        bill.UpdatedAt = DateTime.UtcNow;

        // Create payment application
        var application = new PaymentApplication
        {
            PaymentId = paymentId,
            BillId = billId,
            AppliedAmount = amount,
            AppliedDate = DateTime.Today
        };
        _context.PaymentApplications.Add(application);

        await _context.SaveChangesAsync();

        // Update vendor balance
        await UpdateVendorBalanceAsync(bill.VendorId, -amount);
    }

    public async Task<BillSummary> GetBillSummaryAsync()
    {
        var bills = await _dbSet.ToListAsync();
        var today = DateTime.Today;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        var overdue = bills.Where(b => 
            b.Status != BillStatus.Paid && 
            b.Status != BillStatus.Void &&
            b.DueDate.HasValue && 
            b.DueDate.Value < today).ToList();

        return new BillSummary
        {
            TotalBills = bills.Count(b => b.Status != BillStatus.Void),
            DraftCount = bills.Count(b => b.Status == BillStatus.Draft),
            ReceivedCount = bills.Count(b => b.Status == BillStatus.Received),
            OverdueCount = overdue.Count,
            PaidCount = bills.Count(b => b.Status == BillStatus.Paid),
            TotalOutstanding = bills
                .Where(b => b.Status != BillStatus.Paid && b.Status != BillStatus.Void)
                .Sum(b => b.BalanceDue),
            TotalOverdue = overdue.Sum(b => b.BalanceDue),
            TotalPaidThisMonth = bills
                .Where(b => b.Status == BillStatus.Paid && b.UpdatedAt >= startOfMonth)
                .Sum(b => b.Total)
        };
    }

    public async Task<IEnumerable<Bill>> SearchBillsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllWithDetailsAsync();

        var term = searchTerm.ToLower();
        return await _dbSet
            .Include(b => b.Vendor)
            .Include(b => b.Lines)
            .Where(b => b.TransactionNumber.ToLower().Contains(term) ||
                        b.Vendor.Name.ToLower().Contains(term) ||
                        (b.VendorInvoiceNumber != null && b.VendorInvoiceNumber.ToLower().Contains(term)))
            .OrderByDescending(b => b.TransactionDate)
            .ToListAsync();
    }

    public async Task VoidBillAsync(int billId)
    {
        var bill = await _dbSet.FindAsync(billId);
        if (bill == null) return;

        var previousBalance = bill.BalanceDue;
        bill.Status = BillStatus.Void;
        bill.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Reduce vendor balance
        if (previousBalance > 0)
        {
            await UpdateVendorBalanceAsync(bill.VendorId, -previousBalance);
        }
    }

    public async Task<IEnumerable<Account>> GetExpenseAccountsAsync()
    {
        return await _context.Accounts
            .Where(a => a.AccountType == AccountType.Expense && a.IsActive)
            .OrderBy(a => a.AccountNumber)
            .ToListAsync();
    }

    public async Task<Bill> CreateQuickExpenseAsync(int vendorId, int accountId, decimal amount, string description, DateTime date)
    {
        var bill = new Bill
        {
            TransactionNumber = await GetNextBillNumberAsync(),
            VendorId = vendorId,
            TransactionDate = date,
            ReceivedDate = date,
            DueDate = date,
            Status = BillStatus.Received,
            Subtotal = amount,
            Total = amount
        };

        var line = new BillLine
        {
            Description = description,
            Quantity = 1,
            UnitCost = amount,
            AccountId = accountId,
            Bill = bill
        };

        bill.Lines = new List<BillLine> { line };

        _dbSet.Add(bill);
        await _context.SaveChangesAsync();

        await UpdateVendorBalanceAsync(vendorId, amount);

        return bill;
    }

    private async Task UpdateVendorBalanceAsync(int vendorId, decimal amount)
    {
        var vendor = await _context.Vendors.FindAsync(vendorId);
        if (vendor != null)
        {
            vendor.Balance += amount;
            vendor.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
