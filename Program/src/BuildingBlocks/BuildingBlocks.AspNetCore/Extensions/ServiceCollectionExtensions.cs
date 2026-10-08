using System.Reflection;

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

using BuildingBlocks.AspNetCore.Exceptions.Handlers;

namespace BuildingBlocks.AspNetCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    public static IServiceCollection AddRequestValidation(this IServiceCollection services, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);

        ValidatorOptions.Global.LanguageManager.Enabled = false;

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: false);

        return services;
    }
}
