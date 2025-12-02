using System.ComponentModel.DataAnnotations;

namespace Kwikbooks.Data.Models;

/// <summary>
/// Represents a vendor/supplier (someone you buy from).
/// </summary>
public class Vendor : BaseEntity
{
    /// <summary>
    /// Auto-generated vendor number (e.g., VEND-0001)
    /// </summary>
    [MaxLength(20)]
    public string VendorNumber { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? CompanyName { get; set; }
    
    [MaxLength(200)]
    public string? DisplayName { get; set; }
    
    // Primary Contact
    [MaxLength(200)]
    public string? ContactName { get; set; }
    
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    [MaxLength(100)]
    public string? LastName { get; set; }
    
    [MaxLength(254)]
    [EmailAddress]
    public string? Email { get; set; }
    
    [MaxLength(30)]
    public string? Phone { get; set; }
    
    [MaxLength(30)]
    public string? Mobile { get; set; }
    
    [MaxLength(30)]
    public string? Fax { get; set; }
    
    [MaxLength(200)]
    public string? Website { get; set; }
    
    // Address
    [MaxLength(200)]
    public string? Address1 { get; set; }
    
    [MaxLength(200)]
    public string? Address2 { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    [MaxLength(100)]
    public string? State { get; set; }
    
    [MaxLength(20)]
    public string? PostalCode { get; set; }
    
    [MaxLength(100)]
    public string? Country { get; set; } = "USA";
    
    // Financial Settings
    /// <summary>
    /// Current outstanding balance (money owed to vendor)
    /// </summary>
    public decimal Balance { get; set; } = 0m;
    
    /// <summary>
    /// Your account number with this vendor
    /// </summary>
    [MaxLength(50)]
    public string? AccountNumber { get; set; }
    
    [MaxLength(50)]
    public string PaymentTerms { get; set; } = "Net 30";
    
    public int PaymentTermsDays { get; set; } = 30;
    
    /// <summary>
    /// Vendor's Tax ID (for 1099 reporting)
    /// </summary>
    [MaxLength(50)]
    public string? TaxId { get; set; }
    
    /// <summary>
    /// Is this vendor eligible for 1099?
    /// </summary>
    public bool IsEligibleFor1099 { get; set; } = false;
    
    /// <summary>
    /// Default expense account for purchases from this vendor
    /// </summary>
    public int? DefaultExpenseAccountId { get; set; }
    public Account? DefaultExpenseAccount { get; set; }
    
    // Status & Notes
    public bool IsActive { get; set; } = true;
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    // Computed Properties
    public string FullName => !string.IsNullOrWhiteSpace(DisplayName) 
        ? DisplayName 
        : (!string.IsNullOrWhiteSpace(CompanyName) ? CompanyName : Name);
    
    public string AddressFull => string.Join(", ",
        new[] { Address1, Address2, City, State, PostalCode }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
    
    public string Initials => string.IsNullOrWhiteSpace(Name) 
        ? "?" 
        : string.Concat(Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(n => n[0]))
            .ToUpper();
}
