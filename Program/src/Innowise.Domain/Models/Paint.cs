namespace Innowise.Domain.Models;

public class Paint
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string Brand { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public string Color { get; private set; } = null!;

    public string Finish { get; private set; } = null!;

    public string Surface { get; private set; } = null!;

    public bool IsExterior { get; private set; }

    public decimal VolumeLiters { get; private set; }

    public decimal Price { get; private set; }

    public int StockQuantity { get; private set; }

    private Paint()
    {
    }

    private Paint(Guid id)
    {
        Id = id;
    }

    public static Paint Create(
        string name,
        string brand,
        string description,
        string color,
        string finish,
        string surface,
        bool isExterior,
        decimal volumeLiters,
        decimal price,
        int stockQuantity)
    {
        var paint = new Paint(Guid.NewGuid());

        paint.SetDetails(name, brand, description, color, finish, surface, isExterior, volumeLiters, price, stockQuantity);

        return paint;
    }

    public void SetDetails(
        string name,
        string brand,
        string description,
        string color,
        string finish,
        string surface,
        bool isExterior,
        decimal volumeLiters,
        decimal price,
        int stockQuantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(brand);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(color);
        ArgumentException.ThrowIfNullOrWhiteSpace(finish);
        ArgumentException.ThrowIfNullOrWhiteSpace(surface);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(volumeLiters);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);
        ArgumentOutOfRangeException.ThrowIfNegative(stockQuantity);

        Name = name;
        Brand = brand;
        Description = description;
        Color = color;
        Finish = finish;
        Surface = surface;
        IsExterior = isExterior;
        VolumeLiters = volumeLiters;
        Price = price;
        StockQuantity = stockQuantity;
    }

    public bool HasStockFor(int quantity)
    {
        return StockQuantity >= quantity;
    }
}
