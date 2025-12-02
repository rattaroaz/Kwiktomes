using Microsoft.EntityFrameworkCore;
using Kwikbooks.Data;
using Kwikbooks.Data.Models;

namespace Kwikbooks.Services;

/// <summary>
/// Base implementation of data service providing common CRUD operations.
/// </summary>
/// <typeparam name="T">The entity type inheriting from BaseEntity</typeparam>
public abstract class BaseDataService<T> : IDataService<T> where T : BaseEntity
{
    protected readonly KwikbooksDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected BaseDataService(KwikbooksDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _dbSet.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null)
            return false;

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
