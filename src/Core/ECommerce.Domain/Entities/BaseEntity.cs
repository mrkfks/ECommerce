namespace ECommerce.Domain.Entities;

/// <summary>
/// Tüm entity'lerin türeyeceği base sınıf
/// Audit ve Soft Delete mekanizmalarını merkezi olarak yönetir
/// </summary>
public abstract class BaseEntity : IAuditable, ISoftDeletable
{
    public int Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; protected set; }
    public bool IsDeleted { get; protected set; }

    /// <summary>
    /// Soft delete - veriyi veritabanından silmez, sadece işaretler
    /// </summary>
    public virtual void Delete()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Bu kayıt zaten silinmiş.");

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Soft delete'i geri al
    /// </summary>
    public virtual void Restore()
    {
        if (!IsDeleted)
            throw new InvalidOperationException("Bu kayıt zaten aktif.");

        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// UpdatedAt alanını günceller
    /// </summary>
    protected void MarkAsModified()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
