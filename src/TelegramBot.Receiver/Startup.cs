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
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (receiverConfiguration.IsUsingWebhook)
            {
                ConfigureWebhook(app, env);
            }
        }

        private void ConfigureWebhook(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
