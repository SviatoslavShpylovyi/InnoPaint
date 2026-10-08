using BuildingBlocks.Exceptions;

namespace Innowise.Application.Exceptions;

public sealed class PaintStockExceededException : ConflictException
{
    public PaintStockExceededException(Guid paintId, int requestedQuantity, int availableQuantity)
        : base($"Paint '{paintId}' has {availableQuantity} in stock, which is less than the requested {requestedQuantity}.")
    {
    }
}
