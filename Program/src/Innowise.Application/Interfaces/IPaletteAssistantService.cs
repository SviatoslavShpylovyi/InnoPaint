using Innowise.Application.DTOs.Requests;
using Innowise.Application.DTOs.Responses;

namespace Innowise.Application.Interfaces;

public interface IPaletteAssistantService
{
    Task<AssistantReplyResponse> ReplyAsync(SendAssistantMessageRequest request, CancellationToken cancellationToken = default);
}
