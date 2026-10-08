using System.ComponentModel;

using FluentValidation;

using Innowise.Application.DTOs.Requests;
using Innowise.Application.DTOs.Responses;
using Innowise.Application.Interfaces;

namespace Innowise.Application.Services;

public sealed class PaintCatalogTools
{
    private const int MinimumPaletteSize = 2;
    private const int MaximumPaletteSize = 4;

    private readonly IPaintService _paintService;
    private readonly IValidator<PaintFilterRequest> _filterValidator;
    private readonly IValidator<PaletteChoiceDto> _choiceValidator;
    private readonly HashSet<Guid> _searchedPaintIds = [];

    public PaintCatalogTools(
        IPaintService paintService,
        IValidator<PaintFilterRequest> filterValidator,
        IValidator<PaletteChoiceDto> choiceValidator)
    {
        _paintService = paintService;
        _filterValidator = filterValidator;
        _choiceValidator = choiceValidator;
    }

    public IReadOnlyList<PaletteItemResponse> Palette { get; private set; } = [];

    [Description("Get the actual colors, finishes, surfaces, and brands available in the paint catalog.")]
    public async Task<PaintLookupsResponse> GetFiltersAsync(CancellationToken cancellationToken)
    {
        return await _paintService.GetLookupsAsync(cancellationToken);
    }

    [Description("Search the catalog for paints that are in stock and match the given constraints.")]
    public async Task<PaintPageResponse> SearchPaintsAsync(
        string? color = null,
        string? surface = null,
        string? finish = null,
        bool? isExterior = null,
        decimal? maxPrice = null,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var filter = new PaintFilterRequest(
            Color: color,
            Surface: surface,
            Finish: finish,
            IsExterior: isExterior,
            MaxPrice: maxPrice,
            InStock: true,
            Page: page);

        await _filterValidator.ValidateAndThrowAsync(filter, cancellationToken);

        var results = await _paintService.GetAllAsync(filter, cancellationToken);

        foreach (var paint in results.Items)
        {
            _searchedPaintIds.Add(paint.Id);
        }

        return results;
    }

    [Description(
        "Present the final palette in the UI. Supply 2 to 4 distinct paints found by SearchPaintsAsync "
        + "during this turn, with a short role and explanation. "
        + "This only displays a recommendation; it does not add to an order.")]
    public async Task<string> PresentPaletteAsync(PaletteChoiceDto[] choices, CancellationToken cancellationToken)
    {
        EnsurePaletteSizeIsValid(choices);

        var items = new List<PaletteItemResponse>();

        foreach (var choice in choices)
        {
            await _choiceValidator.ValidateAndThrowAsync(choice, cancellationToken);
            EnsurePaintWasSearched(choice.PaintId);

            var paint = await _paintService.GetByIdAsync(choice.PaintId, cancellationToken);

            EnsurePaintIsInStock(paint);

            items.Add(new PaletteItemResponse(paint, choice.Role.Trim(), choice.Reason.Trim()));
        }

        Palette = items;

        return "Palette displayed. The user can add paints using the UI.";
    }

    private static void EnsurePaletteSizeIsValid(PaletteChoiceDto[] choices)
    {
        if (choices is null || choices.Length is < MinimumPaletteSize or > MaximumPaletteSize)
        {
            throw new ArgumentException($"Choose between {MinimumPaletteSize} and {MaximumPaletteSize} paints.", nameof(choices));
        }

        if (choices.Select(choice => choice.PaintId).Distinct().Count() != choices.Length)
        {
            throw new ArgumentException("Choose distinct paints.", nameof(choices));
        }
    }

    private void EnsurePaintWasSearched(Guid paintId)
    {
        if (!_searchedPaintIds.Contains(paintId))
        {
            throw new ArgumentException("Search for each paint before recommending it.", nameof(paintId));
        }
    }

    private static void EnsurePaintIsInStock(PaintResponse paint)
    {
        if (paint.StockQuantity < 1)
        {
            throw new ArgumentException("A selected paint is no longer in stock.", nameof(paint));
        }
    }
}
