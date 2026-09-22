using Innowise.Application;
using Innowise.Domain;
using Microsoft.EntityFrameworkCore;

namespace Innowise.Infrastructure.Persistence;

// Singleton like PaintStoreService, so each call uses a short-lived context from the factory.
public sealed class EfStoreRepository(IDbContextFactory<InnoPaintDbContext> factory) : IStoreRepository
{
    public IReadOnlyList<Paint> GetPaints()
    {
        using var db = factory.CreateDbContext();
        return db.Paints.AsNoTracking().ToArray();
    }

    public Paint? GetPaint(Guid id)
    {
        using var db = factory.CreateDbContext();
        return db.Paints.AsNoTracking().FirstOrDefault(p => p.Id == id);
    }

    public void SavePaint(Paint paint)
    {
        using var db = factory.CreateDbContext();
        if (db.Paints.Any(p => p.Id == paint.Id)) db.Paints.Update(paint);
        else db.Paints.Add(paint);
        db.SaveChanges();
    }

    public bool DeletePaint(Guid id)
    {
        using var db = factory.CreateDbContext();
        return db.Paints.Where(p => p.Id == id).ExecuteDelete() > 0;
    }

    public Basket? GetBasket(Guid id)
    {
        using var db = factory.CreateDbContext();
        var basket = db.Baskets.AsNoTracking().Include(b => b.Items).FirstOrDefault(b => b.Id == id);
        return basket is null ? null : ToBasket(basket);
    }

    public void SaveBasket(Basket basket)
    {
        using var db = factory.CreateDbContext();
        using var transaction = db.Database.BeginTransaction();
        if (!db.Baskets.Any(b => b.Id == basket.Id)) db.Baskets.Add(new BasketEntity { Id = basket.Id });
        db.BasketItems.Where(i => i.BasketId == basket.Id).ExecuteDelete();
        db.BasketItems.AddRange(basket.Items.Select((item, index) => new BasketItemEntity
        {
            BasketId = basket.Id, PaintId = item.PaintId, Quantity = item.Quantity, Position = index
        }));
        db.SaveChanges();
        transaction.Commit();
    }

    public bool DeleteBasket(Guid id)
    {
        using var db = factory.CreateDbContext();
        return db.Baskets.Where(b => b.Id == id).ExecuteDelete() > 0;
    }

    public IReadOnlyList<Basket> GetBaskets()
    {
        using var db = factory.CreateDbContext();
        return db.Baskets.AsNoTracking().Include(b => b.Items).AsEnumerable().Select(ToBasket).ToArray();
    }

    private static Basket ToBasket(BasketEntity basket) => new(basket.Id,
        basket.Items.OrderBy(i => i.Position).Select(i => new BasketItem(i.PaintId, i.Quantity)).ToArray());
}
