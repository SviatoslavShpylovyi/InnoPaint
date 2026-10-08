using BuildingBlocks.Exceptions;

namespace Innowise.Application.Exceptions;

public sealed class BasketItemNotFoundException : NotFoundException
{
    public BasketItemNotFoundException(Guid basketId, Guid paintId)
        : base($"Basket '{basketId}' has no item for paint '{paintId}'.")
    {
    }
}
