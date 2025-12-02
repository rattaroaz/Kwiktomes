using System.ComponentModel.DataAnnotations;

namespace Kwikbooks.Data.Models;

/// <summary>
/// Represents a customer (someone you sell to).
/// </summary>
public class Customer : BaseEntity
{
    /// <summary>
    /// Auto-generated customer number (e.g., CUST-0001)
    /// </summary>
    [MaxLength(20)]
    public string CustomerNumber { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? CompanyName { get; set; }
    
    /// <summary>
    /// How this customer appears on invoices (defaults to Name if empty)
    /// </summary>
    [MaxLength(200)]
    public string? DisplayName { get; set; }
    
    // Primary Contact
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
    
    [MaxLength(200)]
    public string? Website { get; set; }
    
    // Billing Address
    [MaxLength(200)]
    public string? BillingAddress1 { get; set; }
    
    [MaxLength(200)]
    public string? BillingAddress2 { get; set; }
    
    [MaxLength(100)]
    public string? BillingCity { get; set; }
    
    [MaxLength(100)]
    public string? BillingState { get; set; }
    
    [MaxLength(20)]
    public string? BillingPostalCode { get; set; }
    
    [MaxLength(100)]
    public string? BillingCountry { get; set; } = "USA";
    
    // Shipping Address
    public bool ShippingSameAsBilling { get; set; } = true;
    
    [MaxLength(200)]
    public string? ShippingAddress1 { get; set; }
    
    [MaxLength(200)]
    public string? ShippingAddress2 { get; set; }
    
    [MaxLength(100)]
    public string? ShippingCity { get; set; }
    
    [MaxLength(100)]
    public string? ShippingState { get; set; }
    
    [MaxLength(20)]
    public string? ShippingPostalCode { get; set; }
    
    [MaxLength(100)]
    public string? ShippingCountry { get; set; }
    
    // Financial Settings
    /// <summary>
    /// Current outstanding balance (money owed by customer)
    /// </summary>
    public decimal Balance { get; set; } = 0m;
    
    /// <summary>
    /// Maximum credit allowed for this customer
    /// </summary>
    public decimal CreditLimit { get; set; } = 0m;
    
    /// <summary>
    /// Default payment terms (e.g., "Net 30", "Due on Receipt")
    /// </summary>
    [MaxLength(50)]
    public string PaymentTerms { get; set; } = "Net 30";
    
    /// <summary>
    /// Payment terms in days (used for due date calculation)
    /// </summary>
    public int PaymentTermsDays { get; set; } = 30;
    
    /// <summary>
    /// Custom tax rate for this customer (overrides default)
    /// </summary>
    public decimal? TaxRate { get; set; }
    
    public bool IsTaxExempt { get; set; } = false;
    
    [MaxLength(50)]
    public string? TaxExemptNumber { get; set; }
    
    /// <summary>
    /// Default income account for this customer's transactions
    /// </summary>
    public int? DefaultIncomeAccountId { get; set; }
    
    // Status & Notes
    public bool IsActive { get; set; } = true;
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    // Computed Properties
    public string FullName => !string.IsNullOrWhiteSpace(DisplayName) 
        ? DisplayName 
        : (!string.IsNullOrWhiteSpace(CompanyName) ? CompanyName : Name);
    
    public string BillingAddressFull => string.Join(", ",
        new[] { BillingAddress1, BillingAddress2, BillingCity, BillingState, BillingPostalCode }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
    
    public string ShippingAddressFull => ShippingSameAsBilling 
        ? BillingAddressFull 
        : string.Join(", ",
            new[] { ShippingAddress1, ShippingAddress2, ShippingCity, ShippingState, ShippingPostalCode }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
    
    public string Initials => string.IsNullOrWhiteSpace(Name) 
        ? "?" 
        : string.Concat(Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(n => n[0]))
            .ToUpper();
}
