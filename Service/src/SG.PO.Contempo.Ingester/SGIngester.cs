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
using SG.Shared.ExecutionPolicies;
using Serilog;
using POEvents = SG.MMS.PO.Events;
using POProductEvents = SG.MMS.Product.Events;
using SG.MMS.Product.Retail.Events;

namespace SG.PO.Contempo.Ingester
{
    public class SGIngester : SG.Ingester.Ingester
    {
        public SGIngester(IConfiguration configuration, ILoggerFactory loggerFactory) : base(configuration, loggerFactory)
        {
        }

        public static void Main(string[] args)
        {
            Console.Title = typeof(SGIngester).Namespace;
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
                after: msg => Logger.LogInformation(EventIds.SuccessfullyProcessedEvent, "Succesfully processed event"),
                onError: (msg, e) =>
                {
                    Logger.LogError(EventIds.ErrorProcessingEvent, e, "Error processing event. Errors {e}", e.Data.Values);
                    return true;
                }
                );
            return callCommandService;
        }



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

            //PO contempo handlers
            handlers.Add(POEvents.MessageTypes.For<POEvents.MMSPOCreatedEvent>(), HttpInvokingHandler.CreateHandler<POEvents.MMSPOCreatedEvent>(HttpMethod.Post, msg => new Uri($"POContempo/cmd/create", UriKind.Relative), HttpStatusCode.NoContent));

            handlers.Add(POEvents.MessageTypes.For<POEvents.MMSPOUpdatedEvent>(), HttpInvokingHandler.CreateHandler<POEvents.MMSPOUpdatedEvent>(HttpMethod.Post, msg => new Uri($"POContempo/cmd/update", UriKind.Relative), HttpStatusCode.NoContent));

            //POsku contempo handlers
            handlers.Add(POEvents.MessageTypes.For<POEvents.MMSPOSkuCreatedEvent>(), HttpInvokingHandler.CreateHandler<POEvents.MMSPOSkuCreatedEvent>(HttpMethod.Post, msg => new Uri($"POContempo/cmd/posku/create", UriKind.Relative), HttpStatusCode.NoContent));

            handlers.Add(POEvents.MessageTypes.For<POEvents.MMSPOSkuUpdatedEvent>(), HttpInvokingHandler.CreateHandler<POEvents.MMSPOSkuUpdatedEvent>(HttpMethod.Post, msg => new Uri($"POContempo/cmd/posku/update", UriKind.Relative), HttpStatusCode.NoContent));

            //Product contempo handlers
            handlers.Add(POProductEvents.MessageTypes.For<POProductEvents.MMSProductCreatedEvent>(), HttpInvokingHandler.CreateHandler<POProductEvents.MMSProductCreatedEvent>(HttpMethod.Post, msg => new Uri($"POContempo/cmd/product/create", UriKind.Relative), HttpStatusCode.NoContent));

            handlers.Add(POProductEvents.MessageTypes.For<POProductEvents.MMSProductUpdatedEvent>(), HttpInvokingHandler.CreateHandler<POProductEvents.MMSProductUpdatedEvent>(HttpMethod.Post, msg => new Uri($"POContempo/cmd/product/update", UriKind.Relative), HttpStatusCode.NoContent));

            //PO contempo LookUp handlers
            handlers.Add(MMS.LookupCode.Events.MessageTypes.For<MMS.LookupCode.Events.LookupCodeCreatedEvent>(), HttpInvokingHandler.CreateHandler<MMS.LookupCode.Events.LookupCodeCreatedEvent>(HttpMethod.Post, msg => new Uri($"POContempo/cmd/lookup/create", UriKind.Relative), HttpStatusCode.NoContent));

            handlers.Add(MMS.LookupCode.Events.MessageTypes.For<MMS.LookupCode.Events.LookupCodeUpdatedEvent>(), HttpInvokingHandler.CreateHandler<MMS.LookupCode.Events.LookupCodeUpdatedEvent>(HttpMethod.Post, msg => new Uri($"POContempo/cmd/lookup/update", UriKind.Relative), HttpStatusCode.NoContent));

            //PO contempo ProductRetail handlers
            handlers.Add(MMS.Product.Retail.Events.MessageTypes.For<MMSProductRetailCreatedEvent>(), HttpInvokingHandler.CreateHandler<MMSProductRetailCreatedEvent>(HttpMethod.Post, msg => new Uri($"POContempo/cmd/productretail/create", UriKind.Relative), HttpStatusCode.NoContent));

            handlers.Add(MMS.Product.Retail.Events.MessageTypes.For<MMSProductRetailUpdatedEvent>(), HttpInvokingHandler.CreateHandler<MMSProductRetailUpdatedEvent>(HttpMethod.Post, msg => new Uri($"POContempo/cmd/productretail/update", UriKind.Relative), HttpStatusCode.NoContent));

            return new HttpInvokingHandler(client, handlers, throwOnMissing: false);
        }

        protected override ICheckpointManager CreateCheckpointManager()
        {
            return new CheckpointManager(new NullCheckpointReader(), new NullCheckpointWriter(), 1000, 1000);
        }
    }
}
