using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class CategoryAttributeConfiguration : IEntityTypeConfiguration<CategoryAttribute>
{
    public void Configure(EntityTypeBuilder<CategoryAttribute> builder)
    {
        builder.HasKey(ca => ca.Id);

        builder.Property(ca => ca.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ca => ca.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ca => ca.IsRequired)
            .IsRequired();

        builder.Property(ca => ca.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ca => ca.CreatedAt)
            .IsRequired();

        builder.Property(ca => ca.UpdatedAt)
            .IsRequired();

        // Relationship with Category
        builder.HasOne(ca => ca.Category)
            .WithMany(c => c.Attributes)
            .HasForeignKey(ca => ca.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ca => ca.CategoryId);
        builder.HasIndex(ca => new { ca.CategoryId, ca.Name }).IsUnique();
    }
}
