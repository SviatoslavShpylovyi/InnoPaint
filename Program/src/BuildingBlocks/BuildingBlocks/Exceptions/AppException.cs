namespace BuildingBlocks.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string message, string? details = null, Exception? innerException = null)
        : base(message, innerException)
    {
        Details = details;
    }

    public string? Details { get; }

    public abstract int StatusCode { get; }

    public abstract string Title { get; }
}
