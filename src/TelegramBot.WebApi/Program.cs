using System.Reflection;
using Fanex.Data.Repository;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.AspNetCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using TelegramBot.Common.Configurations;
using TelegramBot.Common.Extensions;
using TelegramBot.Common.Services;
using TelegramBot.WebApi.Extensions;

[assembly: ApiController]
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var configuration = builder.Configuration;
var webApiConfiguration = new WebApiConfiguration();
configuration.GetSection(nameof(WebApiConfiguration)).Bind(webApiConfiguration);
builder.Services
    .AddSingleton(webApiConfiguration).AddSingleton<IDynamicRepository, DynamicRepository>()
    .AddSingleton<ICryptography, Cryptography>()
    .AddFanexLogging(configuration)
    .AddFanexCaching(configuration)
    .AddInfrastructure();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TelegramBot", Version = "v1" });
    options.CustomSchemaIds(type => type.FullName);
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

ExceptionHandler.UseConfig(new LoggingConfiguration().GetHttpHeaderInfo());

var app = builder.Build();
app.UseRouting();
app.MapGet("/", () => "Telegram bot web api!!!");
app.UseUM(configuration);
app.UseFanexDataWithSqlServer(configuration, app.Environment);
app.MapControllers();

var isDevelopment = app.Environment.IsDevelopment();
app.UseExceptionHandler(isDevelopment ? "/error-development" : "/error");
if (!isDevelopment)
{
    app.UseHsts();
    builder.WebHost.UseUrls("http://0.0.0.0:5000");
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "TelegramBot v1"));
}

Logger.Log.Info($"{Assembly.GetExecutingAssembly().GetName().Name} has been started up.");

app.Run();