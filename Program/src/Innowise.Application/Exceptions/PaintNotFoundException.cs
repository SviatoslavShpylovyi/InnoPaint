using BuildingBlocks.Exceptions;

namespace Innowise.Application.Exceptions;

public sealed class PaintNotFoundException : NotFoundException
{
    public PaintNotFoundException(Guid id)
        : base($"Paint with id '{id}' was not found.")
    {
    }
}
