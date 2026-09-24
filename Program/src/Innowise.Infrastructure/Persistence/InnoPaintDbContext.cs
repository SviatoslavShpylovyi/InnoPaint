using Innowise.Domain;
using Microsoft.EntityFrameworkCore;

namespace Innowise.Infrastructure.Persistence;

public sealed class InnoPaintDbContext(DbContextOptions<InnoPaintDbContext> options) : DbContext(options)
{
    public DbSet<Paint> Paints => Set<Paint>();
    public DbSet<BasketEntity> Baskets => Set<BasketEntity>();
    public DbSet<BasketItemEntity> BasketItems => Set<BasketItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Paint>(paint =>
        {
            paint.HasKey(p => p.Id);
            paint.Property(p => p.Name).HasMaxLength(150);
            paint.Property(p => p.Brand).HasMaxLength(100);
            paint.Property(p => p.Description).HasMaxLength(4000);
            paint.Property(p => p.Color).HasMaxLength(80);
            paint.Property(p => p.Finish).HasMaxLength(80);
            paint.Property(p => p.Surface).HasMaxLength(80);
            paint.Property(p => p.VolumeLiters).HasPrecision(10, 3);
            paint.Property(p => p.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<BasketEntity>(basket =>
        {
            basket.ToTable("Baskets");
            basket.HasKey(b => b.Id);
            basket.HasMany(b => b.Items).WithOne().HasForeignKey(i => i.BasketId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BasketItemEntity>(item =>
        {
            item.ToTable("BasketItems");
            item.HasKey(i => new { i.BasketId, i.PaintId });
            // The store service removes a paint from baskets before deleting it; the FK guards that invariant.
            item.HasOne<Paint>().WithMany().HasForeignKey(i => i.PaintId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}

public sealed class BasketEntity
{
    public Guid Id { get; set; }
    public List<BasketItemEntity> Items { get; set; } = [];
}

public sealed class BasketItemEntity
{
    public Guid BasketId { get; set; }
    public Guid PaintId { get; set; }
    public int Quantity { get; set; }
    public int Position { get; set; }
}
