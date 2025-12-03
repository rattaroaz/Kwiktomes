namespace Kwiktomes.Data.Models;

/// <summary>
/// Base entity class for all database models.
/// Provides common properties like Id and audit fields.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
