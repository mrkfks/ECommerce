using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.ToTable("ProductVariants");

            builder.HasKey(pv => pv.Id);

            builder.Property(pv => pv.Sku)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(pv => pv.PriceAdjustment)
                .HasPrecision(18, 2);

            // Product ilişkisi
            builder.HasOne(pv => pv.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(pv => pv.ProductId);
            builder.HasIndex(pv => pv.CompanyId);
            builder.HasIndex(pv => pv.Sku).IsUnique();
        }
    }
}
