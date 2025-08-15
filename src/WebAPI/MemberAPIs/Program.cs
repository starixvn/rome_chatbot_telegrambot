using System.Reflection;
using Fanex.Data.Repository;
using Fanex.Data.SqlServer;
using Fanex.Logging;
using MemberAPIs.Extensions;
using MemberAPIs.Middlewares;
using MemberAPIs.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

[assembly: ApiController]

try 
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddControllers();

    var configuration = builder.Configuration;

    builder.Services
        .AddFanexSql(configuration)
        .AddSingleton<IDynamicRepository, DynamicRepository>()
        .AddFanexLogging(configuration)
        .AddFanexCaching(configuration)
        .AddOneBookUm(configuration)
        .AddInfrastructure()
        .AddFilters()
        .AddHealthChecks();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Member APIs", Version = "v1" });       
        var xmlFilename = $"{typeof(Program).Assembly.GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    });

    builder.Services.AddCustomRateLimiter(configuration);

    var app = builder.Build();
    app.UseRouting();

    app.MapGet("/", () => "Member APIs!!!");
    app.UseUM(configuration);
    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.MapControllers();

    var isDevelopment = app.Environment.IsDevelopment();
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Member APIs v1"));

    if (!isDevelopment)
    {
        app.UseHsts();
        builder.WebHost.UseUrls("http://0.0.0.0:5000");
    }

    Logger.Log.Info($"{typeof(Program).Assembly.GetName().Name} has been started up.");

    app.UseExceptionHandler(appError =>
    {
        appError.Run(async context =>
        {
            var ex = context.Features.Get<IExceptionHandlerFeature>();
            if (ex == null)
            {
                return;
            }

            var serviceProvider = builder.Services.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILoggerService>();
            Console.WriteLine(ex.Error.Message);
            await logger.Error(ex.Error);

            throw ex.Error;
        });
    });
    
    app.UseCustomRateLimiter(configuration);
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}