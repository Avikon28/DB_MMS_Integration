using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.Shared.POProduct.Services;
using Simple.OData.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SG.Shared.POProduct
{
    public class GetPOSkuLines
    {
        private static IConfiguration _configuration;
        private readonly ILogger _logger;

        private ODataClient _client = null;

        private readonly IOptions<QueryService> _config;

        public GetPOSkuLines(IConfiguration configuration, ILogger<GetPOSkuLines> logger,  IOptions<QueryService> config)
        {
            _configuration = configuration;
            _logger = logger;
            //_client = client;
            _config = config;
        }

        public async Task<List<POSkus>> GetPOSkusfromDBFromSku(string sku)
        {
            try
            {
                //pull data from queryservice and load
                //make call to queryservice
                _logger.LogInformation("Going to call PO Queryservice to get POSkus for sku.--{sku}", sku);
                if (_client == null)
                    _client = new ODataClient(_config.Value.QueryServiceAddress);
                var annotations = new ODataFeedAnnotations();
                List<POSkus> listofpoSkus = new List<POSkus>();

                var poSkus = await _client
                            .For<POSkus>()
                            .Filter(x => x.SKU== sku)
                            .FindEntriesAsync(annotations);
                listofpoSkus.AddRange(poSkus);

                while (annotations.NextPageLink != null)
                {
                    listofpoSkus.AddRange(await _client
                        .For<POSkus>()
                        .Filter(x => x.SKU == sku)
                        .FindEntriesAsync(annotations.NextPageLink, annotations));
                }


                _logger.LogInformation("PO Queryservice Returned Data for the POSkus.--{poSkus}", poSkus);


                return listofpoSkus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Get POskus from QueryService: {Reason}", ex.Message);
                return null;
            }

        }

        public async Task<List<POSkus>> GetPOSkusfromDBFromPONumber(string ponumber)
        {
            try
            {
                //pull data from queryservice and load
                //make call to queryservice
                _logger.LogInformation("Going to call PO Queryservice to get POSkus for PONumber.--{ponumber}", ponumber);
                if (_client == null)
                    _client = new ODataClient(_config.Value.QueryServiceAddress);
                var annotations = new ODataFeedAnnotations();
                List<POSkus> listofpoSkus = new List<POSkus>();

                var poSkus = await _client
                            .For<POSkus>()
                            .Filter(x => x.PONumber == Convert.ToInt32(ponumber))
                            .FindEntriesAsync(annotations);
                listofpoSkus.AddRange(poSkus);

                while (annotations.NextPageLink != null)
                {
                    listofpoSkus.AddRange(await _client
                        .For<POSkus>()
                        .Filter(x => x.PONumber == Convert.ToInt32(ponumber))
                        .FindEntriesAsync(annotations.NextPageLink, annotations));
                }


                _logger.LogInformation("PO Queryservice Returned Data for the POSkus.--{poSkus}", poSkus);


                return listofpoSkus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Get POskus from QueryService: {Reason}", ex.Message);
                return null;
            }

        }

        public async Task<List<POO>> GetPOsForVendorFromDB(string vendCode)
        {
            try
            {
                //pull data from queryservice and load
                //make call to queryservice
                _logger.LogInformation("Going to call PO Queryservice to get POs for the Vendorcode.", vendCode);
                if (_client == null)
                    _client = new ODataClient(_config.Value.QueryServiceAddress);
                var annotations = new ODataFeedAnnotations();
                List<POO> listofpos = new List<POO>();

                var pos = await _client
                            .For<POO>()
                            .Filter(x => x.SubVendor.VendCode == vendCode && (x.SubVendor.ClassCode == "I" || x.SubVendor.ClassCode=="D"))
                            .FindEntriesAsync(annotations);
                listofpos.AddRange(pos);

                while (annotations.NextPageLink != null)
                {
                    listofpos.AddRange(await _client
                        .For<POO>()
                        .Filter(x => x.SubVendor.VendCode == vendCode)
                        .FindEntriesAsync(annotations.NextPageLink, annotations));
                }


                _logger.LogInformation("PO Queryservice Returned Data for the POs for VendorCode.", listofpos);


                return listofpos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Get POs for the VendorCode from QueryService: {Reason}", ex.Message);
                return null;
            }

        }

    }
}
