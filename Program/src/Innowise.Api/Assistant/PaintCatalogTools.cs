using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Innowise.Application;

namespace Innowise.Api.Assistant;

public sealed class PaintCatalogTools(PaintStoreService store)
{
    private readonly HashSet<Guid> foundPaintIds = [];

    public IReadOnlyList<PaletteItem> Palette { get; private set; } = Array.Empty<PaletteItem>();
    [Description(
        "Get the actual colors, finishes, surfaces, and brands " +
        "available in the paint catalog.")]
    public PaintLookups GetFilters() => store.Lookups();

    public PaintPage SearchPaints(
        string? color = null,
        string? surface = null,
        string? finish = null,
        bool? isExterior = null,
        decimal? maxPrice = null,
        int page = 1)
    {
        var query = new PaintQuery
        {
            Color = color,
            Surface = surface,
            Finish = finish,
            IsExterior = isExterior,
            MaxPrice = maxPrice,
            InStock = true,
            Page = page,
            PageSize = 20
        };

        // Tools bypass MVC's automatic request validation.
        Validator.ValidateObject(
            query,
            new ValidationContext(query),
            validateAllProperties: true);

        var result = store.Search(query);

        foreach (var paint in result.Items)
            foundPaintIds.Add(paint.Id);

        return result;
    }

    [Description(
        "Present the final palette in the UI. " +
        "Supply 2 to 4 distinct paints found by SearchPaints " +
        "during this turn, with a short role and explanation. " +
        "This only displays a recommendation; it does not add to an order.")]
    public string PresentPalette(PaletteChoice[] choices)
    {
        if (choices is null || choices.Length is < 2 or > 4)
            throw new ArgumentException("Choose between 2 and 4 paints.");
        if (choices.Select(c => c.PaintId).Distinct().Count()
            != choices.Length)
        {
            throw new ArgumentException("Choose distinct paints.");
        }

        var items = new List<PaletteItem>();

        foreach (var choice in choices)
        {
            if (!foundPaintIds.Contains(choice.PaintId))
                throw new ArgumentException(
                    "Search for each paint before recommending it.");

            if (string.IsNullOrWhiteSpace(choice.Role)
                || choice.Role.Length > 80
                || string.IsNullOrWhiteSpace(choice.Reason)
                || choice.Reason.Length > 500)
            {
                throw new ArgumentException(
                    "Supply a short role and reason for each paint.");
            }

            var paint = store.GetPaint(choice.PaintId);

            if (paint.StockQuantity < 1)
                throw new ArgumentException(
                    "A selected paint is no longer in stock.");

            items.Add(new PaletteItem(
                paint,
                choice.Role.Trim(),
                choice.Reason.Trim()));
        }

        Palette = items;
        return "Palette displayed. The user can add paints using the UI.";
    }
}
