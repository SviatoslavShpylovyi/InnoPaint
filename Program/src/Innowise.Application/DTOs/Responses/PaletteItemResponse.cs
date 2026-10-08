namespace Innowise.Application.DTOs.Responses;

public sealed record PaletteItemResponse(
    PaintResponse Paint,
    string Role,
    string Reason);
