using Fanex.Logging.Extensions.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Mime;
using TelegramBot.Client.DependencyInjection;
using TelegramBot.Common.Configurations;
using TelegramBot.Common.Extensions;
using TelegramBot.Receiver.Services;

namespace TelegramBot.Receiver
{
    public class Startup
    {
        private ReceiverConfiguration receiverConfiguration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            receiverConfiguration = new ReceiverConfiguration();
            Configuration.GetSection(nameof(ReceiverConfiguration)).Bind(receiverConfiguration);
            services.AddSingleton(receiverConfiguration);
            services.AddClient(Configuration);
            services.AddFanexLogging(Configuration);
            services.AddHostedService<TelegramHostedService>();
            services.AddSingleton<IHandleUpdateService, HandleUpdateService>();

            if (receiverConfiguration.IsUsingWebhook)
            {
                services.AddControllers().AddNewtonsoftJson();
            }

            services.AddHealthChecks();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment _)
        {
            if (receiverConfiguration.IsUsingWebhook)
            {
                ConfigureWebhook(app);
            }
        }

        private static void ConfigureWebhook(IApplicationBuilder app)
        {
            app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = MediaTypeNames.Text.Plain;
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    await ExceptionHandler.HandleAsync(exceptionHandlerPathFeature.Error, context).ConfigureAwait(false);
                    await context.Response.WriteAsync($"An exception was thrown: {exceptionHandlerPathFeature.Error.Message}");
                    await context.Response.WriteAsync(exceptionHandlerPathFeature.Error.StackTrace);
                });
            });
            
            app.UseHealthChecks("/");
            app.UseRouting();
            app.UseCors();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: $"tgwebhook",
                    pattern: "bot/{accessToken}",
                    new { controller = "Webhook", action = "Post" });
                endpoints.MapControllers();
            });
        }
    }
}
