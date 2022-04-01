using Fanex.Data.Repository;
using Fanex.Logging;
using Fanex.Logging.Extensions;
using Fanex.Logging.Extensions.AspNetCore;
using Fanex.Logging.Extensions.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TelegramBot.Common.Configurations;
using TelegramBot.Common.Extensions;
using TelegramBot.Common.Services;
using TelegramBot.WebApi.Extensions;

namespace TelegramBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var webApiConfiguration = new WebApiConfiguration();
            Configuration.GetSection(nameof(WebApiConfiguration)).Bind(webApiConfiguration);
            services.AddSingleton(webApiConfiguration);
            services.AddSingleton<IDynamicRepository, DynamicRepository>();
            services.AddSingleton<ICryptography, Cryptography>();
            services.AddFanexLogging(Configuration);
            services.AddFanexCaching(Configuration);
            services.AddInfrastructure();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TelegramBot", Version = "v1" });
                c.CustomSchemaIds(type => type.FullName);
                var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList();
                xmlFiles.ForEach(xmlFile => c.IncludeXmlComments(xmlFile));
            });
            services.AddHealthChecks();

            ExceptionHandler
                // To use custom config
                .UseConfig(new LoggingConfiguration()
                .GetHttpHeaderInfo()
                .UseGetDbExceptionInfo(DatabaseInfoGetters.GetDbExceptionInfo));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(env.IsDevelopment() ? "/error-development" : "/error");

            app.UseHealthChecks("/");
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TelegramBot v1"));

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());

            app.UseUM(Configuration);
            app.UseFanexDataWithSqlServer(Configuration, env);

            Logger.Log.Info($"{Assembly.GetExecutingAssembly().GetName().Name} has been started up.");
        }
    }
}
