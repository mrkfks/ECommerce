using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(b => b.ImageUrl)
            .HasMaxLength(500);

        builder.Property(b => b.CompanyId)
            .IsRequired();

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .IsRequired();

        // Relationship with Category
        builder.HasOne(b => b.Category)
            .WithMany(c => c.Brands)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(b => b.Name);
        builder.HasIndex(b => b.CompanyId);
        builder.HasIndex(b => b.CategoryId);
    }
}
