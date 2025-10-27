using System.Diagnostics.CodeAnalysis;
using TelegramBot.Receiver.Configurations;
using TelegramBot.Receiver.DependencyInjection;
using TelegramBot.Receiver.Extensions;
using TelegramBot.Receiver.Services;

namespace TelegramBot.Receiver;

// S6513: Add a justification.
[ExcludeFromCodeCoverage(Justification = "Startup class is only used for application configuration and does not contain business logic to be covered by tests.")]
public class Startup
{
    public Startup(IConfiguration configuration) =>
        Configuration = configuration;

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        var receiverConfiguration = new ReceiverConfiguration();
        Configuration.GetSection(nameof(ReceiverConfiguration)).Bind(receiverConfiguration);

        services.AddSingleton(receiverConfiguration)
            .AddClient(Configuration)
            .AddFanexLogging()
            .AddHostedService<TelegramHostedService>()
            .AddSingleton<IHandleUpdateService, HandleUpdateService>()
            .AddControllers()
            .AddNewtonsoftJson();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.ConfigureExceptionHandler();
            app.UseHsts();
        }

        app.UseRouting();
        app.UseCors();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", () => "Telegram bot receiver!!!");

            endpoints.MapControllerRoute(
                name: $"tgwebhook",
                pattern: "bot/{accessToken}",
                new { controller = "Webhook", action = "Post" });

            endpoints.MapControllers();
        });

    }
}
