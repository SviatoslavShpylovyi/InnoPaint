using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using BuildingBlocks.Exceptions;

namespace BuildingBlocks.AspNetCore.Exceptions.Handlers;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        var problemDetails = CreateProblemDetails(exception);

        if (problemDetails is null)
        {
            _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}.", httpContext.Request.Method, httpContext.Request.Path);

            return false;
        }

        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    private static ProblemDetails? CreateProblemDetails(Exception exception)
    {
        if (exception is AppException appException)
        {
            return new ProblemDetails
            {
                Status = appException.StatusCode,
                Title = appException.Title,
                Detail = appException.Message
            };
        }

        if (exception is ValidationException validationException)
        {
            return CreateValidationProblemDetails(validationException);
        }

        return null;
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = "One or more validation errors occurred."
        };
    }
}
