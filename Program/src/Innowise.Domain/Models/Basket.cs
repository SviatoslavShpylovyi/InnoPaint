namespace Innowise.Domain.Models;

public class Basket
{
    private readonly List<BasketItem> _items = [];

    public Guid Id { get; private set; }

    public IReadOnlyList<BasketItem> Items => [.. _items.OrderBy(item => item.Position)];

    private Basket()
    {
    }

    private Basket(Guid id)
    {
        Id = id;
    }

    public static Basket Create()
    {
        return new Basket(Guid.NewGuid());
    }

    public void SetItemQuantity(Guid paintId, int quantity)
    {
        var existing = _items.Find(item => item.PaintId == paintId);

        if (existing is null)
        {
            _items.Add(BasketItem.Create(Id, paintId, quantity, NextPosition()));

            return;
        }

        existing.SetQuantity(quantity);
    }

    public bool RemoveItem(Guid paintId)
    {
        return _items.RemoveAll(item => item.PaintId == paintId) > 0;
    }

    public void Clear()
    {
        _items.Clear();
    }

    public int QuantityOf(Guid paintId)
    {
        return _items.Find(item => item.PaintId == paintId)?.Quantity ?? 0;
    }

    private int NextPosition()
    {
        return _items.Count == 0 ? 0 : _items.Max(item => item.Position) + 1;
    }
}
