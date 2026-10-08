using System.Net;

namespace BuildingBlocks.Exceptions;

public class BadRequestException : AppException
{
    public BadRequestException(string message, string? details = null, Exception? innerException = null)
        : base(message, details, innerException)
    {
    }

    public override int StatusCode => (int)HttpStatusCode.BadRequest;

    public override string Title => "Bad Request";
}
