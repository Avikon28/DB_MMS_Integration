using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using SG.PO.Intertek.DataModels.Outputmodels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace SG.PO.Intertek.ProcessingService
{
    class App
    {
        private ILogger _logger;
        private IConfiguration _config;
        private readonly ElasticClient _client;
        private readonly ElasticWriter _poIntertekwriter;

        public App(ILogger<App> logger, IConfiguration config, ElasticClient client, ElasticWriter poIntertekwriter)
        {
            _logger = logger;
            _config = config;
            _client = client;
            _poIntertekwriter = poIntertekwriter;            
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
                await _poIntertekwriter.WriteFileAsync<POIntertekOutput>();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);                
                return;
            }

            return;
        }       
    }
}
