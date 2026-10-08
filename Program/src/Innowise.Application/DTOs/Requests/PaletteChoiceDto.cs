namespace Innowise.Application.DTOs.Requests;

public sealed record PaletteChoiceDto(
    Guid PaintId,
    string Role,
    string Reason);
