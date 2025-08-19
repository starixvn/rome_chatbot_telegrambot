using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using WLMonitored.RiskControl.Importer.Extensions;
using WLMonitored.RiskControl.Importer.Jobs;
using WLMonitored.Shared.Extensions;

namespace WLMonitored.RiskControl.Importer;

[ExcludeFromCodeCoverage(Justification = "This class only contains extension methods for DI registration and is not intended to be covered by tests.")]
public sealed class Startup
{
    public Startup(IConfiguration configuration)
        => Configuration = configuration;

    private IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
        => services
            .AddONEBookUM(Configuration)
            .AddFanexLogging(Configuration)
            .AddFanexDataWithSqlServer(Configuration)
            .AddSharedServices()
            .AddServices(Configuration)
            .AddHostedService<ChangeAccumulatedWinLossJob>()
            .AddHostedService<ChangeThresholdWinLossJob>();

#pragma warning disable S2325
    public static void Configure(IApplicationBuilder app)
#pragma warning restore S2325
        => app
            .UseRouting()
            .UseEndpoints(endpoints => endpoints.MapGet("/", () => $"{typeof(Startup).Assembly.GetName().Name} service."));
}
