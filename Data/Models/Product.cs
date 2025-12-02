using System.ComponentModel.DataAnnotations;

namespace Kwikbooks.Data.Models;

/// <summary>
/// Represents a product or service that can be sold or purchased.
/// </summary>
public class Product : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Stock Keeping Unit - unique identifier for inventory
    /// </summary>
    [MaxLength(50)]
    public string? Sku { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Product type: Inventory, Non-Inventory, or Service
    /// </summary>
    public ProductType Type { get; set; } = ProductType.Service;
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    [MaxLength(50)]
    public string? UnitOfMeasure { get; set; } = "Each";
    
    // Sales Information
    /// <summary>
    /// Is this item available for sale to customers?
    /// </summary>
    public bool IsSellable { get; set; } = true;
    
    [MaxLength(1000)]
    public string? SalesDescription { get; set; }
    
    public decimal SalesPrice { get; set; } = 0m;
    
    public int? IncomeAccountId { get; set; }
    public Account? IncomeAccount { get; set; }
    
    // Purchase Information
    /// <summary>
    /// Is this item available for purchase from vendors?
    /// </summary>
    public bool IsPurchasable { get; set; } = true;
    
    [MaxLength(1000)]
    public string? PurchaseDescription { get; set; }
    
    public decimal PurchaseCost { get; set; } = 0m;
    
    public int? ExpenseAccountId { get; set; }
    public Account? ExpenseAccount { get; set; }
    
    /// <summary>
    /// Preferred vendor for this product
    /// </summary>
    public int? PreferredVendorId { get; set; }
    
    // Inventory Tracking (for Inventory type products)
    public decimal QuantityOnHand { get; set; } = 0m;
    
    /// <summary>
    /// Quantity at which to reorder
    /// </summary>
    public decimal ReorderPoint { get; set; } = 0m;
    
    /// <summary>
    /// Quantity to order when reordering
    /// </summary>
    public decimal ReorderQuantity { get; set; } = 0m;
    
    /// <summary>
    /// Current average cost per unit (for inventory valuation)
    /// </summary>
    public decimal AverageCost { get; set; } = 0m;
    
    public int? InventoryAssetAccountId { get; set; }
    public Account? InventoryAssetAccount { get; set; }
    
    /// <summary>
    /// Date of last inventory count
    /// </summary>
    public DateTime? LastInventoryDate { get; set; }
    
    // Tax Settings
    public bool IsTaxable { get; set; } = true;
    
    /// <summary>
    /// Custom tax rate (null = use default)
    /// </summary>
    public decimal? TaxRate { get; set; }
    
    // Status
    public bool IsActive { get; set; } = true;
    
    // Computed Properties
    public bool IsInventoryItem => Type == ProductType.Inventory;
    public bool IsService => Type == ProductType.Service;
    
    public decimal InventoryValue => QuantityOnHand * AverageCost;
    
    public decimal ProfitMargin => SalesPrice > 0 && PurchaseCost > 0 
        ? ((SalesPrice - PurchaseCost) / SalesPrice) * 100 
        : 0;
    
    public bool NeedsReorder => IsInventoryItem && QuantityOnHand <= ReorderPoint;
    
    public string TypeDisplay => Type switch
    {
        ProductType.Inventory => "Inventory",
        ProductType.NonInventory => "Non-Inventory",
        ProductType.Service => "Service",
        _ => "Unknown"
    };
}

/// <summary>
/// Type of product
/// </summary>
public enum ProductType
{
    /// <summary>
    /// Physical product with inventory tracking
    /// </summary>
    Inventory = 1,
    
    /// <summary>
    /// Physical product without inventory tracking
    /// </summary>
    NonInventory = 2,
    
    /// <summary>
    /// Service (no physical product)
    /// </summary>
    Service = 3
}
