namespace Innowise.Application.DTOs.Requests;

public sealed record PaintFilterRequest(
    string? Search = null,
    string? Brand = null,
    string? Color = null,
    string? Finish = null,
    string? Surface = null,
    bool? IsExterior = null,
    bool? InStock = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    decimal? MinVolumeLiters = null,
    decimal? MaxVolumeLiters = null,
    string SortBy = "name",
    bool Descending = false,
    int Page = 1,
    int PageSize = 20);
