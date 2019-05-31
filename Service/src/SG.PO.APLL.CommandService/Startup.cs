using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Serilog;
using SG.Shared;
using SG.Shared.Messaging;
using SG.Shared.Web.Exceptions;
using SG.Shared.Web;
using Serilog.Core;
using Serilog.Core.Enrichers;
using Serilog.Context;
using Serilog.Events;
using Microsoft.Extensions.PlatformAbstractions;
using Serilog.Sinks.Kafka;
using SG.Shared.ElasticSearch.Filters;
using SG.PO.APLL.CommandService.Core.Services;
using SG.Shared.POProduct.Services;
using SG.Shared.POProduct;
using SG.Shared.POProduct.Mapper;
using SG.PO.APLL.DataModel.Outputmodels;
using SG.Shared.ModelCache;

namespace SG.PO.APLL.CommandService
{

    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(env.ContentRootPath)
             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
             .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
             .AddEnvironmentVariables("SG_")
             .AddEnvironmentVariables();
            Configuration = builder.Build();

            var appname = Configuration["applicationName"] ?? PlatformServices.Default.Application.ApplicationName;

            LogEventLevel level;
            if (!Enum.TryParse(Configuration["LogLevel"], out level))
                level = LogEventLevel.Information;

            Log.Logger = new LoggerConfiguration()
                .Enrich.WithProperty("service", env.ApplicationName)
                .Enrich.WithProperty("hostname", Environment.MachineName)
                .Enrich.WithProperty("host", Environment.GetEnvironmentVariable("HOST"))
                .Enrich.WithProperty("location", Configuration["Location"] ?? "unknown").Enrich.FromLogContext()
                .WriteTo.Console(level)
                .MinimumLevel.ControlledBy(new LoggingLevelSwitch(level))
                .WriteTo.Kafka(new KafkaConfiguration(Configuration.GetSection("Serilog.Sinks.Kafka")))
                .CreateLogger();

            Log.Logger.Information("Service Start");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScopedLoggingContext(
          (n, v) => LogContext.PushProperty(n, v),
          v => LogContext.Push(v.Select(x => (ILogEventEnricher)new PropertyEnricher(x.Key, x.Value)).ToArray())
          );
            services.AddDistributedMemoryCache();

            services.AddSingleton<InboundMessageLogContext>();
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddScoped<LookupDataService>();
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ExceptionFilter));
                options.Filters.Add(typeof(HttpActivityContext));
                options.Filters.Add(typeof(HttpInboundMessageLogContext));
                options.Filters.Add(typeof(ElasticIndexFilter<POAPLLOutput>));
            });
            services.AddLogging();
            services.AddMvcCore().AddJsonFormatters();
            services.AddTransient<POAPLService>();
            services.AddTransient<GetPOSkuLines>();
            services.AddTransient<POProductMapper>();
            services.AddTransient<LookupDataService>();

            services.AddTransient<IDataService, ElasticDataService>();

            //add information on configuration
            
            services.Configure<QueryService>(Configuration.GetSection("BaseAddress"));

            var settings = Configuration.GetSection("ElasticSettings");
            services.AddElastic<POAPLLOutput>(options =>
            {
                options.NodeList = settings.GetSection("NodeList").Get<string[]>();
                options.UserName = settings["UserName"];
                options.Password = settings["Password"];
            }, p => p.PONumber);
        }


        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime, Microsoft.Extensions.Caching.Distributed.IDistributedCache cache)
        {
            loggerFactory.AddSerilog();
            app.EnableBuffering(8 << 20);
            app.UseMvc();

            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
        }
    }
}
