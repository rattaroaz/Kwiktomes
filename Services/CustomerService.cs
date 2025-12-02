using Microsoft.EntityFrameworkCore;
using Kwikbooks.Data;
using Kwikbooks.Data.Models;

namespace Kwikbooks.Services;

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
}

/// <summary>
/// Implementation of the customer service.
/// </summary>
public class CustomerService : BaseDataService<Customer>, ICustomerService
{
    public CustomerService(KwikbooksDbContext context) : base(context)
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
}
