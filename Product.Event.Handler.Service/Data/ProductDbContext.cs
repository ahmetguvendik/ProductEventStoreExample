using Microsoft.EntityFrameworkCore;
using ProductEntity = Product.Event.Handler.Service.Models.Product;

namespace Product.Event.Handler.Service.Data;

public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
    public DbSet<ProductEntity> Products => Set<ProductEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductEntity>(builder =>
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasMaxLength(64).IsRequired().ValueGeneratedNever();
            builder.HasIndex(p => p.Id).IsUnique();
            builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
            builder.Property(p => p.Description).HasMaxLength(500);
            builder.Property(p => p.Price).HasPrecision(18, 2);
            builder.Property(p => p.CreatedAt).HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
            builder.Property(p => p.IsActive).HasDefaultValue(true);
        });
    }
}

