using System.ComponentModel.DataAnnotations;
using Innowise.Domain;

namespace Innowise.Application;

public sealed class PaintRequest
{
    [Required, StringLength(150)] public string Name { get; set; } = "";
    [Required, StringLength(100)] public string Brand { get; set; } = "";
    [Required, StringLength(4000)] public string Description { get; set; } = "";
    [Required, StringLength(80)] public string Color { get; set; } = "";
    [Required, StringLength(80)] public string Finish { get; set; } = "";
    [Required, StringLength(80)] public string Surface { get; set; } = "";
    public bool IsExterior { get; set; }
    [Range(typeof(decimal), "0.01", "1000", ParseLimitsInInvariantCulture = true)] public decimal VolumeLiters { get; set; }
    [Range(typeof(decimal), "0.01", "1000000", ParseLimitsInInvariantCulture = true)] public decimal Price { get; set; }
    [Range(0, 1000000)] public int StockQuantity { get; set; }

    public Paint ToPaint(Guid id) => new(id, Name.Trim(), Brand.Trim(), Description.Trim(),
        Color.Trim(), Finish.Trim(), Surface.Trim(), IsExterior, VolumeLiters, Price, StockQuantity);
}

public sealed class PaintQuery : IValidatableObject
{
    [StringLength(200)] public string? Search { get; set; }
    [StringLength(100)] public string? Brand { get; set; }
    [StringLength(80)] public string? Color { get; set; }
    [StringLength(80)] public string? Finish { get; set; }
    [StringLength(80)] public string? Surface { get; set; }
    public bool? IsExterior { get; set; }
    public bool? InStock { get; set; }
    [Range(typeof(decimal), "0", "1000000", ParseLimitsInInvariantCulture = true)] public decimal? MinPrice { get; set; }
    [Range(typeof(decimal), "0", "1000000", ParseLimitsInInvariantCulture = true)] public decimal? MaxPrice { get; set; }
    [Range(typeof(decimal), "0.01", "1000", ParseLimitsInInvariantCulture = true)] public decimal? MinVolumeLiters { get; set; }
    [Range(typeof(decimal), "0.01", "1000", ParseLimitsInInvariantCulture = true)] public decimal? MaxVolumeLiters { get; set; }
    [RegularExpression("^(name|price|volume|brand)$")] public string SortBy { get; set; } = "name";
    public bool Descending { get; set; }
    [Range(1, int.MaxValue)] public int Page { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinPrice > MaxPrice)
            yield return new("MinPrice must not exceed MaxPrice.", [nameof(MinPrice), nameof(MaxPrice)]);
        if (MinVolumeLiters > MaxVolumeLiters)
            yield return new("Minimum volume must not exceed maximum volume.", [nameof(MinVolumeLiters), nameof(MaxVolumeLiters)]);
    }
}

public sealed class QuantityRequest
{
    [Range(1, 1000000)] public int Quantity { get; set; }
}

public sealed record PaintPage(IReadOnlyList<Paint> Items, int TotalCount, int Page, int PageSize);
public sealed record PaintLookups(string[] Brands, string[] Colors, string[] Finishes, string[] Surfaces);
public sealed record BasketLine(Paint Paint, int Quantity, decimal LineTotal);
public sealed record BasketView(Guid Id, IReadOnlyList<BasketLine> Items, long ItemCount, decimal Total);

public interface IStoreRepository
{
    IReadOnlyList<Paint> GetPaints();
    Paint? GetPaint(Guid id);
    void SavePaint(Paint paint);
    bool DeletePaint(Guid id);
    Basket? GetBasket(Guid id);
    void SaveBasket(Basket basket);
    bool DeleteBasket(Guid id);
    IReadOnlyList<Basket> GetBaskets();
}

public sealed class StoreException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

