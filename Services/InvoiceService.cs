using Microsoft.EntityFrameworkCore;
using Kwikbooks.Data;
using Kwikbooks.Data.Models;

namespace Kwikbooks.Services;

/// <summary>
/// Service for managing invoices.
/// </summary>
public interface IInvoiceService : IDataService<Invoice>
{
    /// <summary>
    /// Gets all invoices with related data.
    /// </summary>
    Task<IEnumerable<Invoice>> GetAllWithDetailsAsync();
    
    /// <summary>
    /// Gets an invoice by ID with all related data.
    /// </summary>
    Task<Invoice?> GetByIdWithDetailsAsync(int id);
    
    /// <summary>
    /// Gets invoices by status.
    /// </summary>
    Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status);
    
    /// <summary>
    /// Gets invoices for a specific customer.
    /// </summary>
    Task<IEnumerable<Invoice>> GetByCustomerAsync(int customerId);
    
    /// <summary>
    /// Gets overdue invoices.
    /// </summary>
    Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();
    
    /// <summary>
    /// Gets the next invoice number.
    /// </summary>
    Task<string> GetNextInvoiceNumberAsync();
    
    /// <summary>
    /// Creates an invoice with line items.
    /// </summary>
    Task<Invoice> CreateWithLinesAsync(Invoice invoice, List<InvoiceLine> lines);
    
    /// <summary>
    /// Updates an invoice with line items.
    /// </summary>
    Task<Invoice> UpdateWithLinesAsync(Invoice invoice, List<InvoiceLine> lines);
    
    /// <summary>
    /// Updates invoice status.
    /// </summary>
    Task UpdateStatusAsync(int invoiceId, InvoiceStatus status);
    
    /// <summary>
    /// Marks an invoice as sent.
    /// </summary>
    Task MarkAsSentAsync(int invoiceId);
    
    /// <summary>
    /// Applies a payment to an invoice.
    /// </summary>
    Task ApplyPaymentAsync(int invoiceId, decimal amount, int paymentId);
    
    /// <summary>
    /// Gets invoice summary statistics.
    /// </summary>
    Task<InvoiceSummary> GetInvoiceSummaryAsync();
    
    /// <summary>
    /// Searches invoices.
    /// </summary>
    Task<IEnumerable<Invoice>> SearchInvoicesAsync(string searchTerm);
    
    /// <summary>
    /// Voids an invoice.
    /// </summary>
    Task VoidInvoiceAsync(int invoiceId);
    
    /// <summary>
    /// Recalculates invoice totals from line items.
    /// </summary>
    Task RecalculateTotalsAsync(int invoiceId);
}

/// <summary>
/// Invoice summary statistics.
/// </summary>
public class InvoiceSummary
{
    public int TotalInvoices { get; set; }
    public int DraftCount { get; set; }
    public int SentCount { get; set; }
    public int OverdueCount { get; set; }
    public int PaidCount { get; set; }
    public decimal TotalOutstanding { get; set; }
    public decimal TotalOverdue { get; set; }
    public decimal TotalPaidThisMonth { get; set; }
}

