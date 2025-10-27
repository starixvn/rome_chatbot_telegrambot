using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Quartz;
using TelegramBot.SenderService.Extensions;

namespace TelegramBot.SenderService;

// Justification: Startup class is excluded from code coverage as it contains only configuration and no business logic.
[ExcludeFromCodeCoverage(Justification = "Startup class is excluded from code coverage as it contains only configuration and no business logic.")]
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddFanexDataWithSqlServer(Configuration)
            .AddFanexCachingMemory()
            .AddFanexLogging(Configuration)
            .AddONEBookUM(Configuration)
            .AddServices(Configuration)
            .AddQuartz(quartz =>
            {
                quartz.UseMicrosoftDependencyInjectionJobFactory();
                quartz.AddNotificationSenderJob(Configuration, Fanex.Logging.Logger.Log);
            })
            .AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", () => "TelegramBot Notification Service!!!");
        });
        app.UseONEBookUM(Configuration);
    }
}
