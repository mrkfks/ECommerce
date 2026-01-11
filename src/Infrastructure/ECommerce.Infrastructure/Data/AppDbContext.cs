using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

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

        // Tenant context - TEST: sabit değer döndür
        public int? CurrentCompanyId => 2;  // TEST için sabit CompanyId=2
        public void SetCompanyContext(int companyId)
        {
            // Not: CurrentCompanyId artık dinamik bir property olduğu için bu metod kullanılmıyor
            // Ancak IApplicationDbContext interface'i için tanımlanmış olabilir
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from assembly automatically
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Seed data - Not: Seed data manuel olarak veya ayrı bir DbInitializer ile eklenecek
            // Seed.DataSeeder.SeedData(modelBuilder);

            // Global Query Filters (tenant izolasyonu + soft delete)
            modelBuilder.Entity<User>()
                .HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == null || e.CompanyId == CurrentCompanyId));
            modelBuilder.Entity<Customer>()
                .HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == null || e.CompanyId == CurrentCompanyId));
            modelBuilder.Entity<Order>()
                .HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == null || e.CompanyId == CurrentCompanyId));
            modelBuilder.Entity<Product>()
                .HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == null || e.CompanyId == CurrentCompanyId));
            modelBuilder.Entity<Category>()
                .HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == null || e.CompanyId == CurrentCompanyId));
            modelBuilder.Entity<Review>()
                .HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == null || e.CompanyId == CurrentCompanyId));
            modelBuilder.Entity<ProductSpecification>()
                .HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == null || e.CompanyId == CurrentCompanyId));
            modelBuilder.Entity<ProductVariant>()
                .HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == null || e.CompanyId == CurrentCompanyId));

            // Brand ve Model query filters
            modelBuilder.Entity<Brand>()
                .HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == null || e.CompanyId == CurrentCompanyId));
            modelBuilder.Entity<Model>()
                .HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == null || e.CompanyId == CurrentCompanyId));

            // Global attribute entities
            modelBuilder.Entity<GlobalAttribute>()
                .HasQueryFilter(e => CurrentCompanyId == null || e.CompanyId == CurrentCompanyId);
            modelBuilder.Entity<GlobalAttributeValue>()
                .HasQueryFilter(e => e.GlobalAttribute == null || CurrentCompanyId == null || e.GlobalAttribute.CompanyId == CurrentCompanyId);
            modelBuilder.Entity<CategoryGlobalAttribute>()
                .HasQueryFilter(e => CurrentCompanyId == null || e.Category.CompanyId == CurrentCompanyId);
            modelBuilder.Entity<BrandCategory>()
                .HasQueryFilter(e => CurrentCompanyId == null || e.Brand.CompanyId == CurrentCompanyId);

            // Notification query filter
            modelBuilder.Entity<Notification>()
                .HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == null || e.CompanyId == CurrentCompanyId));

            // Campaign query filter
            modelBuilder.Entity<Campaign>()
                .HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == null || e.CompanyId == CurrentCompanyId));

            // CustomerMessage query filter
            modelBuilder.Entity<CustomerMessage>()
                .HasQueryFilter(e => !e.IsDeleted && (CurrentCompanyId == null || e.CompanyId == CurrentCompanyId));

            // LoginHistory - kullanıcının şirketine göre filtrelenmez (admin tüm girişleri görebilir)
            modelBuilder.Entity<LoginHistory>()
                .HasQueryFilter(e => !e.IsDeleted);

            // Child entity query filters - Parent ile uyumlu olmalı
            // Address -> Customer ilişkisi için filter
            modelBuilder.Entity<Address>()
                .HasQueryFilter(e => e.Customer == null || (!e.Customer.IsDeleted && (CurrentCompanyId == null || e.Customer.CompanyId == CurrentCompanyId)));

            // OrderItem -> Order ilişkisi için filter
            modelBuilder.Entity<OrderItem>()
                .HasQueryFilter(e => e.Order == null || (!e.Order.IsDeleted && (CurrentCompanyId == null || e.Order.CompanyId == CurrentCompanyId)));

            // UserRole -> User ilişkisi için filter
            modelBuilder.Entity<UserRole>()
                .HasQueryFilter(e => e.User == null || (!e.User.IsDeleted && (CurrentCompanyId == null || e.User.CompanyId == CurrentCompanyId)));

            // CategoryAttribute -> Category ilişkisi için filter
            modelBuilder.Entity<CategoryAttribute>()
                .HasQueryFilter(e => e.Category == null || (!e.Category.IsDeleted && (CurrentCompanyId == null || e.Category.CompanyId == CurrentCompanyId)));

            // ProductVariantAttribute -> ProductVariant ilişkisi için filter
            modelBuilder.Entity<ProductVariantAttribute>()
                .HasQueryFilter(e => e.ProductVariant == null || (!e.ProductVariant.IsDeleted && (CurrentCompanyId == null || e.ProductVariant.CompanyId == CurrentCompanyId)));

            // CategoryAttributeValue -> CategoryAttribute -> Category ilişkisi için filter
            modelBuilder.Entity<CategoryAttributeValue>()
                .HasQueryFilter(e => e.CategoryAttribute == null || e.CategoryAttribute.Category == null || 
                    (!e.CategoryAttribute.Category.IsDeleted && (CurrentCompanyId == null || e.CategoryAttribute.Category.CompanyId == CurrentCompanyId)));
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
    }
}

