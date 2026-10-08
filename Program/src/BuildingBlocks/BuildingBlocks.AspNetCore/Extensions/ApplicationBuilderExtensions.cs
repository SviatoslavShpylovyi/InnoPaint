using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;

namespace BuildingBlocks.AspNetCore.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseExceptionHandler();
        app.UseStatusCodePages();

        return app;
    }
}
