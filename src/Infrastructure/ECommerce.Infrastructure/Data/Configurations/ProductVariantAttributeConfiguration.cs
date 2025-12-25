using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class ProductVariantAttributeConfiguration : IEntityTypeConfiguration<ProductVariantAttribute>
    {
        public void Configure(EntityTypeBuilder<ProductVariantAttribute> builder)
        {
            builder.ToTable("ProductVariantAttributes");

            builder.HasKey(pva => pva.Id);

            // ProductVariant ilişkisi
            builder.HasOne(pva => pva.ProductVariant)
                .WithMany(pv => pv.VariantAttributes)
                .HasForeignKey(pva => pva.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            // AttributeValue ilişkisi
            builder.HasOne(pva => pva.AttributeValue)
                .WithMany(av => av.VariantAttributes)
                .HasForeignKey(pva => pva.AttributeValueId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(pva => pva.ProductVariantId);
            builder.HasIndex(pva => pva.AttributeValueId);
            builder.HasIndex(pva => new { pva.ProductVariantId, pva.AttributeValueId }).IsUnique();
        }
    }
}
