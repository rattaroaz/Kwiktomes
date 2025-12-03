using Microsoft.EntityFrameworkCore;
using Kwiktomes.Data;
using Kwiktomes.Data.Models;

namespace Kwiktomes.Services;

/// <summary>
/// Represents a transaction in customer history.
/// </summary>
public class CustomerTransactionItem
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
    public bool IsCredit { get; set; } // True for payments (reduce balance)
}

/// <summary>
/// Customer balance summary.
/// </summary>
public class CustomerBalanceSummary
{
    public decimal TotalInvoiced { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal OverdueAmount { get; set; }
    public int OpenInvoices { get; set; }
    public int OverdueInvoices { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public decimal LastPaymentAmount { get; set; }
    public DateTime? OldestOpenInvoiceDate { get; set; }
}

/// <summary>
/// Service for managing customers.
/// </summary>
public interface ICustomerService : IDataService<Customer>
{
    /// <summary>
    /// Gets all active customers.
    /// </summary>
    Task<IEnumerable<Customer>> GetActiveCustomersAsync();
    
    /// <summary>
    /// Searches customers by name, company, or email.
    /// </summary>
    Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm);
    
    /// <summary>
    /// Gets a customer by their customer number.
    /// </summary>
    Task<Customer?> GetByCustomerNumberAsync(string customerNumber);
    
    /// <summary>
    /// Gets customers with outstanding balances.
    /// </summary>
    Task<IEnumerable<Customer>> GetCustomersWithBalanceAsync();
    
    /// <summary>
    /// Gets the next available customer number.
    /// </summary>
    Task<string> GetNextCustomerNumberAsync();
    
    /// <summary>
    /// Updates a customer's balance.
    /// </summary>
    Task UpdateBalanceAsync(int customerId, decimal amount);
    
    /// <summary>
    /// Gets total amount owed by all customers (Accounts Receivable).
    /// </summary>
    Task<decimal> GetTotalReceivablesAsync();
    
    /// <summary>
    /// Gets the count of active customers.
    /// </summary>
    Task<int> GetActiveCustomerCountAsync();
    
    /// <summary>
    /// Gets transaction history for a customer (invoices and payments).
    /// </summary>
    Task<IEnumerable<CustomerTransactionItem>> GetCustomerTransactionsAsync(int customerId, int? limit = null);
    
    /// <summary>
    /// Gets balance summary for a customer.
    /// </summary>
    Task<CustomerBalanceSummary> GetCustomerBalanceSummaryAsync(int customerId);
    
    /// <summary>
    /// Recalculates and updates customer balance based on invoices and payments.
    /// </summary>
    Task RecalculateBalanceAsync(int customerId);
}

/// <summary>
/// Implementation of the customer service.
/// </summary>
public class CustomerService : BaseDataService<Customer>, ICustomerService
{
    public CustomerService(KwiktomesDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Customer>> GetAllAsync()
    {
        return await _dbSet
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public override async Task<Customer> CreateAsync(Customer customer)
    {
        // Generate customer number if not provided
        if (string.IsNullOrEmpty(customer.CustomerNumber))
        {
            customer.CustomerNumber = await GetNextCustomerNumberAsync();
        }
        
        return await base.CreateAsync(customer);
    }

    public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        var term = searchTerm.ToLower();
        return await _dbSet
            .Where(c => c.Name.ToLower().Contains(term) ||
                        (c.CompanyName != null && c.CompanyName.ToLower().Contains(term)) ||
                        (c.Email != null && c.Email.ToLower().Contains(term)) ||
                        c.CustomerNumber.ToLower().Contains(term))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Customer?> GetByCustomerNumberAsync(string customerNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.CustomerNumber == customerNumber);
    }

    public async Task<IEnumerable<Customer>> GetCustomersWithBalanceAsync()
    {
        return await _dbSet
            .Where(c => c.Balance > 0)
            .OrderByDescending(c => c.Balance)
            .ToListAsync();
    }

    public async Task<string> GetNextCustomerNumberAsync()
    {
        var lastCustomer = await _dbSet
            .OrderByDescending(c => c.CustomerNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastCustomer != null && !string.IsNullOrEmpty(lastCustomer.CustomerNumber))
        {
            // Extract number from format "CUST-0001"
            var parts = lastCustomer.CustomerNumber.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[1], out int currentNumber))
            {
                nextNumber = currentNumber + 1;
            }
        }

        return $"CUST-{nextNumber:D4}";
    }

