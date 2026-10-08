using System.Net;

namespace BuildingBlocks.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message, string? details = null, Exception? innerException = null)
        : base(message, details, innerException)
    {
    }

    public override int StatusCode => (int)HttpStatusCode.Conflict;

    public override string Title => "Conflict";
}
