namespace Innowise.Application.DTOs.Requests;

public sealed record SendAssistantMessageRequest(
    Guid? ConversationId,
    string Message);
