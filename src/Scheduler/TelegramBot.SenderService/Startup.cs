using Microsoft.AspNetCore.Builder;
using Quartz;
using TelegramBot.SenderService.Extensions;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics.CodeAnalysis;

namespace TelegramBot.SenderService;

[ExcludeFromCodeCoverage]
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
