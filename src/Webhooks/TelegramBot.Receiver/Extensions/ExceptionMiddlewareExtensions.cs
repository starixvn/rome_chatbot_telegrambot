using System.Diagnostics.CodeAnalysis;
using System.Net;
using Fanex.Logging.Extensions.AspNetCore;
using Microsoft.AspNetCore.Diagnostics;

namespace TelegramBot.Receiver.Extensions;

[ExcludeFromCodeCoverage]
public static class ExceptionMiddlewareExtensions
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    await ExceptionHandler.HandleAsync(contextFeature.Error, context).ConfigureAwait(false);
                    await context.Response.WriteAsync(new
                    {
                        context.Response.StatusCode,
                        Message = "Internal Server Error."
                    }.ToString());
                }
            });
        });
    }
}
