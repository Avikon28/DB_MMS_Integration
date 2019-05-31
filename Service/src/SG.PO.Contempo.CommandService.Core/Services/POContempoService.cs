using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SG.MMS.PO.Events;
using SG.PO.Contempo.CommandService.Core.Mapper;
using SG.PO.Contempo.CommandService.Core.OutputModels;
using SG.Shared.Api;
using Simple.OData.Client;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SG.Shared.POProduct;
using SG.PO.Contempo.DataModels.Outputmodels;
using SG.PO.Contempo.CommandService.Core.Helper;
using SG.Shared.POProduct.Services;
using SG.Vendor.MMS.Events;
using System.Linq;
using SG.MMS.Product.Retail.Events;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.Shared.ModelCache.models;
using SG.Shared.ModelCache;

namespace SG.PO.Contempo.CommandService.Core.Services
{
    public class POContempoService
    {

        private static IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IOptions<QueryService> _config;
        private ODataClient client = null;
        private readonly IDataService _getDataService;
        private readonly string _poIndex;
        private readonly GetPOSkuLines _GetPOSkuLines;
        private readonly POContempoProductMapper _POCtmpProductMapper;

        public POContempoService(IConfiguration configuration, IDataService getDataService, ILogger<POContempoService> logger, IOptions<QueryService> config,
            GetPOSkuLines GetPOSkuLines, POContempoProductMapper POCtmpProductMapper)
        {
            _configuration = configuration;
            _logger = logger;
            _config = config;
            _getDataService = getDataService;
            _GetPOSkuLines = GetPOSkuLines;
            _POCtmpProductMapper = POCtmpProductMapper;
            _poIndex = _configuration.GetValue<string>("CurrentIndex");
        }

        public async Task<ApiResult<string>> UpsertPOContempo(MMS.PO.Events.MMSPOEvent model)
        {
            try
            {
                string ponumber = model.PONumber;
                _logger.LogInformation("Passed in {PONumber}.", model.PONumber);

                _logger.LogDebug("Fetching existing data for {PONumber}", ponumber);
                var existing = await GetExistingPO(ponumber);

                if (existing.poContempo != null)
                {
                    existing.poContempo = model.MapEventtoOutput(existing.poContempo);

                    if (existing.poContempo.POSkus != null && existing.poContempo.POSkus.Count > 0)
                    {
                        //update data in elastic
                        _logger.LogInformation("Going to update data for {PONumber} in elastic", ponumber);
                        var response = await _getDataService.UpdateItem<POContempoOutput>(existing.poContempo, _poIndex);

                        _logger.LogDebug("Updated data for {PONumber} in elastic", ponumber);
                    }
                    else
                    {
                        _logger.LogDebug("No SKUs found for {PONumber}. PO Data not updated in elastic.", ponumber);
                    }
                }
                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertPOContempo - Failed Updating ContempoPO: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }

        }

        public async Task<ApiResult<string>> UpsertPOContempoPOSku(MMS.PO.Events.MMSPOSkuEvent model)
        {

            try
            {
                string ponumber = model.PONumber;
                _logger.LogInformation("Passed in {PONumber}.", model.PONumber);

                _logger.LogDebug("Fetching existing data for {PONumber}", ponumber);
                var existing = await GetExistingPO(ponumber);

                if (existing.poContempo != null)
                {
                    existing.poContempo = model.MapEventtoOutput(existing.poContempo);

                    if (existing.poContempo.POSkus != null && existing.poContempo.POSkus.Count > 0)
                    {
                        _logger.LogDebug("Going to update data for {PONumber} in elastic", ponumber);
                        var response = await _getDataService.UpdateItem<POContempoOutput>(existing.poContempo, _poIndex);

                        _logger.LogDebug("Updated data for {PONumber} in elastic", ponumber);
                    }
                    else
                    {
                        _logger.LogDebug("No SKUs found for {PONumber}. PO Data not updated in elastic.", ponumber);
                    }
                }
                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertPOContempoPOSku - Failed Updating ContempoPO: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }
        }

