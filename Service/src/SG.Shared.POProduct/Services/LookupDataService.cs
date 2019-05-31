using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SG.MMS.QueryService.ODATA.Models.OMS;
using SG.MMS.QueryService.ODATA.Models.PO;
using Simple.OData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SG.Shared.POProduct.Services
{
    public class LookupDataService
    {


        private static IConfiguration _configuration;
        private readonly ILogger _logger;


        private readonly IOptions<QueryService> _config;
        private static ODataClient client = null;

        public LookupDataService(IConfiguration configuration, ILogger<LookupDataService> logger, IOptions<QueryService> config)
        {
            _configuration = configuration;
            _logger = logger;
            //_client = client;
            _config = config;
        }

        public async Task<List<MMS.QueryService.ODATA.Models.PO.ProductVendor>> GetProductVendors(string sku)
        {
            List<MMS.QueryService.ODATA.Models.PO.ProductVendor> listofproductvendors = new List<MMS.QueryService.ODATA.Models.PO.ProductVendor>();
            try
            {

                if (client == null)
                    client = new ODataClient(_config.Value.QueryServiceAddress);
                var annotations = new ODataFeedAnnotations();

                var lookupdataList = await client
                                .For<MMS.QueryService.ODATA.Models.PO.ProductVendor>()
                                .Filter(x => x.Sku == sku)
                                .FindEntriesAsync();
                listofproductvendors.AddRange(lookupdataList);
                //_logger.LogDebug("Fetching lookup data from ODataClient Finished");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed on GetProductVendors: {message}", ex.Message);
                //throw;
            }



            return listofproductvendors;
        }

        public async Task<List<MMS.QueryService.ODATA.Models.PO.ProductHierarchy>> GetProductHierarchy(string subClass)
        {
            List<MMS.QueryService.ODATA.Models.PO.ProductHierarchy> listofproducthierarchy = new List<MMS.QueryService.ODATA.Models.PO.ProductHierarchy>();
            try
            {


                if (client == null)
                    client = new ODataClient(_config.Value.QueryServiceAddress);
                var annotations = new ODataFeedAnnotations();

                var lookupdataList = await client
                                .For<MMS.QueryService.ODATA.Models.PO.ProductHierarchy>()
                                .Filter(x => x.SubClass == subClass)
                                .FindEntriesAsync();
                listofproducthierarchy.AddRange(lookupdataList);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed on GetProductHierarchy: {message}", ex.Message);
                //throw;
            }



            return listofproducthierarchy;
        }

        public async Task<ProductLabelCode> GetProductLabelDescription(string code)
        {
            ProductLabelCode productlabelCode = new ProductLabelCode();
            try
            {


                if (client == null)
                    client = new ODataClient(_config.Value.QueryServiceAddress);
                var annotations = new ODataFeedAnnotations();

                var productlabelCodes = await client
                                .For<ProductLabelCode>()
                                .Filter(x => x.Code == code)
                                .FindEntriesAsync();

                productlabelCode = productlabelCodes?.ToList().FirstOrDefault();
            }
            catch (Exception ex)
            {

                throw;
            }



            return productlabelCode;
        }

        public async Task<List<MMS.QueryService.ODATA.Models.PO.VendorAp>> GetVendorName(string vendCode)
        {
            List<MMS.QueryService.ODATA.Models.PO.VendorAp> listofproductvendors = new List<MMS.QueryService.ODATA.Models.PO.VendorAp>();
            try
            {


                if (client == null)
                    client = new ODataClient(_config.Value.QueryServiceAddress);
                var annotations = new ODataFeedAnnotations();

                var lookupdataList = await client
                                .For<MMS.QueryService.ODATA.Models.PO.VendorAp>()
                                .Filter(x => x.VendCode == vendCode)
                                .FindEntriesAsync();
                listofproductvendors.AddRange(lookupdataList);

            }
            catch (Exception ex)
            {

                throw;
            }



            return listofproductvendors;
        }

    }
}