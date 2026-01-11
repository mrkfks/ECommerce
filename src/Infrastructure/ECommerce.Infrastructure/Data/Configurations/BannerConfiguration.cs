using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Description)
            .HasMaxLength(500);

        builder.Property(b => b.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(b => b.Link)
            .HasMaxLength(500);

        builder.Property(b => b.Order)
            .IsRequired();

        builder.Property(b => b.IsActive)
            .IsRequired();

        builder.Property(b => b.CompanyId)
            .IsRequired();

        builder.HasOne(b => b.Company)
            .WithMany()
            .HasForeignKey(b => b.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(b => !b.IsDeleted);

        builder.HasIndex(b => new { b.CompanyId, b.Order });
    }
}