/// <summary>
/// Implementation of the invoice service.
/// </summary>
public class InvoiceService : BaseDataService<Invoice>, IInvoiceService
{
    public InvoiceService(KwikbooksDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Invoice>> GetAllAsync()
    {
        return await _dbSet
            .Include(i => i.Customer)
            .Include(i => i.Lines)
            .OrderByDescending(i => i.TransactionDate)
            .ThenByDescending(i => i.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(i => i.Customer)
            .Include(i => i.Lines)
                .ThenInclude(l => l.Product)
            .Include(i => i.PaymentApplications)
            .OrderByDescending(i => i.TransactionDate)
            .ThenByDescending(i => i.Id)
            .ToListAsync();
    }

    public async Task<Invoice?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(i => i.Customer)
            .Include(i => i.Lines)
                .ThenInclude(l => l.Product)
            .Include(i => i.PaymentApplications)
                .ThenInclude(pa => pa.Payment)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<IEnumerable<Invoice>> GetByStatusAsync(InvoiceStatus status)
    {
        return await _dbSet
            .Include(i => i.Customer)
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByCustomerAsync(int customerId)
    {
        return await _dbSet
            .Include(i => i.Lines)
            .Where(i => i.CustomerId == customerId)
            .OrderByDescending(i => i.TransactionDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
    {
        var today = DateTime.Today;
        return await _dbSet
            .Include(i => i.Customer)
            .Where(i => i.Status != InvoiceStatus.Paid && 
                        i.Status != InvoiceStatus.Void &&
                        i.DueDate.HasValue && 
                        i.DueDate.Value < today)
            .OrderBy(i => i.DueDate)
            .ToListAsync();
    }

    public async Task<string> GetNextInvoiceNumberAsync()
    {
        var lastInvoice = await _dbSet
            .OrderByDescending(i => i.TransactionNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1001;
        if (lastInvoice != null && !string.IsNullOrEmpty(lastInvoice.TransactionNumber))
        {
            var parts = lastInvoice.TransactionNumber.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[1], out int currentNumber))
            {
                nextNumber = currentNumber + 1;
            }
        }

        return $"INV-{nextNumber:D4}";
    }

    public async Task<Invoice> CreateWithLinesAsync(Invoice invoice, List<InvoiceLine> lines)
    {
        if (string.IsNullOrEmpty(invoice.TransactionNumber))
        {
            invoice.TransactionNumber = await GetNextInvoiceNumberAsync();
        }

        // Calculate totals
        decimal subtotal = 0;
        decimal taxTotal = 0;

        foreach (var line in lines)
        {
            line.Invoice = invoice;
            subtotal += line.LineTotal;
            taxTotal += line.LineTax;
        }

        invoice.Subtotal = subtotal;
        invoice.TaxAmount = taxTotal;
        invoice.Total = subtotal + taxTotal - invoice.DiscountAmount;
        invoice.Lines = lines;

        _dbSet.Add(invoice);
        await _context.SaveChangesAsync();

        // Update customer balance
        await UpdateCustomerBalanceAsync(invoice.CustomerId, invoice.Total);

        return invoice;
    }

    public async Task<Invoice> UpdateWithLinesAsync(Invoice invoice, List<InvoiceLine> lines)
    {
        var existingInvoice = await _dbSet
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == invoice.Id);

        if (existingInvoice == null)
            throw new InvalidOperationException("Invoice not found");

        var oldTotal = existingInvoice.Total;

        // Remove existing lines
        _context.InvoiceLines.RemoveRange(existingInvoice.Lines);

        // Update invoice properties
        existingInvoice.CustomerId = invoice.CustomerId;
        existingInvoice.TransactionDate = invoice.TransactionDate;
        existingInvoice.DueDate = invoice.DueDate;
        existingInvoice.BillingAddress = invoice.BillingAddress;
        existingInvoice.ShippingAddress = invoice.ShippingAddress;
        existingInvoice.Terms = invoice.Terms;
        existingInvoice.CustomerMessage = invoice.CustomerMessage;
        existingInvoice.Memo = invoice.Memo;
        existingInvoice.DiscountAmount = invoice.DiscountAmount;
        existingInvoice.UpdatedAt = DateTime.UtcNow;

        // Calculate totals
        decimal subtotal = 0;
        decimal taxTotal = 0;

        foreach (var line in lines)
        {
            line.InvoiceId = invoice.Id;
            subtotal += line.LineTotal;
            taxTotal += line.LineTax;
            _context.InvoiceLines.Add(line);
        }

        existingInvoice.Subtotal = subtotal;
        existingInvoice.TaxAmount = taxTotal;
        existingInvoice.Total = subtotal + taxTotal - existingInvoice.DiscountAmount;

        await _context.SaveChangesAsync();

        // Update customer balance (adjust for difference)
        var balanceAdjustment = existingInvoice.Total - oldTotal;
        if (balanceAdjustment != 0)
        {
            await UpdateCustomerBalanceAsync(existingInvoice.CustomerId, balanceAdjustment);
        }

        return existingInvoice;
    }

    public async Task UpdateStatusAsync(int invoiceId, InvoiceStatus status)
    {
        var invoice = await _dbSet.FindAsync(invoiceId);
        if (invoice == null) return;

        invoice.Status = status;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task MarkAsSentAsync(int invoiceId)
    {
        var invoice = await _dbSet.FindAsync(invoiceId);
        if (invoice == null) return;

        invoice.Status = InvoiceStatus.Sent;
        invoice.SentDate = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task ApplyPaymentAsync(int invoiceId, decimal amount, int paymentId)
    {
        var invoice = await _dbSet.FindAsync(invoiceId);
        if (invoice == null) return;

        invoice.AmountPaid += amount;
        
        if (invoice.AmountPaid >= invoice.Total)
        {
            invoice.Status = InvoiceStatus.Paid;
        }
        else if (invoice.AmountPaid > 0)
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }

        invoice.UpdatedAt = DateTime.UtcNow;

        // Create payment application
        var application = new PaymentApplication
        {
            PaymentId = paymentId,
            InvoiceId = invoiceId,
            AppliedAmount = amount,
            AppliedDate = DateTime.Today
        };
        _context.PaymentApplications.Add(application);

        await _context.SaveChangesAsync();

        // Update customer balance
        await UpdateCustomerBalanceAsync(invoice.CustomerId, -amount);
    }

    public async Task<InvoiceSummary> GetInvoiceSummaryAsync()
    {
        var invoices = await _dbSet.ToListAsync();
        var today = DateTime.Today;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        var overdue = invoices.Where(i => 
            i.Status != InvoiceStatus.Paid && 
            i.Status != InvoiceStatus.Void &&
            i.DueDate.HasValue && 
            i.DueDate.Value < today).ToList();

        return new InvoiceSummary
        {
            TotalInvoices = invoices.Count(i => i.Status != InvoiceStatus.Void),
            DraftCount = invoices.Count(i => i.Status == InvoiceStatus.Draft),
            SentCount = invoices.Count(i => i.Status == InvoiceStatus.Sent),
            OverdueCount = overdue.Count,
            PaidCount = invoices.Count(i => i.Status == InvoiceStatus.Paid),
            TotalOutstanding = invoices
                .Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Void)
                .Sum(i => i.BalanceDue),
            TotalOverdue = overdue.Sum(i => i.BalanceDue),
            TotalPaidThisMonth = invoices
                .Where(i => i.Status == InvoiceStatus.Paid && i.UpdatedAt >= startOfMonth)
                .Sum(i => i.Total)
        };
    }

    public async Task<IEnumerable<Invoice>> SearchInvoicesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllWithDetailsAsync();

        var term = searchTerm.ToLower();
        return await _dbSet
            .Include(i => i.Customer)
            .Include(i => i.Lines)
            .Where(i => i.TransactionNumber.ToLower().Contains(term) ||
                        i.Customer.Name.ToLower().Contains(term) ||
                        (i.Customer.CompanyName != null && i.Customer.CompanyName.ToLower().Contains(term)))
            .OrderByDescending(i => i.TransactionDate)
            .ToListAsync();
    }

    public async Task VoidInvoiceAsync(int invoiceId)
    {
        var invoice = await _dbSet.FindAsync(invoiceId);
        if (invoice == null) return;

        var previousBalance = invoice.BalanceDue;
        invoice.Status = InvoiceStatus.Void;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Reduce customer balance
        if (previousBalance > 0)
        {
            await UpdateCustomerBalanceAsync(invoice.CustomerId, -previousBalance);
        }
    }

    public async Task RecalculateTotalsAsync(int invoiceId)
    {
        var invoice = await _dbSet
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null) return;

        decimal subtotal = invoice.Lines.Sum(l => l.LineTotal);
        decimal taxTotal = invoice.Lines.Sum(l => l.LineTax);

        invoice.Subtotal = subtotal;
        invoice.TaxAmount = taxTotal;
        invoice.Total = subtotal + taxTotal - invoice.DiscountAmount;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private async Task UpdateCustomerBalanceAsync(int customerId, decimal amount)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer != null)
        {
            customer.Balance += amount;
            customer.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
