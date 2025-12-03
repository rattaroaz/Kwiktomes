using Microsoft.EntityFrameworkCore;
using Kwiktomes.Data;
using Kwiktomes.Data.Models;

namespace Kwiktomes.Services;

/// <summary>
/// Service for managing products and services.
/// </summary>
public interface IProductService : IDataService<Product>
{
    /// <summary>
    /// Gets all active products.
    /// </summary>
    Task<IEnumerable<Product>> GetActiveProductsAsync();
    
    /// <summary>
    /// Searches products by name, SKU, or description.
    /// </summary>
    Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
    
    /// <summary>
    /// Gets a product by SKU.
    /// </summary>
    Task<Product?> GetBySkuAsync(string sku);
    
    /// <summary>
    /// Gets products by type (Inventory, NonInventory, Service).
    /// </summary>
    Task<IEnumerable<Product>> GetByTypeAsync(ProductType type);
    
    /// <summary>
    /// Gets products by category.
    /// </summary>
    Task<IEnumerable<Product>> GetByCategoryAsync(string category);
    
    /// <summary>
    /// Gets all sellable products (for invoices).
    /// </summary>
    Task<IEnumerable<Product>> GetSellableProductsAsync();
    
    /// <summary>
    /// Gets all purchasable products (for bills).
    /// </summary>
    Task<IEnumerable<Product>> GetPurchasableProductsAsync();
    
    /// <summary>
    /// Gets inventory items that need reordering.
    /// </summary>
    Task<IEnumerable<Product>> GetLowStockProductsAsync();
    
    /// <summary>
    /// Gets all unique categories.
    /// </summary>
    Task<IEnumerable<string>> GetCategoriesAsync();
    
    /// <summary>
    /// Updates inventory quantity.
    /// </summary>
    Task UpdateInventoryAsync(int productId, decimal quantityChange, decimal? newAverageCost = null);
    
    /// <summary>
    /// Gets total inventory value.
    /// </summary>
    Task<decimal> GetTotalInventoryValueAsync();
    
    /// <summary>
    /// Gets count of active products by type.
    /// </summary>
    Task<Dictionary<ProductType, int>> GetProductCountsByTypeAsync();
}

/// <summary>
/// Implementation of the product service.
/// </summary>
public class ProductService : BaseDataService<Product>, IProductService
{
    public ProductService(KwiktomesDbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _dbSet
            .Include(p => p.IncomeAccount)
            .Include(p => p.ExpenseAccount)
            .Include(p => p.InventoryAssetAccount)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _dbSet
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        var term = searchTerm.ToLower();
        return await _dbSet
            .Include(p => p.IncomeAccount)
            .Where(p => p.Name.ToLower().Contains(term) ||
                        (p.Sku != null && p.Sku.ToLower().Contains(term)) ||
                        (p.Description != null && p.Description.ToLower().Contains(term)) ||
                        (p.Category != null && p.Category.ToLower().Contains(term)))
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _dbSet
            .Include(p => p.IncomeAccount)
            .Include(p => p.ExpenseAccount)
            .FirstOrDefaultAsync(p => p.Sku == sku);
    }

    public async Task<IEnumerable<Product>> GetByTypeAsync(ProductType type)
    {
        return await _dbSet
            .Where(p => p.Type == type && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
    {
        return await _dbSet
            .Where(p => p.Category == category && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetSellableProductsAsync()
    {
        return await _dbSet
            .Where(p => p.IsSellable && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetPurchasableProductsAsync()
    {
        return await _dbSet
            .Where(p => p.IsPurchasable && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetLowStockProductsAsync()
    {
        return await _dbSet
            .Where(p => p.Type == ProductType.Inventory && 
                        p.IsActive && 
                        p.QuantityOnHand <= p.ReorderPoint)
            .OrderBy(p => p.QuantityOnHand)
            .ToListAsync();
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _dbSet
            .Where(p => p.Category != null && p.Category != "")
            .Select(p => p.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task UpdateInventoryAsync(int productId, decimal quantityChange, decimal? newAverageCost = null)
    {
        var product = await _dbSet.FindAsync(productId);
        if (product == null || product.Type != ProductType.Inventory) return;

        product.QuantityOnHand += quantityChange;
        
        if (newAverageCost.HasValue)
        {
            product.AverageCost = newAverageCost.Value;
        }
        
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalInventoryValueAsync()
    {
        return await _dbSet
            .Where(p => p.Type == ProductType.Inventory && p.IsActive)
            .SumAsync(p => p.QuantityOnHand * p.AverageCost);
    }

    public async Task<Dictionary<ProductType, int>> GetProductCountsByTypeAsync()
    {
        var counts = await _dbSet
            .Where(p => p.IsActive)
            .GroupBy(p => p.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        return counts.ToDictionary(x => x.Type, x => x.Count);
    }
}
