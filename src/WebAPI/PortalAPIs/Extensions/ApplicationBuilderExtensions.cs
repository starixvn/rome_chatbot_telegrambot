using System.Diagnostics.CodeAnalysis;
using ONEbook.UM;
using ONEbook.UM.AspNetCore;
using ONEbook.UM.Configurations;
using ONEbook.UM.Helpers;
using ONEbook.UM.Transport;

namespace PortalAPIs.Extensions
{
    [ExcludeFromCodeCoverage(Justification = "Extension class for middleware registration; tested indirectly via integration tests.")]
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseUM(this IApplicationBuilder app, IConfiguration configuration)
        {
            var umConfig = new UMClientConfiguration().Initialize(configuration);
            UMClientManager
                .UseConfig(umConfig)
                .UseRestClient(new UMRestClient(umConfig))
                .UseHttpContext(new HttpContextHelper(app.ApplicationServices.GetService<IHttpContextAccessor>()));
            return app;
        }
    }
}
