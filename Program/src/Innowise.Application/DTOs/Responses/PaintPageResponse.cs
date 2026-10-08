namespace Innowise.Application.DTOs.Responses;

public sealed record PaintPageResponse(
    IReadOnlyList<PaintResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);
