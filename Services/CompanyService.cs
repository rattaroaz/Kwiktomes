using Microsoft.EntityFrameworkCore;
using Kwiktomes.Data;
using Kwiktomes.Data.Models;

namespace Kwiktomes.Services;

/// <summary>
/// Service for managing the company profile.
/// Since this is a single-company application, this service provides
/// specialized methods for working with the company settings.
/// </summary>
public interface ICompanyService
{
    /// <summary>
    /// Gets the company profile. Creates a default one if none exists.
    /// </summary>
    Task<Company> GetCompanyAsync();
    
    /// <summary>
    /// Updates the company profile.
    /// </summary>
    Task<Company> UpdateCompanyAsync(Company company);
    
    /// <summary>
    /// Checks if the company profile has been set up.
    /// </summary>
    Task<bool> IsSetupCompleteAsync();
    
    /// <summary>
    /// Marks the company setup as complete.
    /// </summary>
    Task CompleteSetupAsync();
    
    /// <summary>
    /// Gets the next invoice number and increments the counter.
    /// </summary>
    Task<string> GetNextInvoiceNumberAsync();
    
    /// <summary>
    /// Gets the next bill number and increments the counter.
    /// </summary>
    Task<string> GetNextBillNumberAsync();
    
    /// <summary>
    /// Updates the company logo.
    /// </summary>
    Task UpdateLogoAsync(byte[] logoData, string contentType);
    
    /// <summary>
    /// Removes the company logo.
    /// </summary>
    Task RemoveLogoAsync();
}

/// <summary>
/// Implementation of the company service.
/// </summary>
public class CompanyService : ICompanyService
{
    private readonly KwiktomesDbContext _context;
    private Company? _cachedCompany;

    public CompanyService(KwiktomesDbContext context)
    {
        _context = context;
    }

    public async Task<Company> GetCompanyAsync()
    {
        // Return cached company if available
        if (_cachedCompany != null)
            return _cachedCompany;

        // Try to get existing company
        var company = await _context.Companies.FirstOrDefaultAsync();
        
        if (company == null)
        {
            // Create default company profile
            company = new Company
            {
                Name = "My Company",
                Currency = "USD",
                CurrencySymbol = "$",
                FiscalYearStartMonth = 1,
                DefaultPaymentTermsDays = 30,
                InvoicePrefix = "INV-",
                NextInvoiceNumber = 1001,
                BillPrefix = "BILL-",
                NextBillNumber = 1001,
                AccountingMethod = AccountingMethod.Accrual,
                InventoryMethod = InventoryMethod.FIFO,
                BusinessType = BusinessType.SoleProprietorship,
                Industry = IndustryType.Other,
                Country = "USA",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
        }

        _cachedCompany = company;
        return company;
    }

    public async Task<Company> UpdateCompanyAsync(Company company)
    {
        company.UpdatedAt = DateTime.UtcNow;
        _context.Companies.Update(company);
        await _context.SaveChangesAsync();
        
        // Update cache
        _cachedCompany = company;
        
        return company;
    }

    public async Task<bool> IsSetupCompleteAsync()
    {
        var company = await GetCompanyAsync();
        return company.SetupCompletedAt.HasValue;
    }

    public async Task CompleteSetupAsync()
    {
        var company = await GetCompanyAsync();
        company.SetupCompletedAt = DateTime.UtcNow;
        await UpdateCompanyAsync(company);
    }

    public async Task<string> GetNextInvoiceNumberAsync()
    {
        var company = await GetCompanyAsync();
        var invoiceNumber = $"{company.InvoicePrefix}{company.NextInvoiceNumber}";
        
        company.NextInvoiceNumber++;
        await UpdateCompanyAsync(company);
        
        return invoiceNumber;
    }

    public async Task<string> GetNextBillNumberAsync()
    {
        var company = await GetCompanyAsync();
        var billNumber = $"{company.BillPrefix}{company.NextBillNumber}";
        
        company.NextBillNumber++;
        await UpdateCompanyAsync(company);
        
        return billNumber;
    }

    public async Task UpdateLogoAsync(byte[] logoData, string contentType)
    {
        var company = await GetCompanyAsync();
        company.Logo = logoData;
        company.LogoContentType = contentType;
        await UpdateCompanyAsync(company);
    }

    public async Task RemoveLogoAsync()
    {
        var company = await GetCompanyAsync();
        company.Logo = null;
        company.LogoContentType = null;
        await UpdateCompanyAsync(company);
    }
}
