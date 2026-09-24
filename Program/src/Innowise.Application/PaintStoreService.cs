using Innowise.Domain;

namespace Innowise.Application;

public sealed class PaintStoreService(IStoreRepository repository)
{
    private readonly object gate = new();

    public PaintPage Search(PaintQuery query)
    {
        var paints = repository.GetPaints().AsEnumerable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var terms = query.Search.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            paints = paints.Where(p => terms.All(term =>
                $"{p.Name} {p.Brand} {p.Description} {p.Color} {p.Finish} {p.Surface}"
                    .Contains(term, StringComparison.OrdinalIgnoreCase)));
        }
        paints = paints.Where(p => Matches(p.Brand, query.Brand) && Matches(p.Color, query.Color)
            && Matches(p.Finish, query.Finish) && Matches(p.Surface, query.Surface)
            && (query.IsExterior is null || p.IsExterior == query.IsExterior)
            && (query.InStock is null || (p.StockQuantity > 0) == query.InStock)
            && (query.MinPrice is null || p.Price >= query.MinPrice)
            && (query.MaxPrice is null || p.Price <= query.MaxPrice)
            && (query.MinVolumeLiters is null || p.VolumeLiters >= query.MinVolumeLiters)
            && (query.MaxVolumeLiters is null || p.VolumeLiters <= query.MaxVolumeLiters));
        var results = paints.ToArray();
        IOrderedEnumerable<Paint> sorted = query.SortBy switch
        {
            "price" => query.Descending ? results.OrderByDescending(p => p.Price) : results.OrderBy(p => p.Price),
            "volume" => query.Descending ? results.OrderByDescending(p => p.VolumeLiters) : results.OrderBy(p => p.VolumeLiters),
            "brand" => query.Descending ? results.OrderByDescending(p => p.Brand, StringComparer.OrdinalIgnoreCase) : results.OrderBy(p => p.Brand, StringComparer.OrdinalIgnoreCase),
            _ => query.Descending ? results.OrderByDescending(p => p.Name, StringComparer.OrdinalIgnoreCase) : results.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
        };
        var offset = ((long)query.Page - 1) * query.PageSize;
        return new(offset >= results.Length ? [] : sorted.ThenBy(p => p.Id).Skip((int)offset).Take(query.PageSize).ToArray(),
            results.Length, query.Page, query.PageSize);
    }

    public PaintLookups Lookups()
    {
        var paints = repository.GetPaints();
        static string[] Values(IEnumerable<string> values) => values.Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase).ToArray();
        return new(Values(paints.Select(p => p.Brand)), Values(paints.Select(p => p.Color)),
            Values(paints.Select(p => p.Finish)), Values(paints.Select(p => p.Surface)));
    }

    public Paint GetPaint(Guid id) => repository.GetPaint(id) ?? throw new StoreException(404, "Paint was not found.");

    public Paint CreatePaint(PaintRequest request)
    {
        lock (gate)
        {
            var paint = request.ToPaint(Guid.NewGuid());
            repository.SavePaint(paint);
            return paint;
        }
    }

    public Paint UpdatePaint(Guid id, PaintRequest request)
    {
        lock (gate)
        {
            GetPaint(id);
            if (repository.GetBaskets().Any(b => b.Items.Any(i => i.PaintId == id && i.Quantity > request.StockQuantity)))
                throw new StoreException(409, "Stock cannot be reduced below an existing basket quantity.");
            var paint = request.ToPaint(id);
            repository.SavePaint(paint);
            return paint;
        }
    }

    public void DeletePaint(Guid id)
    {
        lock (gate)
        {
            GetPaint(id);
            foreach (var basket in repository.GetBaskets().Where(b => b.Items.Any(i => i.PaintId == id)))
                repository.SaveBasket(basket with { Items = basket.Items.Where(i => i.PaintId != id).ToArray() });
            repository.DeletePaint(id);
        }
    }

    public BasketView CreateBasket()
    {
        lock (gate)
        {
            var basket = new Basket(Guid.NewGuid(), []);
            repository.SaveBasket(basket);
            return View(basket);
        }
    }

    public BasketView GetBasket(Guid id)
    {
        lock (gate) return View(FindBasket(id));
    }

    public BasketView SetItem(Guid basketId, Guid paintId, int quantity)
    {
        lock (gate)
        {
            var basket = FindBasket(basketId);
            var paint = GetPaint(paintId);
            if (quantity < 1 || quantity > 1000000) throw new StoreException(400, "Quantity must be between 1 and 1000000.");
            if (quantity > paint.StockQuantity) throw new StoreException(409, "Requested quantity exceeds available stock.");
            basket = basket with { Items = basket.Items.Where(i => i.PaintId != paintId).Append(new BasketItem(paintId, quantity)).ToArray() };
            repository.SaveBasket(basket);
            return View(basket);
        }
    }

    public void RemoveItem(Guid basketId, Guid paintId)
    {
        lock (gate)
        {
            var basket = FindBasket(basketId);
            if (!basket.Items.Any(i => i.PaintId == paintId)) throw new StoreException(404, "Basket item was not found.");
            repository.SaveBasket(basket with { Items = basket.Items.Where(i => i.PaintId != paintId).ToArray() });
        }
    }

    public void ClearBasket(Guid id)
    {
        lock (gate) repository.SaveBasket(FindBasket(id) with { Items = [] });
    }

    public void DeleteBasket(Guid id)
    {
        lock (gate)
            if (!repository.DeleteBasket(id)) throw new StoreException(404, "Basket was not found.");
    }

    private Basket FindBasket(Guid id) => repository.GetBasket(id) ?? throw new StoreException(404, "Basket was not found.");
    private BasketView View(Basket basket)
    {
        var lines = basket.Items.Select(i => { var paint = GetPaint(i.PaintId); return new BasketLine(paint, i.Quantity, paint.Price * i.Quantity); }).ToArray();
        return new(basket.Id, lines, lines.Sum(i => (long)i.Quantity), lines.Sum(i => i.LineTotal));
    }
    private static bool Matches(string value, string? filter) => string.IsNullOrWhiteSpace(filter)
        || value.Equals(filter.Trim(), StringComparison.OrdinalIgnoreCase);
}
