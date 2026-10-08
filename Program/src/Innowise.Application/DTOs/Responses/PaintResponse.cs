namespace Innowise.Application.DTOs.Responses;

public sealed record PaintResponse(
    Guid Id,
    string Name,
    string Brand,
    string Description,
    string Color,
    string Finish,
    string Surface,
    bool IsExterior,
    decimal VolumeLiters,
    decimal Price,
    int StockQuantity);
