using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using WLMonitored.Shared.Services;

namespace WLMonitored.Shared.Extensions;

[ExcludeFromCodeCoverage(Justification = "This class only contains extension methods for DI registration and is not intended to be covered by tests.")]
public static class DIExtensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
        => services
            .AddSingleton<ILoggerService, LoggerService>()
            .AddSingleton<IGlobalSettingService, GlobalSettingService>();
}