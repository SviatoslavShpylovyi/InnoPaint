using System.Net;

namespace BuildingBlocks.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string message, string? details = null, Exception? innerException = null)
        : base(message, details, innerException)
    {
    }

    public override int StatusCode => (int)HttpStatusCode.NotFound;

    public override string Title => "Not Found";
}
