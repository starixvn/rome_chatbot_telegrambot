using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using WLMonitored.Shared.Services;

namespace WLMonitored.Shared.Extensions;

[ExcludeFromCodeCoverage]
public static class DIExtensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
        => services
            .AddSingleton<ILoggerService, LoggerService>()
            .AddSingleton<IGlobalSettingService, GlobalSettingService>();
}