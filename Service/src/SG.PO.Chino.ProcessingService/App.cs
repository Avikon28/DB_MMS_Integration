using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using SG.PO.Chino.DataModels.Outputmodels;
using System;
using System.Threading.Tasks;


namespace SG.PO.Chino.ProcessingService
{
    class App
    {
        private ILogger _logger;
        private IConfiguration _config;
        private readonly ElasticClient _client;
        private readonly ElasticWriter _writer;
        private readonly string _indexFormat;


        public App(ILogger<App> logger, IConfiguration config, ElasticClient client, ElasticWriter writer )
        {
            _logger = logger;
            _config = config;
            _client = client;
            _writer = writer;
            _indexFormat = _config.GetValue<string>("CurrentIndex");
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
                await _writer.WriteFileAsync<POChinoOutput>(_indexFormat);
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
