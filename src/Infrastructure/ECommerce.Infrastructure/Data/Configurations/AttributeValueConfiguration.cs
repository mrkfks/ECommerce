using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
    {
        public void Configure(EntityTypeBuilder<AttributeValue> builder)
        {
            builder.ToTable("AttributeValues");

            builder.HasKey(av => av.Id);

            builder.Property(av => av.Value)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(av => av.ColorCode)
                .HasMaxLength(20);

            // Attribute ilişkisi
            builder.HasOne(av => av.Attribute)
                .WithMany(a => a.Values)
                .HasForeignKey(av => av.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(av => av.AttributeId);
            builder.HasIndex(av => new { av.AttributeId, av.Value }).IsUnique();
        }
    }
}
