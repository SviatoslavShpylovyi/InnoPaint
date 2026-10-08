namespace Innowise.Application.DTOs.Requests;

public sealed record CreatePaintRequest(
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
