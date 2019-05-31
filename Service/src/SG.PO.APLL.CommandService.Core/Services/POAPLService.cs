using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SG.PO.APLL.CommandService.Core.Outputmodels;
using SG.Shared.Api;
using Simple.OData.Client;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SG.PO.APLL.CommandService.Core.Mapper;
using SG.Shared.POProduct;
using SG.Shared.POProduct.Mapper;
using SG.Shared.POProduct.Services;
using SG.PO.APLL.DataModel.Outputmodels;
using SG.Vendor.MMS.Events;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.Shared.ModelCache.models;
using SG.Shared.ModelCache;

namespace SG.PO.APLL.CommandService.Core.Services
{
    public class POAPLService
    {
        private static IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly string _poIndex;

        private readonly IOptions<QueryService> _config;

        private ODataClient client = null;
        private readonly IDataService _getDataService;

        private readonly GetPOSkuLines _GetPOSkuLines;

        private readonly POProductMapper _POProductMapper;

        public POAPLService(IConfiguration configuration, IDataService getDataService, ILogger<POAPLService> logger,
            IOptions<QueryService> config, GetPOSkuLines GetPOSkuLines, POProductMapper POProductMapper)
        {
            _configuration = configuration;
            _logger = logger;
            //_client = client;
            _config = config;
            _getDataService = getDataService;
            _GetPOSkuLines = GetPOSkuLines;
            _POProductMapper = POProductMapper;
            _poIndex = _configuration.GetValue<string>("CurrentIndex");

        }

        public async Task<ApiResult<string>> UpsertPOAPL(MMS.PO.Events.MMSPOEvent model)
        {

            try
            {
                string ponumber = model.PONumber;
                _logger.LogInformation("Passed in {PONumber}.", model.PONumber);

                _logger.LogInformation("Fetching existing data for {PONumber}", ponumber);
                var existing = await GetExistingPO(ponumber);
                if (existing.POAPL != null)
                {
                    //if (existing.POAPL.CheckPoStatus())//filter conditions
                    //{
                    existing.POAPL = model.MapEventtoOutput(existing.POAPL);

                    _logger.LogInformation("Going to update data for {PONumber} in elastic", ponumber);
                    var response = await _getDataService.UpdateItem<POAPLLOutput>(existing.POAPL, _poIndex);

                    _logger.LogDebug("Updated data for {PONumber} in elastic", ponumber);
                    //}
                }
                // existing.POAPL.ActivityCode = model.GetType() == typeof(MMSPOCreatedEvent) ? "A" : "U";

                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertPOAPL- Failed Updating APLPO: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }

        }


        public async Task<ApiResult<string>> UpsertPOAPLPOSku(MMS.PO.Events.MMSPOSkuEvent model)
        {

            try
            {
                string ponumber = model.PONumber;
                _logger.LogInformation("Passed in {PONumber}.", model.PONumber);

                _logger.LogDebug("Fetching existing data for {PONumber}", ponumber);
                var existing = await GetExistingPO(ponumber);
                if (existing.POAPL != null)
                {
                    //if (existing.POAPL.CheckPoStatus())//filter conditions
                    //{
                    existing.POAPL = model.MapEventtoOutput(existing.POAPL);

                    _logger.LogDebug("Going to update data for {PONumber} in elastic", ponumber);
                    var response = await _getDataService.UpdateItem<POAPLLOutput>(existing.POAPL, _poIndex);

                    _logger.LogDebug("Updated data for {PONumber} in elastic", ponumber);
                    //}
                }

                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertPOAPLPOSku-Failed Updating APLPO: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }
        }

        public async Task<ApiResult<string>> UpsertPOAPLProduct(MMS.Product.Events.MMSProductEvent model)
        {
            try
            {
                string skutobepassed = model.Sku;
                _logger.LogInformation("Passed in {Sku}.  Using {ProductSku} as po sku for lookup.", model.Sku, skutobepassed);

                _logger.LogDebug("Fetching POSkuLines for {Sku}", skutobepassed);
                var poskus = await _GetPOSkuLines.GetPOSkusfromDBFromSku(skutobepassed);
                if (poskus != null && poskus.Count > 0)
                {
                    //update for the POSkuLines w.r.t po
                    _logger.LogDebug("Going to update data for {Sku} for POSku in elastic", skutobepassed);
                    //update data in elastic for the poskus returned
                    poskus.ForEach(x =>
                    {
                        //check if the PO exists
                        var existingPO = GetExistingPO(x.PONumber.ToString());
                        if (existingPO != null && existingPO.Result.POAPL != null)//check to see if the PO is on elastic/DB, if not skip
                        {
                            //if (existingPO.POAPL.CheckPoStatus())//filter conditions
                            //{
                            _POProductMapper.UpdatePOObject(existingPO.Result.POAPL, model, x);
                            var response = _getDataService.UpdateItem<POAPLLOutput>(existingPO.Result.POAPL, _poIndex);
                            //}
                        }
                    });

                    _logger.LogDebug("Updated data for {Sku} for POSku in elastic", skutobepassed);
                }
                else
                {
                    _logger.LogDebug("No POSKUs found for {Sku}. PO Product Data not updated in elastic.", skutobepassed);
                }
                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertPOAPLProduct-Failed Updating APLPO: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }
        }

