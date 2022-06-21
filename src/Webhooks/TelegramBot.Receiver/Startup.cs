using System.Net.Mime;
using Fanex.Logging.Extensions.AspNetCore;
using Microsoft.AspNetCore.Diagnostics;
using TelegramBot.Receiver.Configurations;
using TelegramBot.Receiver.DependencyInjection;
using TelegramBot.Receiver.Extensions;
using TelegramBot.Receiver.Services;

namespace TelegramBot.Receiver;

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
