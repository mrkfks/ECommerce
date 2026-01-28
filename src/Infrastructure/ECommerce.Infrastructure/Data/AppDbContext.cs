using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ECommerce.Infrastructure.Data
{
    public class AppDbContext : DbContext, IApplicationDbContext
    {
        private readonly ITenantService _tenantService;

        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantService tenantService) : base(options)
        {
            _tenantService = tenantService;
        }

        // DbSets
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Model> Models { get; set; }
        public DbSet<ProductSpecification> ProductSpecifications { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductAttribute> Attributes { get; set; }
        public DbSet<AttributeValue> AttributeValues { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductVariantAttribute> ProductVariantAttributes { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<CategoryAttribute> CategoryAttributes { get; set; }
        public DbSet<CategoryAttributeValue> CategoryAttributeValues { get; set; }
        public DbSet<GlobalAttribute> GlobalAttributes { get; set; }
        public DbSet<GlobalAttributeValue> GlobalAttributeValues { get; set; }
        public DbSet<CategoryGlobalAttribute> CategoryGlobalAttributes { get; set; }
        public DbSet<BrandCategory> BrandCategories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CustomerMessage> CustomerMessages { get; set; }
        public DbSet<LoginHistory> LoginHistories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        // Tenant context
        public int? CurrentCompanyId => _tenantService.GetCompanyId();

        public void SetCompanyContext(int companyId)
        {
            // Not: CurrentCompanyId artık dinamik bir property olduğu için bu metod kullanılmıyor
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from assembly automatically
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Seed data - Not: Seed data manuel olarak veya ayrı bir DbInitializer ile eklenecek
            // Seed.DataSeeder.SeedData(modelBuilder);

            // Apply Global Query Filters automatically for all ITenantEntity and ISoftDeletable
            var entityTypes = modelBuilder.Model.GetEntityTypes();

            foreach (var entityType in entityTypes)
            {
                var clrType = entityType.ClrType;

                // ITenantEntity ve ISoftDeletable filtresi
                if (typeof(ITenantEntity).IsAssignableFrom(clrType) && typeof(ISoftDeletable).IsAssignableFrom(clrType))
                {
                    var method = SetGlobalQueryFilterMethod.MakeGenericMethod(clrType);
                    method.Invoke(this, new object[] { modelBuilder });
                }
                // Sadece ITenantEntity filtresi
                else if (typeof(ITenantEntity).IsAssignableFrom(clrType))
                {
                    var method = SetTenantQueryFilterMethod.MakeGenericMethod(clrType);
                    method.Invoke(this, new object[] { modelBuilder });
                }
                // Sadece ISoftDeletable filtresi
                else if (typeof(ISoftDeletable).IsAssignableFrom(clrType))
                {
                    var method = SetSoftDeleteQueryFilterMethod.MakeGenericMethod(clrType);
                    method.Invoke(this, new object[] { modelBuilder });
                }
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                // Soft Delete: Silme işlemlerini DeletedAt ile işaretle
                if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable)
                {
                    entry.State = EntityState.Modified;
                    entry.Property("IsDeleted").CurrentValue = true;
                    entry.Property("DeletedAt").CurrentValue = DateTime.UtcNow;
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }

                // Audit alanlarını otomatik set et
                if (entry.Entity is IAuditable)
                {
                    var now = DateTime.UtcNow;

                    if (entry.State == EntityState.Added)
                    {
                        // CreatedAt ve UpdatedAt'ı reflection ile set et (private setter bypass)
                        entry.Property("CreatedAt").CurrentValue = now;
                        entry.Property("UpdatedAt").CurrentValue = now;

                        // Soft delete için varsayılan değer
                        if (entry.Entity is ISoftDeletable)
                        {
                            entry.Property("IsDeleted").CurrentValue = false;
                            entry.Property("DeletedAt").CurrentValue = null;
                        }
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        // UpdatedAt'ı güncelle
                        entry.Property("UpdatedAt").CurrentValue = now;
                        // CreatedAt değişmesin
                        entry.Property("CreatedAt").IsModified = false;
                    }
                }

                // IsActive default value set et
                if (entry.State == EntityState.Added)
                {
                    try
                    {
                        var entityType = entry.Entity.GetType();
                        var isActiveProperty = entityType.GetProperty("IsActive");

                        if (isActiveProperty != null)
                        {
                            var currentValue = isActiveProperty.GetValue(entry.Entity);

                            // Eğer IsActive false ise veya henüz set edilmemişse, true yap
                            if (currentValue == null || (currentValue is bool boolValue && !boolValue))
                            {
                                isActiveProperty.SetValue(entry.Entity, true);
                            }
                        }
                    }
                    catch
                    {
                        // Entity'nin IsActive property'si yoksa ignore et
                    }
                }

                // Product Version kontrolü (Optimistic Concurrency)
                if (entry.Entity is Product && entry.State == EntityState.Modified)
                {
                    entry.Property("Version").CurrentValue = Guid.NewGuid();
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        #region Global Query Filter Helpers

        private static readonly MethodInfo SetGlobalQueryFilterMethod = typeof(AppDbContext)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(t => t.IsGenericMethod && t.Name == nameof(SetGlobalQueryFilter));

        private static readonly MethodInfo SetTenantQueryFilterMethod = typeof(AppDbContext)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(t => t.IsGenericMethod && t.Name == nameof(SetTenantQueryFilter));

        private static readonly MethodInfo SetSoftDeleteQueryFilterMethod = typeof(AppDbContext)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(t => t.IsGenericMethod && t.Name == nameof(SetSoftDeleteQueryFilter));

        private void SetGlobalQueryFilter<T>(ModelBuilder modelBuilder) where T : class, ITenantEntity, ISoftDeletable
        {
            modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == null || e.CompanyId == CurrentCompanyId));
        }

        private void SetTenantQueryFilter<T>(ModelBuilder modelBuilder) where T : class, ITenantEntity
        {
            modelBuilder.Entity<T>().HasQueryFilter(e => CurrentCompanyId == null || e.CompanyId == CurrentCompanyId);
        }

        private void SetSoftDeleteQueryFilter<T>(ModelBuilder modelBuilder) where T : class, ISoftDeletable
        {
            modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
        }

        #endregion
    }
}

