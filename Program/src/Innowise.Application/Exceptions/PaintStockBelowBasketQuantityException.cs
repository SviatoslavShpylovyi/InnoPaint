using BuildingBlocks.Exceptions;

namespace Innowise.Application.Exceptions;

public sealed class PaintStockBelowBasketQuantityException : ConflictException
{
    public PaintStockBelowBasketQuantityException(Guid paintId, int stockQuantity)
        : base($"Stock of paint '{paintId}' cannot be reduced to {stockQuantity} because a basket already holds more.")
    {
    }
}
