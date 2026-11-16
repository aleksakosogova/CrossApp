using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ProductRow> Products => Set<ProductRow>();
    public DbSet<BatchRow> Batches => Set<BatchRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var product = modelBuilder.Entity<ProductRow>();
        product.HasKey(p => p.Id);
        product.Property(p => p.Id).HasMaxLength(32);
        product.Property(p => p.Sku).IsRequired().HasMaxLength(32);
        product.Property(p => p.Name).IsRequired().HasMaxLength(200);
        product.Property(p => p.Unit).IsRequired().HasMaxLength(20);
        product.Property(p => p.Description).HasMaxLength(500);
        product.HasIndex(p => p.Sku).IsUnique();

        product.HasMany(p => p.Batches)
               .WithOne(b => b.Product)
               .HasForeignKey(b => b.ProductId)
               .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<BatchRow>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Amount).IsRequired();
            b.Property(x => x.ReceivedAt).IsRequired();
        });

        // Додаткове завдання 2: Початкові дані (Seeder) на порожній таблиці
        product.HasData(
            new ProductRow
            {
                Id = "seed-001",
                Sku = "INIT-SKU-1",
                Name = "Стартовий цемент М500",
                Unit = "мішок",
                Quantity = 50,
                Description = "Додано через HasData seeder"
            }
        );
    }
}