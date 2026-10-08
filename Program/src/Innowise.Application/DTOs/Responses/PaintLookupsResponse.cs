namespace Innowise.Application.DTOs.Responses;

public sealed record PaintLookupsResponse(
    IReadOnlyList<string> Brands,
    IReadOnlyList<string> Colors,
    IReadOnlyList<string> Finishes,
    IReadOnlyList<string> Surfaces);
