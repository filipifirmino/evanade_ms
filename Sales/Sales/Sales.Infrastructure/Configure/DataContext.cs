using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sales.Infrastructure.Entities;

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