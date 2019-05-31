using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.FineLine.CommandService.Core.Mapper;
using SG.PO.FineLine.CommandService.Core.outputmodels;
using SG.PO.FineLine.DataModels;
using SG.PO.FineLine.DataModels.Outputmodels;
using SG.Shared.Api;
using SG.Shared.POProduct;
using SG.Shared.POProduct.Services;
using SG.Shared.QueryData;
using SG.Shared.QueryData.models;
using Simple.OData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SG.PO.FineLine.CommandService.Core.Services
{
    public class POFineLineService
    {

        private static IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly string _poIndex;

        private readonly IOptions<QueryService> _config;

        private ODataClient client = null;
        private readonly IDataService _getDataService;

        private readonly GetPOSkuLines _GetPOSkuLines;

        private readonly POFineLineProductMapper _POFLProductMapper;

        public POFineLineService(IConfiguration configuration, IDataService getDataService, ILogger<POFineLineService> logger,
            IOptions<QueryService> config, GetPOSkuLines GetPOSkuLines, POFineLineProductMapper POFLProductMapper)
        {
            _configuration = configuration;
            _logger = logger;
            // _client = client;
            _config = config;
            _getDataService = getDataService;
            _GetPOSkuLines = GetPOSkuLines;
            _POFLProductMapper = POFLProductMapper;
            _poIndex = _configuration.GetValue<string>("CurrentIndex");
        }

        public async Task<ApiResult<string>> UpsertPOFineLine(MMS.PO.Events.MMSPOEvent model)
        {

            try
            {
                string ponumber = model.PONumber;
                _logger.LogInformation("Passed in {PONumber}.", model.PONumber);

                _logger.LogDebug("Fetching existing data for {PONumber}", ponumber);
                var existing = await GetExistingPO(ponumber);
                if (existing.POFineLine != null)
                {
                    existing.POFineLine = model.MapEventtoOutput(existing.POFineLine);

                    if (existing.POFineLine.POSkus != null && existing.POFineLine.POSkus.Count > 0)
                    {
                        //update data in elastic
                        _logger.LogInformation("Going to update data for {PONumber} in elastic", ponumber);
                        var response = await _getDataService.UpdateItem<POFineLineOutput>(existing.POFineLine, _poIndex);

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
                _logger.LogError(ex, "UpsertPOFineLine- Failed Updating FineLinePO: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }
        }

        public async Task<ApiResult<string>> UpsertPOFineLinePOSku(MMS.PO.Events.MMSPOSkuEvent model)
        {
            try
            {
                string ponumber = model.PONumber;
                _logger.LogInformation("Passed in {PONumber}.", model.PONumber);

                _logger.LogDebug("Fetching existing data for {PONumber}", ponumber);
                var existing = await GetExistingPO(ponumber);

                if (existing.POFineLine != null)
                {
                    if (existing.POFineLine.POSkus != null && existing.POFineLine.POSkus.Count > 0)
                    {
                        //update data in elastic
                        _logger.LogDebug("Going to update data for {PONumber} in elastic", ponumber);
                        var response = await _getDataService.UpdateItem<POFineLineOutput>(existing.POFineLine, _poIndex);
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
                _logger.LogError(ex, "UpsertPOFineLine- Failed Updating FineLinePO: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }
        }

        public async Task<ApiResult<string>> UpsertPOFineLineProduct(MMS.Product.Events.MMSProductEvent model)
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
                       if (existingPO != null && existingPO.Result.POFineLine != null)//check to see if the PO is on elastic/DB, if not skip
                       {
                           _POFLProductMapper.UpdatePOObject(existingPO.Result.POFineLine, model, x);
                           var response = _getDataService.UpdateItem<POFineLineOutput>(existingPO.Result.POFineLine, _poIndex);
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
                _logger.LogError(ex, "UpsertPOFineLineProduct- Failed Updating FineLinePO: {Reason}", ex.Message);
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
                if (existing.POFineLine != null)
                {
                    existing.POFineLine.ForceInclude = true;
                    _logger.LogDebug("Going to update data for {PONumber} in elastic", ponumber);
                    var response = await _getDataService.UpdateItem<POFineLineOutput>(existing.POFineLine, _poIndex);

                    _logger.LogDebug("Updated data for {PONumber} in elastic", ponumber);
                }
                // existing.POAPL.ActivityCode = model.GetType() == typeof(MMSPOCreatedEvent) ? "A" : "U";

                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertPOFineLine- Failed Updating FineLinePO: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }

        }

        private async Task<POFineLineResult> GetExistingPO(string ponumber)
        {
            POFineLineResult poresult = new POFineLineResult();
            DataResult result = new DataResult();

            ////call elastic to see if data is present. otherwise return data from DB
            //_logger.LogInformation("Going to call Elastic to retrieve data for {PONumber} ", ponumber);
            //var doc = await _getDataService.GetItem<POFineLineOutput, POO>(ponumber, _poIndex);
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
            //            var obtecttobemapped = propValue as POFineLineOutput;
            //            poresult.POFineLine = obtecttobemapped;
            //            _logger.LogDebug("Retrieved data from Elastic for {PONumber} ", ponumber);
            //            break;
            //        }
            //    }
            //    //replace POSku data from DB as there are no posku events raised. hence elastic may not have the latest posku data at all.
            //    //Need to merge posku data from DB with POsku data for that PO from DB

            //    if (poresult.POFineLine != null)
            //    {
            //        var poskus = await _GetPOSkuLines.GetPOSkusfromDBFromPONumber(ponumber);
            //        _logger.LogInformation("PO Queryservice Returned Data.", poskus);
            //        if (poskus != null && poskus.Count > 0)
            //        {
            //            poresult.POFineLine.UpdatePOFineLinePoSkuData(poskus);
            //        }
            //    }
            //}
            //else
            //{
                var obtecttobemapped = await GetPOfromDB(ponumber);
                poresult.POFineLine = obtecttobemapped;
            //}
            return poresult;
        }

        private async Task<POFineLineOutput> GetPOfromDB(string ponumber)
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
                            .Filter(x => x.PONumber == po && (x.StatusCode == "OP" || x.StatusCode == "CN"))//filter conditions
                            .FindEntryAsync();

                //call below only if conditions matched
                if (pocheck != null)
                {
                    var inputProduct = await client
                            .For<POO>()
                            .Key(po)
                            .Expand("POSkus/POProduct")
                            .FindEntryAsync();

                    _logger.LogInformation("PO Queryservice Returned Data.-- {inputProduct}", inputProduct);

                    var output = inputProduct.MaptoOutput();

                    //remove posku that has 0 retailprice

                    var poskustoberemoved = output.POSkus?.Where(x => x.POProduct.TicketRetail == "0");
                    _logger.LogInformation("POskus ignored.-- {poskustoberemoved}", poskustoberemoved?.ToList());

                    output.POSkus?.RemoveAll(x => x.POProduct.TicketRetail=="0");

                    return output;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed Mapping POFineLine: {Reason}", ex.Message);
                return null;
            }
        }

        public async Task<ApiResult<string>> UpsertPOFineLineLookupCode(SG.MMS.LookupCode.Events.LookupCodeEvent model)
        {

            try
            {
                string labelType = model.Code;
                if (model.CodeType != "LABL")
                {
                    _logger.LogInformation("LookUp Code Type is not 'LABL' type. Passed in {Code}. Hence skipped further processing.", labelType);
                    return new ApiResult<string>();
                }
                _logger.LogInformation("Passed in {Code}.", labelType);
                _logger.LogDebug("Fetching existing data for {LookUpCode}", labelType);
                var existingPOs = await GetProductVendorsByLabel(labelType);

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
                        if (existingPOresult != null && existingPOresult.POFineLine != null
                            && existingPOresult.POFineLine.POSkus != null && existingPOresult.POFineLine.POSkus.Count() > 0)
                        {
                            existingPOresult.POFineLine.POSkus.ForEach(p =>
                            {
                                if (p.POProduct != null && eachpo.Value.Contains(p.SKUNumber))
                                {
                                    p.POProduct.TicketDescription = model.ShortDescription;
                                }
                            });

                            //update data in elastic
                            _logger.LogDebug("Going to update data for {PONumber} in elastic", eachpo.Key);
                            var response = await _getDataService.UpdateItem<POFineLineOutput>(existingPOresult.POFineLine, _poIndex);
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
                    _logger.LogDebug("No data found for LookUp: {LookUpCode}", labelType);
                }

                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertPOFineLineLookupCode- Failed Updating FineLinePO: {Reason}", ex.Message);
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

        public async Task<ApiResult<string>> UpsertPOFineLineProductRetail(MMS.Product.Retail.Events.MMSProductRetailEvent model)
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
                        if (existingPOresult != null && existingPOresult.POFineLine != null
                            && existingPOresult.POFineLine.POSkus != null && existingPOresult.POFineLine.POSkus.Count() > 0)
                        {
                            existingPOresult.POFineLine.POSkus.ForEach(x =>
                            {
                                if (
                                    x.SKUNumber == model.Sku && model.RetailType == "Ticket" && model.CurrencyCode == "USD" &&
                                    x.PurchaseOrderDate != null && !string.Equals(x.PurchaseOrderDate.Value.Date, DateTime.Today.Date)//!string.Equals(x.PurchaseOrderDate.ToString("MM/dd/yy"), DateTime.Today.ToString("MM/dd/yy"))
                                    )
                                {
                                    x.POProduct.TicketRetail = model.Retail.GetRetailPrice();
                                }
                            });
                            //update data in elastic
                            _logger.LogDebug("Going to update data for {PONumber} in elastic", eachpo.PONumber);
                            var response = await _getDataService.UpdateItem<POFineLineOutput>(existingPOresult.POFineLine, _poIndex);
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
                _logger.LogError(ex, "UpsertPOFinLineProductRetail - Failed Updating FineLinePO: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }
        }
    }
}
