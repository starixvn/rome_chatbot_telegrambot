using Microsoft.AspNetCore.Hosting;
using WLMonitored.RiskControl.Importer;

await Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
        webBuilder
            .UseStartup<Startup>()
            .UseUrls("http://0.0.0.0:5000")
    )
    .Build()
    .RunAsync();
