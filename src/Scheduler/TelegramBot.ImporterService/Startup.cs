using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Quartz;
using TelegramBot.ImporterService.Extensions;

namespace TelegramBot.ImporterService;

[ExcludeFromCodeCoverage(Justification = "This class only contains extension methods for DI registration and is not intended to be covered by tests.")]
public sealed class Startup
{
    public Startup(IConfiguration configuration)
        => Configuration = configuration;

    private IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
       => services
       .AddFanexDataWithSqlServer(Configuration)
       .AddFanexLogging(Configuration)
       .AddFanexCachingMemory()
       .AddRemotingRef(Configuration)
       .AddONEBookUM(Configuration)
       .AddRepositories()
       .AddServices()
       .AddAlphaServicesClient(Configuration)
       .AddQuartz(quartz =>
       {
           quartz.UseMicrosoftDependencyInjectionJobFactory();
           quartz.AddScheduleJobs(Configuration, Fanex.Logging.Logger.Log);
       })
       .AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

    public void Configure(IApplicationBuilder app)
        => app
        .UseRouting()
        .UseEndpoints(endpoints => endpoints.MapGet("/", () => "TelegramBot.Importer service!!!"))
        .UseONEBookUM(Configuration);
}