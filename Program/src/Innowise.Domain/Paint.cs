namespace Innowise.Domain;

public sealed record Paint(Guid Id, string Name, string Brand, string Description,
    string Color, string Finish, string Surface, bool IsExterior, decimal VolumeLiters,
    decimal Price, int StockQuantity);

public sealed record BasketItem(Guid PaintId, int Quantity);
public sealed record Basket(Guid Id, IReadOnlyList<BasketItem> Items);
