using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using SG.Shared.QueryData.models;
using Simple.OData.Client;
using System.Threading.Tasks;

namespace SG.Shared.QueryData
{
    public class ElasticDataService : IDataService
    {

        private static IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly ElasticClient _client;
        
        //private readonly IOptions<QueryService> _config;

       
        private ODataClient client = null;
        //private string Index => ($"oms-product");

        public ElasticDataService(IConfiguration configuration, ILogger<ElasticDataService> logger, ElasticClient client)
        {
            _configuration = configuration;
            _logger = logger;
            _client = client;
            
        }

        public async Task<DataResult> GetItem<Tout,Tin>(string key, string Index) where Tout : class where Tin : class
        {

            ElasticResult<Tout> elasticresult = new ElasticResult<Tout>();
            DBResult<Tin> dbresult = new DBResult<Tin>();
            var doc = await _client.GetAsync<Tout>(new DocumentPath<Tout>(key), g => g.Index(Index));

            if (doc.Source != null)//data is in elastic
            {
                elasticresult.Output = doc.Source;

                elasticresult.Exists = true;
            }
           
            return elasticresult;
        }

       
        //generic method to get all data and its asscociations. caller can ovverride and choose not to invoke this
        private async Task<T> GetItemFromDB<T>(string key, string queryaddress) where T : class
        {

            //pull data from queryservice and load
            if (client == null)
                client = new ODataClient(queryaddress);

            var metadata = await client.GetMetadataAsync<Microsoft.OData.Edm.IEdmModel>();
            //get the navigational properties if any and make a odata call
            string propertiestoexpandedon = Helper.Helper.GetPropertiestobeExpandedOn<T>(metadata).Substring(0, Helper.Helper.GetPropertiestobeExpandedOn<T>(metadata).Length-1);
            if (propertiestoexpandedon.Length>0)
            {
                  var inputitem = await client
                                        .For<T>()
                                        .Key(key)
                                        .Expand(propertiestoexpandedon)
                                        .FindEntryAsync();
                return inputitem;
            }
            else
            {
                var inputitem = await client
                                      .For<T>()
                                      .Key(key)
                                      .FindEntryAsync();
                return inputitem;
            }

          

           


        }

        //insert/update date in elastic
        public async Task<T> UpdateItem<T>(T model, string Index) where T : class
        {


            var response = await _client.UpdateAsync<T, object>(model, u => u.Doc(model).Index(Index).DocAsUpsert());
            return model;
        }
    }
}
