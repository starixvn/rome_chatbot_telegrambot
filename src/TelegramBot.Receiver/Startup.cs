using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TelegramBot.Client.DependencyInjection;
using TelegramBot.Common.Configurations;
using TelegramBot.Common.Extensions;
using TelegramBot.Receiver.Services;

namespace TelegramBot.Receiver
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddClient(Configuration);
            var receiverConfiguration = new ReceiverConfiguration();
            Configuration.GetSection(nameof(ReceiverConfiguration)).Bind(receiverConfiguration);
            services.AddSingleton(receiverConfiguration);
            services.AddFanexLogging(Configuration);
            services.AddHostedService<TelegramHostedService>();
            services.AddSingleton<IHandleUpdateService, HandleUpdateService>();
            services.AddControllers().AddNewtonsoftJson();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors();
            var receiverConfiguration = app.ApplicationServices.GetService<ReceiverConfiguration>();
            if (receiverConfiguration.IsUsingWebhook)
            {
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
}