    public async Task UpdateBalanceAsync(int customerId, decimal amount)
    {
        var customer = await _dbSet.FindAsync(customerId);
        if (customer != null)
        {
            customer.Balance += amount;
            customer.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetTotalReceivablesAsync()
    {
        return await _dbSet
            .Where(c => c.Balance > 0)
            .SumAsync(c => c.Balance);
    }

    public async Task<int> GetActiveCustomerCountAsync()
    {
        return await _dbSet.CountAsync(c => c.IsActive);
    }

    public async Task<IEnumerable<CustomerTransactionItem>> GetCustomerTransactionsAsync(int customerId, int? limit = null)
    {
        var transactions = new List<CustomerTransactionItem>();
        
        // Get invoices for customer
        var invoicesQuery = _context.Invoices
            .Where(i => i.CustomerId == customerId)
            .OrderByDescending(i => i.TransactionDate)
            .AsQueryable();
            
        var invoices = await invoicesQuery.ToListAsync();
        
        foreach (var invoice in invoices)
        {
            var (status, statusClass) = GetInvoiceStatusDisplay(invoice.Status, invoice.IsOverdue);
            transactions.Add(new CustomerTransactionItem
            {
                Id = invoice.Id,
                TransactionNumber = invoice.TransactionNumber,
                Type = "Invoice",
                TypeIcon = "📄",
                Date = invoice.TransactionDate,
                DueDate = invoice.DueDate,
                Status = status,
                StatusClass = statusClass,
                Amount = invoice.Total,
                Balance = invoice.BalanceDue,
                Memo = invoice.Memo,
                IsCredit = false
            });
        }
        
        // Get payments from customer
        var payments = await _context.Payments
            .Where(p => p.CustomerId == customerId && p.Direction == PaymentDirection.Received)
            .OrderByDescending(p => p.TransactionDate)
            .ToListAsync();
        
        foreach (var payment in payments)
        {
            transactions.Add(new CustomerTransactionItem
            {
                Id = payment.Id,
                TransactionNumber = payment.TransactionNumber,
                Type = "Payment",
                TypeIcon = "💳",
                Date = payment.TransactionDate,
                Status = "Received",
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

    public async Task<CustomerBalanceSummary> GetCustomerBalanceSummaryAsync(int customerId)
    {
        var customer = await _dbSet.FindAsync(customerId);
        if (customer == null)
            return new CustomerBalanceSummary();
        
        var invoices = await _context.Invoices
            .Where(i => i.CustomerId == customerId)
            .ToListAsync();
        
        var payments = await _context.Payments
            .Where(p => p.CustomerId == customerId && p.Direction == PaymentDirection.Received)
            .OrderByDescending(p => p.TransactionDate)
            .ToListAsync();
        
        var openInvoices = invoices.Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Void).ToList();
        var overdueInvoices = openInvoices.Where(i => i.IsOverdue).ToList();
        var lastPayment = payments.FirstOrDefault();
        
        return new CustomerBalanceSummary
        {
            TotalInvoiced = invoices.Where(i => i.Status != InvoiceStatus.Void).Sum(i => i.Total),
            TotalPayments = payments.Sum(p => p.Total),
            CurrentBalance = customer.Balance,
            OverdueAmount = overdueInvoices.Sum(i => i.BalanceDue),
            OpenInvoices = openInvoices.Count,
            OverdueInvoices = overdueInvoices.Count,
            LastPaymentDate = lastPayment?.TransactionDate,
            LastPaymentAmount = lastPayment?.Total ?? 0,
            OldestOpenInvoiceDate = openInvoices.OrderBy(i => i.TransactionDate).FirstOrDefault()?.TransactionDate
        };
    }

    public async Task RecalculateBalanceAsync(int customerId)
    {
        var customer = await _dbSet.FindAsync(customerId);
        if (customer == null) return;
        
        // Sum of unpaid invoice balances
        var invoiceBalance = await _context.Invoices
            .Where(i => i.CustomerId == customerId && 
                        i.Status != InvoiceStatus.Paid && 
                        i.Status != InvoiceStatus.Void)
            .SumAsync(i => i.Total - i.AmountPaid);
        
        customer.Balance = invoiceBalance;
        customer.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private static (string Status, string Class) GetInvoiceStatusDisplay(InvoiceStatus status, bool isOverdue)
    {
        if (isOverdue)
            return ("Overdue", "badge-danger");
            
        return status switch
        {
            InvoiceStatus.Draft => ("Draft", "badge-secondary"),
            InvoiceStatus.Sent => ("Sent", "badge-info"),
            InvoiceStatus.PartiallyPaid => ("Partial", "badge-warning"),
            InvoiceStatus.Paid => ("Paid", "badge-success"),
            InvoiceStatus.Overdue => ("Overdue", "badge-danger"),
            InvoiceStatus.Void => ("Void", "badge-secondary"),
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
