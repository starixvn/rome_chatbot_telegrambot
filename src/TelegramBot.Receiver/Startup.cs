using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using TelegramBot.Client.DependencyInjection;
using TelegramBot.Client.Services;
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
            services.AddHostedService<WebhookService>();
            services.AddScoped<IHandleUpdateService, HandleUpdateService>();
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

            var botService = app.ApplicationServices.GetService<IBotService>();
            var bots = botService.GetAllAsync().GetAwaiter().GetResult();
            app.UseEndpoints(endpoints =>
            {
                bots.ToList().ForEach(bot =>
                {
                    endpoints.MapControllerRoute(
                        name: "tgwebhook",
                        pattern: $"bot/{bot.AccessToken}",
                        new { controller = "Webhook", action = "Post" });
                });
                endpoints.MapControllers();
            });
        }
    }
}
