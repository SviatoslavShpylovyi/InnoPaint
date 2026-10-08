using BuildingBlocks.Exceptions;

namespace Innowise.Application.Exceptions;

public sealed class BasketNotFoundException : NotFoundException
{
    public BasketNotFoundException(Guid id)
        : base($"Basket with id '{id}' was not found.")
    {
    }
}