        public async Task<ApiResult<string>> UpsertPOContempoProduct(MMS.Product.Events.MMSProductEvent model)
        {

            try
            {
                string skutobepassed = model.Sku;
                _logger.LogInformation("Passed in {Sku}.  Using {ProductSku} as po sku for lookup.", model.Sku, skutobepassed);

                _logger.LogDebug("Fetching POSkuLines for {Sku}", skutobepassed);
                var poskus = await _GetPOSkuLines.GetPOSkusfromDBFromSku(skutobepassed);

                //update for the POSkuLines w.r.t po

                _logger.LogDebug("Going to update data for {Sku} for POSku in elastic", skutobepassed);

                if (poskus != null && poskus.Count > 0)
                {
                    //update data in elastic for the poskus returned
                   poskus.ForEach(x =>
                   {
                       //check if the PO exists
                       var existingPO = GetExistingPO(x.PONumber.ToString());
                       if (existingPO != null && existingPO.Result.poContempo != null)//check to see if the PO is on elastic/DB, if not skip
                       {
                           _POCtmpProductMapper.UpdatePOObject(existingPO.Result.poContempo, model, x);
                           var response = _getDataService.UpdateItem<POContempoOutput>(existingPO.Result.poContempo, _poIndex);
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
                _logger.LogError(ex, "UpsertPOContempoProduct - Failed Updating ContempoPO: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }
        }

        public async Task<ApiResult<string>> UpsertPOContempoLookUp(MMS.LookupCode.Events.LookupCodeEvent model)
        {
            try
            {
                string lkupCode = model.Code;
                if (model.CodeType != "LABL")
                {
                    _logger.LogInformation("LookUp Code Type is not 'LABL' type. Passed in {Code}. Hence skipped further processing.", lkupCode);
                    return new ApiResult<string>();
                }

                _logger.LogInformation("Passed in {Code}.", lkupCode);
                _logger.LogDebug("Fetching existing data for {LookUpCode}", lkupCode);
                var existingPOs = await GetProductVendorsByLabel(lkupCode);

                if (existingPOs != null && existingPOs.Count > 0)
                {
                    Dictionary<int, List<string>> distinctPOSkus = existingPOs.GroupBy(x => x.PO,
                             (key, elements) =>
                              new
                              {
                                  PO = key,
                                  SKUs = elements.Where(m => m.PO == key).Select(p => p.SKU).ToList()
                              }).ToDictionary(p => p.PO, p => p.SKUs);

                    foreach (var eachpo in distinctPOSkus)
                    {
                        _logger.LogDebug("Fetching existing data for {PONumber}", eachpo.Key.ToString());
                        var existingPOresult = await GetExistingPO(eachpo.Key.ToString());
                        if (existingPOresult != null && existingPOresult.poContempo != null
                            && existingPOresult.poContempo.POSkus != null && existingPOresult.poContempo.POSkus.Count() > 0)
                        {
                            existingPOresult.poContempo.POSkus.ForEach(p =>
                            {
                                if (p.POProduct != null && eachpo.Value.Contains(p.SKU))
                                {
                                    p.POProduct.LabelDescription = model.ShortDescription;
                                }
                            });

                            //update data in elastic
                            _logger.LogDebug("Going to update data for {PONumber} in elastic", eachpo.Key);
                            var response = await _getDataService.UpdateItem<POContempoOutput>(existingPOresult.poContempo, _poIndex);
                            _logger.LogDebug("Updated data for {PONumber} in elastic", eachpo.Key);
                        }
                        else
                        {
                            _logger.LogDebug("No data found for {PONumber}", eachpo.Key.ToString());
                        }
                    }
                }
                else
                {
                    _logger.LogDebug("No data found for LookUp: {LookUpCode}", lkupCode);
                }

                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertPOContempoLookUp - Failed Updating ContempoPO LookUp data: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }
        }

        public async Task<List<POandSkus>> GetProductVendorsByLabel(string labelCode)
        {
            try
            {

                if (client == null)
                    client = new ODataClient(_config.Value.QueryServiceAddress);

                var listofposkus = await client
                                        .Unbound<POandSkus>()
                                        .Function("GetPOsforProductlabel")
                                        .Set(new { labeltype = labelCode })
                                        .ExecuteAsEnumerableAsync();

                if (listofposkus != null && listofposkus.Count() > 0)
                {
                    return listofposkus.ToList();
                }

                _logger.LogDebug("Fetching lookup data from ODataClient Finished");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed reading from ODataClient: {message}", ex.Message);
                throw;
            }

            return new List<POandSkus>();
        }

        public async Task<ApiResult<string>> UpsertPOContempoProductRetail(MMSProductRetailEvent model)
        {
            try
            {
                _logger.LogInformation("Passed in {Sku}-{RetailType}-{CurrencyCode}.", model.Sku, model.RetailType, model.CurrencyCode);
                _logger.LogDebug("Fetching existing data for {Sku}-{RetailType}-{CurrencyCode}.", model.Sku, model.RetailType, model.CurrencyCode);
                _logger.LogDebug("Fetching POSkuLines for {Sku}", model.Sku);

                var poskus = await _GetPOSkuLines.GetPOSkusfromDBFromSku(model.Sku);

                if (poskus != null && poskus.Count > 0)
                {
                    foreach (var eachpo in poskus)
                    {
                        _logger.LogDebug("Fetching existing data for {PONumber}", eachpo.PONumber.ToString());
                        var existingPOresult = await GetExistingPO(eachpo.PONumber.ToString());
                        if (existingPOresult != null && existingPOresult.poContempo != null
                            && existingPOresult.poContempo.POSkus != null && existingPOresult.poContempo.POSkus.Count() > 0)
                        {
                            existingPOresult.poContempo.POSkus.ForEach(x =>
                            {
                                if (
                                    x.SKU == model.Sku && model.RetailType == "Ticket" && model.CurrencyCode == "USD" &&
                                    x.CreateDate != null && !string.Equals(x.CreateDate.Value.ToString("MM/dd/yyyy"), DateTime.Today.ToString("MM/dd/yyyy"))
                                    )
                                {
                                    x.POProduct.RetailPrice = model.Retail.GetRetailPrice();
                                }
                            });
                            //update data in elastic
                            _logger.LogDebug("Going to update data for {PONumber} in elastic", eachpo.PONumber);
                            var response = await _getDataService.UpdateItem<POContempoOutput>(existingPOresult.poContempo, _poIndex);
                            _logger.LogDebug("Updated data for {PONumber} in elastic", eachpo.PONumber);
                        }
                        else
                        {
                            _logger.LogDebug("No data found for {PONumber}", eachpo.PONumber.ToString());
                        }
                    }
                }
                else
                {
                    _logger.LogDebug("No POSKUs found for {Sku}. PO Product Retail Data not updated in elastic.", model.Sku);
                }
                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertPOContempoProductRetail - Failed Updating ContempoPO: {Reason}", ex.Message);
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
                if (existing.poContempo != null)
                {
                    existing.poContempo.ForceInclude = true;
                    _logger.LogDebug("Going to update data for {PONumber} in elastic", poNumber);
                    var response = await _getDataService.UpdateItem<POContempoOutput>(existing.poContempo, _poIndex);

                    _logger.LogDebug("Updated data for {PONumber} in elastic", poNumber);
                }

                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ForceInclude- Failed Updating PO Contempo ForceInclude: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }
        }

        private async Task<POContempoResult> GetExistingPO(string ponumber)
        {
            POContempoResult poresult = new POContempoResult();
            DataResult result = new DataResult();

            ////call elastic to see if data is present. otherwise return data from DB
            //_logger.LogDebug("Going to call Elastic to retrieve data for {PONumber} ", ponumber);
            //var doc = await _getDataService.GetItem<POContempoOutput, POO>(ponumber, _poIndex);

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
            //            var obtecttobemapped = propValue as POContempoOutput;
            //            if (obtecttobemapped != null && obtecttobemapped.PONumber == ponumber)
            //            {
            //                poresult.poContempo = obtecttobemapped;
            //                _logger.LogDebug("Retrieved data from Elastic for {PONumber} ", ponumber);
            //                break;
            //            }
            //        }
            //    }

            //    //replace POSku data from DB as there are no posku events raised. hence elastic may not have the latest posku data at all.
            //    //Need to merge posku data from DB with POsku data for that PO from DB
            //    if (poresult.poContempo != null)
            //    {
            //        var retPOskus = await _GetPOSkuLines.GetPOSkusfromDBFromPONumber(ponumber);

            //        _logger.LogInformation("PO Queryservice Returned Data.", retPOskus);

            //        if (retPOskus != null && retPOskus.Count > 0)
            //        {
            //            poresult.poContempo.UpdatePOContempoPoSkuData(retPOskus);
            //        }
            //    }
            //}
            //else
            //{
                var obtecttobemapped = await GetPOfromDB(ponumber);
                poresult.poContempo = obtecttobemapped;
            //}

            return poresult;
        }

        private async Task<POContempoOutput> GetPOfromDB(string ponumber)
        {
            try
            {
                //pull data from queryservice and load
                //make call to queryservice
                _logger.LogInformation("Going to call PO Queryservice.--{ponumber}", ponumber);
                if (client == null)
                    client = new ODataClient(_config.Value.QueryServiceAddress);

                int po = int.Parse(ponumber);
                var inputProduct = await client
                        .For<POO>()
                        .Key(po)
                        .Expand("POSkus/POProduct")
                        .FindEntryAsync();
                //call below only if conditions matched
                if (inputProduct != null && inputProduct.StatusCode != "CL")
                {
                    _logger.LogInformation("PO Queryservice Returned Data.-- {inputProduct}", inputProduct);

                    var output = inputProduct.MaptoOutput();
                    //remove posku that has 0 retailprice

                    var poskustoberemoved = output.POSkus?.Where(x => x.POProduct.RetailPrice == "0");
                    _logger.LogInformation("POskus ignored.-- {poskustoberemoved}", poskustoberemoved?.ToList());

                    output.POSkus?.RemoveAll(x => x.POProduct.RetailPrice == "0");

                    return output;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPOfromDB - Failed Mapping POContempo: {Reason}", ex.Message);
                return null;
            }
        }       
    }
}
