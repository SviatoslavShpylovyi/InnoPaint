using BuildingBlocks.Exceptions;

namespace Innowise.Application.Exceptions;

public sealed class AssistantConversationNotFoundException : NotFoundException
{
    public AssistantConversationNotFoundException(Guid conversationId)
        : base($"Conversation '{conversationId}' expired. Start a new conversation.")
    {
    }
}
