using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SG.Shared;
using SG.Shared.Messaging;
using Serilog.Context;
using Serilog.Core;
using Serilog.Core.Enrichers;
using SG.Shared.Messaging.Handlers;
using System.Net.Http;
using System.Net;
using SG.Shared.KafkaConsumer;
using Serilog;
using POEvents = SG.MMS.PO.Events;
using POProductEvents = SG.MMS.Product.Events;
using POvendorEvents = SG.Vendor.MMS.Events;

namespace SG.PO.APLL.Ingester
{
    public class SGIngester : SG.Ingester.Ingester
    {
        public SGIngester(IConfiguration configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }

        public static void Main(string[] args)
        {
            Console.Title = "SG.PO.APLL.Ingester";
            var config = DefaultConfiguration();
            var logging = DefaultLogging(config);
            var ingester = new SGIngester(config, logging);
            var loggingContext = new LogContextWrapper(
                (n, v) => LogContext.PushProperty(n, v),
                v => LogContext.Push(v.Select(x => (ILogEventEnricher)new PropertyEnricher(x.Key, x.Value)).ToArray())
            );

            Log.Logger.Information("Service Start");
            try
            {
                ingester.Run(loggingContext);
            }
            catch (Exception ex)
            {
                Log.Logger.Fatal("{Unhandled exception}", ex);
                throw ex;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        protected override IHandle<Message> CreateHandler(ScopedLoggingContext context)
        {

            var callCommandService = new TryCatchFinallyHandler<Message>(CreateHttpHandler(),
                after: msg => Logger.LogInformation(EventIds.SuccessfullyProcessedEvent, "Succesfully processed event."),
                onError: (msg, e) =>
                {
                    Logger.LogError(EventIds.ErrorProcessingEvent, e, "Error processing event. Errors {e}", e.Data.Values);

                    return true;
                }
                );


            return callCommandService;
        }
        //test 


        private IHandle<Message> CreateHttpHandler()
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri(Configuration.GetSection("http")["baseaddress"]),
                //TODO: Optimize command service startup caching
                //temporary to wait for the command service to cache it's startup data                
                Timeout = TimeSpan.FromMinutes(2)
            };

            var handlers = new Dictionary<string, Func<HttpClient, Message, Task>>();

            //PO handlers
            handlers.Add(POEvents.MessageTypes.For<POEvents.MMSPOCreatedEvent>(), HttpInvokingHandler.CreateHandler<POEvents.MMSPOCreatedEvent>(HttpMethod.Post, msg => new Uri($"poapl/cmd/create", UriKind.Relative), HttpStatusCode.NoContent));

            handlers.Add(POEvents.MessageTypes.For<POEvents.MMSPOUpdatedEvent>(), HttpInvokingHandler.CreateHandler<POEvents.MMSPOUpdatedEvent>(HttpMethod.Post, msg => new Uri($"poapl/cmd/update", UriKind.Relative), HttpStatusCode.NoContent));

            //POsku handlers
            handlers.Add(POEvents.MessageTypes.For<POEvents.MMSPOSkuCreatedEvent>(), HttpInvokingHandler.CreateHandler<POEvents.MMSPOSkuCreatedEvent>(HttpMethod.Post, msg => new Uri($"poapl/cmd/posku/create", UriKind.Relative), HttpStatusCode.NoContent));

            handlers.Add(POEvents.MessageTypes.For<POEvents.MMSPOSkuUpdatedEvent>(), HttpInvokingHandler.CreateHandler<POEvents.MMSPOSkuUpdatedEvent>(HttpMethod.Post, msg => new Uri($"poapl/cmd/posku/update", UriKind.Relative), HttpStatusCode.NoContent));

            //Product handlers
            handlers.Add(POProductEvents.MessageTypes.For<POProductEvents.MMSProductCreatedEvent>(), HttpInvokingHandler.CreateHandler<POProductEvents.MMSProductCreatedEvent>(HttpMethod.Post, msg => new Uri($"poapl/cmd/product/create", UriKind.Relative), HttpStatusCode.NoContent));

            handlers.Add(POProductEvents.MessageTypes.For<POProductEvents.MMSProductUpdatedEvent>(), HttpInvokingHandler.CreateHandler<POProductEvents.MMSProductUpdatedEvent>(HttpMethod.Post, msg => new Uri($"poapl/cmd/product/update", UriKind.Relative), HttpStatusCode.NoContent));

            //POVendor handlers
            handlers.Add(POvendorEvents.MessageTypes.For<POvendorEvents.MMSSubVendorCreatedEvent>(), HttpInvokingHandler.CreateHandler<POvendorEvents.MMSSubVendorCreatedEvent>(HttpMethod.Post, msg => new Uri($"poapl/cmd/vendor/create", UriKind.Relative), HttpStatusCode.NoContent));

            handlers.Add(POvendorEvents.MessageTypes.For<POvendorEvents.MMSSubVendorUpdatedEvent>(), HttpInvokingHandler.CreateHandler<POvendorEvents.MMSSubVendorUpdatedEvent>(HttpMethod.Post, msg => new Uri($"poapl/cmd/vendor/update", UriKind.Relative), HttpStatusCode.NoContent));



            return new HttpInvokingHandler(client, handlers, throwOnMissing: false);
        }

        protected override ICheckpointManager CreateCheckpointManager()
        {
            return new CheckpointManager(new NullCheckpointReader(), new NullCheckpointWriter(), 1000, 1000);
            //return new CheckpointManager(new NullCheckpointReader(), new NullCheckpointWriter(), Math.Max(100, 2 * Configuration.GetValue<int>("SG.OMS.Product:MaxBatchCount")), Math.Max(1000, 2 * Configuration.GetValue<int>("SG.OMS.Product:BatchTimeout")));
        }
    }
}
