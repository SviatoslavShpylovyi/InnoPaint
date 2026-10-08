namespace Innowise.Application.DTOs.Responses;

public sealed record BasketLineResponse(
    PaintResponse Paint,
    int Quantity,
    decimal LineTotal);
