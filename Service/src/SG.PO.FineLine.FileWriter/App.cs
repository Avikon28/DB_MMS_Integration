using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using SG.PO.FineLine.FileWriter.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace SG.PO.FineLine.FileWriter
{
    class App
    {
        private ILogger _logger;
        private IConfiguration _config;
        private readonly ElasticClient _client;
        private readonly ElasticWriter _writer;
        private string IndexFormat => ($"mi9-po-{DateTime.Now.ToString("yyyy.MM.dd")}");


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
                _writer.WriteFile<POFineLineOutput>();// this call will be replaced with the below call
               // _writer.WriteFile<POFineLineOutput>(IndexFormat);
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
