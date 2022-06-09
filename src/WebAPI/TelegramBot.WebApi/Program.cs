using Alpha.API.Client.Extensions;
using Fanex.Data.Repository;
using Fanex.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;
using TelegramBot.Infrastructure.Shared;
using TelegramBot.Infrastructure.Shared.Configurations;
using TelegramBot.WebApi.Extensions;
using TelegramBot.WebApi.Middlewares;

[assembly: ApiController]
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var configuration = builder.Configuration;
var webAPIConfiguration = new WebAPIConfiguration();
configuration.GetSection(nameof(WebAPIConfiguration)).Bind(webAPIConfiguration);
webAPIConfiguration.HiddenActionsBySystem = configuration.GetSection("HiddenActionsBySystems")
                .Get<HiddenActionsBySystem[]>()
                .ToDictionary(kvp => kvp.SiteName, kvp => kvp.ActionIds);

builder.Services
    .AddSingleton(webAPIConfiguration)
    .AddSingleton<IDynamicRepository, DynamicRepository>()
    .AddSingleton<ICryptography, Cryptography>()
    .AddFanexLogging(configuration)
    .AddFanexCaching(configuration)
    .AddAlphaServicesClient(configuration)
    .AddOneBookUm(configuration)
    .AddInfrastructure()
    .AddFilters();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TelegramBot", Version = "v1" });
    options.CustomSchemaIds(type => type.FullName);
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();
app.UseRouting();
app.MapGet("/", () => "Telegram bot web api!!!");
app.UseUM(configuration);
app.UseFanexDataWithSqlServer(configuration, app.Environment);
app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapControllers();

var isDevelopment = app.Environment.IsDevelopment();
app.UseSwagger();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "TelegramBot v1"));

if (!isDevelopment)
{
    app.UseHsts();
    builder.WebHost.UseUrls("http://0.0.0.0:5000");
}

Logger.Log.Info($"{Assembly.GetExecutingAssembly().GetName().Name} has been started up.");

app.Run();