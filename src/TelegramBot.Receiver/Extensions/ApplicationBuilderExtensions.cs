using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using TelegramBot.Client.Services;

namespace TelegramBot.Receiver.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseWebhook(this IApplicationBuilder app)
        {
            var botService = app.ApplicationServices.GetService<IBotService>();
#pragma warning disable S4462 // Calls to "async" methods should not be blocking
            var bots = botService.GetAllAsync().GetAwaiter().GetResult();
#pragma warning restore S4462 // Calls to "async" methods should not be blocking
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
            return app;
        }
    }
}
