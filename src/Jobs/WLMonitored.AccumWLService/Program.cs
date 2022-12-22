using Microsoft.AspNetCore.Hosting;
using WLMonitored.AccumWLService;

await Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
    .Build()
    .RunAsync();
