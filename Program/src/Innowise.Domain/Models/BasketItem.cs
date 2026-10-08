namespace Innowise.Domain.Models;

public class BasketItem
{
    public Guid BasketId { get; private set; }

    public Guid PaintId { get; private set; }

    public int Quantity { get; private set; }

    public int Position { get; private set; }

    private BasketItem()
    {
    }

    private BasketItem(Guid basketId, Guid paintId)
    {
        BasketId = basketId;
        PaintId = paintId;
    }

    public static BasketItem Create(Guid basketId, Guid paintId, int quantity, int position)
    {
        var item = new BasketItem(basketId, paintId);

        item.SetQuantity(quantity);
        item.SetPosition(position);

        return item;
    }

    public void SetQuantity(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        Quantity = quantity;
    }

    public void SetPosition(int position)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position);

        Position = position;
    }
}
