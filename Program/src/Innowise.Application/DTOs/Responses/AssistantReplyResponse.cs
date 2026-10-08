namespace Innowise.Application.DTOs.Responses;

public sealed record AssistantReplyResponse(
    Guid ConversationId,
    string Message,
    IReadOnlyList<PaletteItemResponse> Palette);
