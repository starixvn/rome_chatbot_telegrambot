using Microsoft.AspNetCore.Hosting;
using WLMonitored.RiskControl.Importer;

await Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
    .Build()
    .RunAsync();