        public async Task<ApiResult<string>> UpsertPOAPLVendor(MMSSubVendorEvent model)
        {

            try
            {
                string vendcode = model.VendCode;
                _logger.LogInformation("Using {VendCode} as po vendorcode for lookup.", vendcode);

                _logger.LogDebug("Fetching POs for VendorCode for {VendCode}", vendcode);
                var pos = await _GetPOSkuLines.GetPOsForVendorFromDB(vendcode);
                if (pos != null && pos.Count > 0)
                {
                    //update for the POs w.r.t vendorcode
                    _logger.LogDebug("Going to update data for POS for VendCode in elastic", vendcode);

                    //update data in elastic for the POs returned
                    pos.ForEach(async x =>
                    {
                        //check if the PO exists
                        var existingPO = await GetExistingPO(x.PONumber.ToString());
                        if (existingPO != null && existingPO.POAPL != null)//check to see if the PO is on elastic/DB, if not skip
                        {
                            //if (existingPO.POAPL.CheckPoStatus())
                            //{
                            _POProductMapper.UpdatePOObjectForVendor(existingPO.POAPL, model);
                            var response = await _getDataService.UpdateItem<POAPLLOutput>(existingPO.POAPL, _poIndex);
                            //}
                        }

                    });

                    _logger.LogDebug("Updated data for POs for VendorCode -{vendcode} in elastic", vendcode);
                }
                else
                {
                    _logger.LogDebug("No POs found for VendorCode - {vendcode}. PO APLLVendor Data not updated in elastic.", vendcode);
                }
                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertPOAPLVendor-Failed Updating APLPO: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }

        }

        public async Task<ApiResult<string>> ForceInclude(string poNumber)
        {

            try
            {
                string ponumber = poNumber.ToString();
                _logger.LogInformation("Passed in {PONumber}.", poNumber);

                _logger.LogDebug("Fetching existing data for {PONumber}", ponumber);
                var existing = await GetExistingPO(ponumber);
                if (existing.POAPL != null)
                {

                    existing.POAPL.ForceInclude = true;
                    _logger.LogDebug("Going to update data for {PONumber} in elastic", ponumber);
                    var response = await _getDataService.UpdateItem<POAPLLOutput>(existing.POAPL, _poIndex);

                    _logger.LogDebug("Updated data for {PONumber} in elastic", ponumber);
                    //}
                }
                // existing.POAPL.ActivityCode = model.GetType() == typeof(MMSPOCreatedEvent) ? "A" : "U";

                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertPOAPL- Failed Updating APLPO: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }

        }

        private async Task<POAPLResult> GetExistingPO(string ponumber)
        {

            POAPLResult poresult = new POAPLResult();
            DataResult result = new DataResult();
            //removing the elastic call for now to pinpoint a defect

            //call elastic to see if data is present. otherwise return data from DB
            //_logger.LogInformation("Going to call Elastic to retrieve data for {PONumber} ", ponumber);
            // var doc = await _getDataService.GetItem<POAPLLOutput, POO>(ponumber, _poIndex);

            //if (doc.Exists)
            //{
            //    _logger.LogInformation("Data Exists in elastic for  {PONumber} ", ponumber);
            //    poresult.Created = false;
            //    poresult.Exists = true;
            //    Type myType = doc.GetType();
            //    IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

            //    foreach (PropertyInfo prop in props)
            //    {
            //        object propValue = prop.GetValue(doc, null);
            //        if (propValue != null && (prop.Name == "Output"))
            //        {
            //            var obtecttobemapped = propValue as POAPLLOutput;
            //            poresult.POAPL = obtecttobemapped;
            //            _logger.LogInformation("Retrieved data from Elastic for {PONumber} ", ponumber);
            //            break;
            //        }
            //    }

            //    //replace POSku data from DB as there are no posku events raised. hence elastic may not have the latest posku data at all.
            //    //Need to merge posku data from DB with POsku data for that PO from DB
            //    _logger.LogInformation("Data exists in elastic for  {PONumber}, will call GetPOSkusfromDBFromPONumber ", ponumber);
            //    var poskus = await _GetPOSkuLines.GetPOSkusfromDBFromPONumber(ponumber);

            //    if (poskus != null && poskus.Count > 0)
            //    {
            //        poresult.POAPL.UpdatePOAPLPoSkuData(poskus);
            //    }
            //}
            //else
            //{
                var obtecttobemapped = await GetPOfromDB(ponumber);
                poresult.POAPL = obtecttobemapped;
                //if (obtecttobemapped != null)
                //    poresult.POAPL.ForceInclude = Convert.ToBoolean(_configuration["ForceInclude"]);//To read ForceInclude property value from appsettings.json
            //}
            
            return poresult;
        }


        private async Task<POAPLLOutput> GetPOfromDB(string ponumber)
        {
            try
            {
                //pull data from queryservice and load
                //make call to queryservice

                _logger.LogInformation("Going to call PO Queryservice.--{ponumber}", ponumber);
                if (client == null)
                    client = new ODataClient(_config.Value.QueryServiceAddress);

                int po = int.Parse(ponumber);
                //check conditions for the PO
                var pocheck = await client
                            .For<POO>()
                            .Filter(x => x.PONumber == po && x.StatusCode != "CL" && (x.SubVendor.ClassCode == "I" || x.SubVendor.ClassCode == "D"))//filter conditions
                            .FindEntryAsync();

                //call below only if conditions matched
                if (pocheck != null)
                {
                    var inputProduct = await client
                                .For<POO>()
                                .Key(po)
                                .Expand("SubVendor,POSkus/POProduct")
                                .FindEntryAsync();

                    _logger.LogInformation("PO Queryservice Returned Data.-- {inputProduct}", inputProduct);

                    var output = inputProduct.MaptoOutput();

                    return output;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed Mapping POAPL: {Reason}", ex.Message);
                return null;
            }

        }
    }
}
