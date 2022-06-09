using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using WLMonitored.AccumWLService.Extensions;
using WLMonitored.AccumWLService.Jobs;
using WLMonitored.Shared.Extensions;

namespace WLMonitored.AccumWLService;

[ExcludeFromCodeCoverage]
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
            .AddFanexCachingMemory()
            .AddServices(Configuration)
            .AddHostedService<AccumulateWinLossJob>();

#pragma warning disable S2325
    public void Configure(IApplicationBuilder app)
#pragma warning restore S2325
        => app
            .UseRouting()
            .UseEndpoints(endpoints => endpoints.MapGet("/", () => $"{typeof(Startup).Assembly.GetName().Name} service."));
}
