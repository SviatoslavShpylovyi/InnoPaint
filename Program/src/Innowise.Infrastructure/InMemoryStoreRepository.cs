using System.Collections.Concurrent;
using Innowise.Application;
using Innowise.Domain;

namespace Innowise.Infrastructure;

public sealed class InMemoryStoreRepository : IStoreRepository
{
    private readonly ConcurrentDictionary<Guid, Paint> paints = new();
    private readonly ConcurrentDictionary<Guid, Basket> baskets = new();

    public IReadOnlyList<Paint> GetPaints() => paints.Values.ToArray();
    public Paint? GetPaint(Guid id) => paints.GetValueOrDefault(id);
    public void SavePaint(Paint paint) => paints[paint.Id] = paint;
    public bool DeletePaint(Guid id) => paints.TryRemove(id, out _);
    public Basket? GetBasket(Guid id) => baskets.GetValueOrDefault(id);
    public void SaveBasket(Basket basket) => baskets[basket.Id] = basket;
    public bool DeleteBasket(Guid id) => baskets.TryRemove(id, out _);
    public IReadOnlyList<Basket> GetBaskets() => baskets.Values.ToArray();
}
