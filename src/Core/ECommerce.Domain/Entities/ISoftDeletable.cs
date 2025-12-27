namespace ECommerce.Domain.Entities;

/// <summary>
/// Soft delete desteği olan entity'ler için interface
/// </summary>
public interface ISoftDeletable
{
    DateTime? DeletedAt { get; }
    bool IsDeleted { get; }

    void Delete();
    void Restore();
}
