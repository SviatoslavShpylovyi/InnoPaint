using Microsoft.EntityFrameworkCore;

using Innowise.Domain.Models;

namespace Innowise.Infrastructure.Data;

public class InnoPaintDbContext : DbContext
{
    public InnoPaintDbContext(DbContextOptions<InnoPaintDbContext> options)
        : base(options)
    {
    }

    public DbSet<Paint> Paints => Set<Paint>();

    public DbSet<Basket> Baskets => Set<Basket>();

    public DbSet<BasketItem> BasketItems => Set<BasketItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        ConfigurePaint(modelBuilder);
        ConfigureBasket(modelBuilder);
        ConfigureBasketItem(modelBuilder);
    }

    private static void ConfigurePaint(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Paint>(entity =>
        {
            entity.HasKey(paint => paint.Id);
            entity.Property(paint => paint.Name).HasMaxLength(150).IsRequired();
            entity.Property(paint => paint.Brand).HasMaxLength(100).IsRequired();
            entity.Property(paint => paint.Description).HasMaxLength(4000).IsRequired();
            entity.Property(paint => paint.Color).HasMaxLength(80).IsRequired();
            entity.Property(paint => paint.Finish).HasMaxLength(80).IsRequired();
            entity.Property(paint => paint.Surface).HasMaxLength(80).IsRequired();
            entity.Property(paint => paint.VolumeLiters).HasPrecision(10, 3);
            entity.Property(paint => paint.Price).HasPrecision(18, 2);
        });
    }

    private static void ConfigureBasket(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Basket>(entity =>
        {
            entity.ToTable("Baskets");
            entity.HasKey(basket => basket.Id);

            entity.HasMany(basket => basket.Items)
                .WithOne()
                .HasForeignKey(item => item.BasketId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(basket => basket.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }

    private static void ConfigureBasketItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BasketItem>(entity =>
        {
            entity.ToTable("BasketItems");
            entity.HasKey(item => new { item.BasketId, item.PaintId });

            entity.HasOne<Paint>()
                .WithMany()
                .HasForeignKey(item => item.PaintId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
