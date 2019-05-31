using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using SG.PO.APLL.DataModel.Outputmodels;
using System;
using System.Threading.Tasks;


namespace SG.PO.APLL.ProcessingService
{
    class App
    {
        private ILogger _logger;
        private IConfiguration _config;
        private readonly ElasticClient _client;
        private readonly ElasticWriter _writer;
        


        public App(ILogger<App> logger, IConfiguration config, ElasticClient client, ElasticWriter writer )
        {
            _logger = logger;
            _config = config;
            _client = client;
            _writer = writer;
        }

        public void Run()
        {
            RunAsync().Wait();
        }

        private async Task RunAsync()
        {
            await Task.Yield();

            try
            {
               await _writer.WriteFileAsync<POAPLLOutput>();

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                //_logger.LogCritical(null, e, "Failure in publish");
                return;
            }

            return;
        }
    }
}
