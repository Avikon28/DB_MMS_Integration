using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.Intertek.CommandService.Core.Mapper;
using SG.PO.Intertek.CommandService.Core.Outputmodels;
using SG.PO.Intertek.DataModels.Outputmodels;
using SG.Shared.Api;
using SG.Shared.ModelCache;
using SG.Shared.ModelCache.models;
using SG.Shared.POProduct;
using SG.Shared.POProduct.Services;
using SG.Vendor.MMS.Events;
using Simple.OData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;



namespace SG.PO.Intertek.CommandService.Core.Services
{
    public class POIntertekService
    {

        private static IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly string _poIndex;

        private readonly IOptions<QueryService> _config;

        private ODataClient client = null;
        private readonly IDataService _getDataService;
        private readonly GetPOSkuLines _GetPOSkuLines;
        private readonly POProductMapper _POProductMapper;

        private const string _locationNumber = "STORE";
        private const string _classCode = "I";
        private const string _statusCode = "CL";

        public POIntertekService(IConfiguration configuration, IDataService getDataService, ILogger<POIntertekService> logger,
            IOptions<QueryService> config,
            GetPOSkuLines GetPOSkuLines, POProductMapper POProductMapper)
        {
            _configuration = configuration;
            _logger = logger;     
            _config = config;
            _getDataService = getDataService;
            _GetPOSkuLines = GetPOSkuLines;
            _POProductMapper = POProductMapper;
            _poIndex = _configuration.GetValue<string>("CurrentIndex");
        }

        public async Task<ApiResult<string>> UpsertPOIntertek(MMS.PO.Events.MMSPOEvent model)
        {

            try
            {
                string ponumber = model.PONumber;
                _logger.LogInformation("Passed in {PONumber}.", model.PONumber);

                _logger.LogDebug("Fetching existing data for {PONumber}", ponumber);
                var existing = await GetExistingPO(ponumber);
                if (existing.POIntertek != null)
                {
                    existing.POIntertek = model.MapEventtoOutput(existing.POIntertek);

                    //update data in elastic
                    _logger.LogInformation("Going to update data for {PONumber} in elastic", ponumber);

                    var response = await _getDataService.UpdateItem<POIntertekOutput>(existing.POIntertek, _poIndex);

                    _logger.LogDebug("Updated data for {PONumber} in elastic", ponumber);
                }

                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertPOIntertek - Failed Updating IntertekPO: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }

        }

        public async Task<ApiResult<string>> UpsertPOIntertekPOSku(MMS.PO.Events.MMSPOSkuEvent model)
        {

            try
            {
                string ponumber = model.PONumber;
                _logger.LogInformation("Passed in {PONumber}.", model.PONumber);

                _logger.LogDebug("Fetching existing data for {PONumber}", ponumber);
                var existing = await GetExistingPO(ponumber);

                if (existing.POIntertek != null)
                {
                    existing.POIntertek = model.MapEventtoOutput(existing.POIntertek);

                    _logger.LogDebug("Going to update data for {PONumber} in elastic", ponumber);
                    var response = await _getDataService.UpdateItem<POIntertekOutput>(existing.POIntertek, _poIndex);

                    _logger.LogDebug("Updated data for {PONumber} in elastic", ponumber);
                }

                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertPOIntertekPOSku - Failed Updating IntertekPO: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }

        }

        public async Task<ApiResult<string>> UpsertPOIntertekProduct(MMS.Product.Events.MMSProductEvent model)
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
                       if (existingPO != null && existingPO.Result.POIntertek != null)//check to see if the PO is on elastic/DB, if not skip
                    {
                           _POProductMapper.UpdatePOObject(existingPO.Result.POIntertek, model, x);
                           var response = _getDataService.UpdateItem<POIntertekOutput>(existingPO.Result.POIntertek, _poIndex);
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
                _logger.LogError(ex, "UpsertPOIntertekProduct - Failed Updating IntertekPO: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }

        }

        public async Task<ApiResult<string>> UpsertPOIntertekVendor(MMSSubVendorEvent model)
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
                        if (existingPO != null && existingPO.POIntertek != null)//check to see if the PO is on elastic/DB, if not skip
                    {
                            _POProductMapper.UpdatePOObjectForVendor(existingPO.POIntertek, model);
                            var response = await _getDataService.UpdateItem<POIntertekOutput>(existingPO.POIntertek, _poIndex);
                        }

                    });

                    _logger.LogDebug("Updated data for POs for VendorCode -{vendcode} in elastic", vendcode);

                }
                else
                {
                    _logger.LogDebug("No POs found for VendorCode - {vendcode}. PO IntertekVendor Data not updated in elastic.", vendcode);
                }
                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertPOIntertekVendor - Failed Updating IntertekPO: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }

        }        

        public async Task<ApiResult<string>> ForceInclude(string poNumber)
        {

            try
            {                
                _logger.LogInformation("Passed in {PONumber}.", poNumber);

                _logger.LogDebug("Fetching existing data for {PONumber}", poNumber);
                var existing = await GetExistingPO(poNumber);
                if (existing.POIntertek != null)
                {

                    existing.POIntertek.ForceInclude = true;
                    _logger.LogDebug("Going to update data for {PONumber} in elastic", poNumber);
                    var response = await _getDataService.UpdateItem<POIntertekOutput>(existing.POIntertek, _poIndex);

                    _logger.LogDebug("Updated data for {PONumber} in elastic", poNumber);
                    //}
                }                

                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ForceInclude- Failed Updating IntertekPO: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }

        }

        private async Task<POIntertekResult> GetExistingPO(string ponumber)
        {

            POIntertekResult poresult = new POIntertekResult();
            DataResult result = new DataResult();


            ////call elastic to see if data is present. otherwise return data from DB
            //_logger.LogDebug("Going to call Elastic to retrieve data for {PONumber} ", ponumber);
            //var doc = await _getDataService.GetItem<POIntertekOutput, POO>(ponumber, _poIndex);

            //if (doc.Exists)
            //{

            //    poresult.Created = false;
            //    poresult.Exists = true;
            //    Type myType = doc.GetType();
            //    IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

            //    foreach (PropertyInfo prop in props)
            //    {
            //        object propValue = prop.GetValue(doc, null);
            //        if (propValue != null && (prop.Name == "Output"))
            //        {
            //            var obtecttobemapped = propValue as POIntertekOutput;
            //            poresult.POIntertek = obtecttobemapped;
            //            _logger.LogDebug("Retrieved data from Elastic for {PONumber} ", ponumber);
            //            break;
            //        }
            //    }

            //    //replace POSku data from DB as there are no posku events raised. hence elastic may not have the latest posku data at all.
            //    //Need to merge posku data from DB with POsku data for that PO from DB
            //    var poskus = await _GetPOSkuLines.GetPOSkusfromDBFromPONumber(ponumber);

            //    if (poskus != null && poskus.Count > 0)
            //    {
            //        poresult.POIntertek.UpdatePOIntertekPoSkuData(poskus);
            //    }

            //}
            //else
            //{
                var obtecttobemapped = await GetPOfromDB(ponumber);
                poresult.POIntertek = obtecttobemapped;
            //}

            return poresult;
        }

        private async Task<POIntertekOutput> GetPOfromDB(string ponumber)
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
                            .Filter(x => x.PONumber == po && x.SubVendor.ClassCode == _classCode
                                && x.StatusCode != _statusCode && x.LocationNumber != _locationNumber)//filter conditions
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
                _logger.LogError(ex, "Failed Mapping POIntertek: {Reason}", ex.Message);
                return null;
            }

        }
    }
}
