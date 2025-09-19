using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sales.Infrastructure.Entities;
using Sales.Infrastructure.ValueObjects;

namespace Sales.Infrastructure.Configure;

public class DataContext : DbContext
{
    private readonly IConfiguration _configuration;

    public DataContext(DbContextOptions<DataContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            
            entity.Property(e => e.OrderId).ValueGeneratedNever();
            entity.Property(e => e.CustomerId).IsRequired();
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.OwnsMany(e => e.Items, items =>
            {
                items.WithOwner().HasForeignKey(oi => oi.OrderId);
                items.ToTable("OrderItems");
                items.Property<int>("Id");
                items.HasKey("Id");

                items.Property(oi => oi.ProductId).IsRequired();
                items.Property(oi => oi.Quantity).IsRequired();
                items.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            });


            entity.HasIndex(e => e.CustomerId);

            entity.HasIndex(e => e.Status);

            entity.HasIndex(e => e.TotalAmount);
                  
            // Composite index for common query patterns
            entity.HasIndex(e => new { e.CustomerId, e.Status });
        });
    }
  
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);
        }
        base.OnConfiguring(optionsBuilder);
    }
}