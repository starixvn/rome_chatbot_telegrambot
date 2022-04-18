using Fanex.Logging.Extensions.AspNetCore;
using Microsoft.AspNetCore.Diagnostics;
using System.Net.Mime;
using TelegramBot.Receiver.Configurations;
using TelegramBot.Receiver.DependencyInjection;
using TelegramBot.Receiver.Services;

var builder = WebApplication.CreateBuilder(args);

var receiverConfiguration = new ReceiverConfiguration();
var configuration = builder.Configuration;

configuration.GetSection(nameof(ReceiverConfiguration)).Bind(receiverConfiguration);
builder.Services.AddSingleton(receiverConfiguration)
    .AddClient(configuration)
    .AddFanexLogging(configuration)
    .AddHostedService<TelegramHostedService>()
    .AddSingleton<IHandleUpdateService, HandleUpdateService>()
    .AddControllers()
    .AddNewtonsoftJson();

var app = builder.Build();
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = MediaTypeNames.Text.Plain;
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature != null)
        {
            await ExceptionHandler.HandleAsync(exceptionHandlerPathFeature.Error, context).ConfigureAwait(false);
            await context.Response.WriteAsync($"An exception was thrown: {exceptionHandlerPathFeature.Error.Message}");
            await context.Response.WriteAsync(exceptionHandlerPathFeature.Error.StackTrace);
        }
    });
});

app.UseRouting();
app.UseCors();
app.MapGet("/", () => "Telegram bot receiver!!!");
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: $"tgwebhook",
        pattern: "bot/{accessToken}",
        new { controller = "Webhook", action = "Post" });
    endpoints.MapControllers();
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    builder.WebHost.UseUrls("http://0.0.0.0:5000");
}

app.Run();