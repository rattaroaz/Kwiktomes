using System.ComponentModel.DataAnnotations;

namespace Kwiktomes.Data.Models;

/// <summary>
/// Represents the company/business profile.
/// This is the primary business entity for the application.
/// </summary>
public class Company : BaseEntity
{
    // Basic Information
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? LegalName { get; set; }
    
    [MaxLength(200)]
    public string? DbaName { get; set; } // "Doing Business As" name
    
    [MaxLength(50)]
    public string? TaxId { get; set; } // EIN/Tax ID
    
    public BusinessType BusinessType { get; set; } = BusinessType.SoleProprietorship;
    
    public IndustryType Industry { get; set; } = IndustryType.Other;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    // Primary Business Address
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
    
    // Contact Information
    [MaxLength(30)]
    public string? Phone { get; set; }
    
    [MaxLength(30)]
    public string? Fax { get; set; }
    
    [MaxLength(254)]
    [EmailAddress]
    public string? Email { get; set; }
    
    [MaxLength(200)]
    [Url]
    public string? Website { get; set; }
    
    // Financial Settings
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";
    
    [MaxLength(5)]
    public string CurrencySymbol { get; set; } = "$";
    
    /// <summary>
    /// First month of fiscal year (1=January, 12=December)
    /// </summary>
    [Range(1, 12)]
    public int FiscalYearStartMonth { get; set; } = 1;
    
    /// <summary>
    /// Income tax year same as fiscal year
    /// </summary>
    public bool TaxYearSameAsFiscal { get; set; } = true;
    
    /// <summary>
    /// Default sales tax rate (as percentage, e.g., 8.25 for 8.25%)
    /// </summary>
    [Range(0, 100)]
    public decimal DefaultTaxRate { get; set; } = 0m;
    
    [MaxLength(100)]
    public string? TaxAgencyName { get; set; }
    
    /// <summary>
    /// Track inventory using FIFO, LIFO, or Average Cost
    /// </summary>
    public InventoryMethod InventoryMethod { get; set; } = InventoryMethod.FIFO;
    
    /// <summary>
    /// Accounting method: Cash or Accrual
    /// </summary>
    public AccountingMethod AccountingMethod { get; set; } = AccountingMethod.Accrual;
    
    // Invoice Settings
    [MaxLength(10)]
    public string InvoicePrefix { get; set; } = "INV-";
    
    public int NextInvoiceNumber { get; set; } = 1001;
    
    [MaxLength(10)]
    public string BillPrefix { get; set; } = "BILL-";
    
    public int NextBillNumber { get; set; } = 1001;
    
    /// <summary>
    /// Default payment terms in days
    /// </summary>
    public int DefaultPaymentTermsDays { get; set; } = 30;
    
    [MaxLength(1000)]
    public string? InvoiceNotes { get; set; }
    
    [MaxLength(1000)]
    public string? InvoiceTerms { get; set; }
    
    // Branding
    public byte[]? Logo { get; set; }
    
    [MaxLength(10)]
    public string? LogoContentType { get; set; }
    
    [MaxLength(7)]
    public string PrimaryColor { get; set; } = "#2ca01c"; // Kwiktomes green
    
    [MaxLength(7)]
    public string SecondaryColor { get; set; } = "#1a1d21";
    
    // Subscription/Setup
    public DateTime? SetupCompletedAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Computed Properties
    public string FullAddress => string.Join(", ", 
        new[] { Address1, Address2, City, State, PostalCode, Country }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
    
    public string DisplayName => !string.IsNullOrWhiteSpace(DbaName) ? DbaName : Name;
    
    public string FiscalYearDisplay => FiscalYearStartMonth switch
    {
        1 => "January - December",
        2 => "February - January",
        3 => "March - February",
        4 => "April - March",
        5 => "May - April",
        6 => "June - May",
        7 => "July - June",
        8 => "August - July",
        9 => "September - August",
        10 => "October - September",
        11 => "November - October",
        12 => "December - November",
        _ => "January - December"
    };
}

/// <summary>
/// Type of business entity
/// </summary>
public enum BusinessType
{
    SoleProprietorship = 1,
    Partnership = 2,
    LLC = 3,
    SCorporation = 4,
    CCorporation = 5,
    NonProfit = 6,
    Other = 99
}

/// <summary>
/// Industry categories for the business
/// </summary>
public enum IndustryType
{
    Accounting = 1,
    Advertising = 2,
    Agriculture = 3,
    Automotive = 4,
    Construction = 5,
    Consulting = 6,
    Education = 7,
    Entertainment = 8,
    FinancialServices = 9,
    FoodAndBeverage = 10,
    Healthcare = 11,
    Hospitality = 12,
    InformationTechnology = 13,
    Insurance = 14,
    Legal = 15,
    Manufacturing = 16,
    Marketing = 17,
    NonProfit = 18,
    ProfessionalServices = 19,
    RealEstate = 20,
    Retail = 21,
    Transportation = 22,
    Utilities = 23,
    Wholesale = 24,
    Other = 99
}

/// <summary>
/// Inventory valuation methods
/// </summary>
public enum InventoryMethod
{
    FIFO = 1,      // First In, First Out
    LIFO = 2,      // Last In, First Out
    AverageCost = 3
}

/// <summary>
/// Accounting method for recognizing income/expenses
/// </summary>
public enum AccountingMethod
{
    Cash = 1,      // Record when money changes hands
    Accrual = 2    // Record when transaction occurs
}
