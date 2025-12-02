using Microsoft.EntityFrameworkCore;
using Kwikbooks.Data;
using Kwikbooks.Data.Models;

namespace Kwikbooks.Services;

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
}
