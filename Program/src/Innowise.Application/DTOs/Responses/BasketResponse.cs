namespace Innowise.Application.DTOs.Responses;

public sealed record BasketResponse(
    Guid Id,
    IReadOnlyList<BasketLineResponse> Items,
    long ItemCount,
    decimal Total);
