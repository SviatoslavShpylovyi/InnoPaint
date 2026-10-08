using BuildingBlocks.Exceptions;

namespace Innowise.Application.Exceptions;

public sealed class AssistantTurnLimitReachedException : BadRequestException
{
    public AssistantTurnLimitReachedException(int turnLimit)
        : base($"This conversation reached its limit of {turnLimit} turns. Start a new conversation to continue.")
    {
    }
}
