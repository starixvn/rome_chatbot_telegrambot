using Microsoft.AspNetCore.Hosting;

namespace TelegramBot.SenderService;

#pragma warning disable S1118
public class Program
#pragma warning restore S1118
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Run();
    }

    public static IHost CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls("http://0.0.0.0:5000");
            })
            .Build();
}
