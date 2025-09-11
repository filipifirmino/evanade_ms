using Inventory.InfraStructure.Entitys;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Inventory.InfraStructure.Configure;

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
    
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.HasKey(e => e.ProductId);
    
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
    
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18,2)");
    
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Price);
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