﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Core.Enrichers;
using SG.PO.FineLine.FileWriter.Helper;
using SG.PO.FineLine.FileWriter.Services;
using SG.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SG.PO.FineLine.FileWriter
{
    class Program
    {
        static IConfiguration Configuration { get; set; }
        static IServiceProvider _serviceProvider;

        protected static IConfiguration DefaultConfiguration(
            IEnumerable<KeyValuePair<string, string>> defaultValues = null,
            Action<ConfigurationBuilder> customOverrides = null,
            string basePath = null)
        {

            var builder = new ConfigurationBuilder();
            builder.SetBasePath(!string.IsNullOrWhiteSpace(basePath) ? basePath : Directory.GetCurrentDirectory());
            if (defaultValues != null)
                builder.AddInMemoryCollection(defaultValues);
            builder.AddJsonFile("appsettings.json", optional: true);
            customOverrides?.Invoke(builder);
            builder.AddEnvironmentVariables("SG_");
            builder.AddEnvironmentVariables();
            return builder.Build();
        }

        static void Main(string[] args)
        {
          

            Configuration = DefaultConfiguration(defaultValues: DefaultConnection());
            var appname = Configuration["applicationName"] ?? PlatformServices.Default.Application.ApplicationName;

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .Enrich.WithProperty("service", appname)
                .Enrich.WithProperty("hostname", Environment.MachineName)
                .Enrich.WithProperty("location", Configuration["Location"] ?? "unknown")

                .CreateLogger();

            Log.Logger.Information("Service Start");

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            var factory = _serviceProvider.GetService<ILoggerFactory>();
            factory.AddSerilog();

            try
            {
                var app = _serviceProvider.GetService<App>();
                app.Run();
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }

        private static IEnumerable<KeyValuePair<string, string>> DefaultConnection()
        {
            yield return new KeyValuePair<string, string>("DefaultConnectionString", "SQLConnection");
        }

        public static void ConfigureServices(IServiceCollection services)
        {


            services.AddScopedLoggingContext(
               (n, v) => LogContext.PushProperty(n, v),
               v => LogContext.Push(v.Select(x => (ILogEventEnricher)new PropertyEnricher(x.Key, x.Value)).ToArray())
               );

            
            services.AddSingleton<IConfiguration>(Configuration);
            //services.AddElastic();
            var settings = Configuration.GetSection("ElasticSettings");
            
            services.AddElastic<POFineLineOutput>(options => 
            {
                options.NodeList = settings.GetSection("NodeList").Get<string[]>();
                options.UserName = settings["UserName"];
                options.Password = settings["Password"];
            }, p => p.PurchaseOrder);

            //get the output options
            services.Configure<OutputSettings>(Configuration.GetSection("WriteOutput"));

            services.AddTransient<ElasticWriter>();

            services.AddTransient<ProduceOutput>();

            services.AddSingleton<App>();
        }
    }
}
