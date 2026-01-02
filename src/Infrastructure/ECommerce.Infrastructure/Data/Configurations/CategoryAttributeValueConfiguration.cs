using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class CategoryAttributeValueConfiguration : IEntityTypeConfiguration<CategoryAttributeValue>
{
    public void Configure(EntityTypeBuilder<CategoryAttributeValue> builder)
    {
        builder.HasKey(cav => cav.Id);

        builder.Property(cav => cav.Value)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cav => cav.ColorCode)
            .HasMaxLength(20);

        builder.Property(cav => cav.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(cav => cav.CreatedAt)
            .IsRequired();

        builder.Property(cav => cav.UpdatedAt)
            .IsRequired();

        // Relationship with CategoryAttribute
        builder.HasOne(cav => cav.CategoryAttribute)
            .WithMany(ca => ca.Values)
            .HasForeignKey(cav => cav.CategoryAttributeId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(cav => cav.CategoryAttributeId);
    }
}
