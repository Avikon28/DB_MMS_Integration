using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.Chino.CommandService.Core.Mapper;
using SG.PO.Chino.CommandService.Core.OutputModels;
using SG.PO.Chino.DataModels.Outputmodels;
using SG.Shared.Api;
using SG.Shared.ModelCache;
using SG.Shared.ModelCache.models;
using SG.Shared.POProduct;
using SG.Shared.POProduct.Services;
using Simple.OData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SG.PO.Chino.CommandService.Core.Services
{
    public class POChinoService
    {
        private static IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly string _index;
        private readonly IOptions<QueryService> _config;

        private ODataClient client = null;
        private readonly IDataService _getDataService;
        private readonly GetPOSkuLines _getPOSkuLines;
        private const string _locationNumber = "WHS02";

        public POChinoService(IConfiguration configuration, IDataService getDataService, ILogger<POChinoService> logger,
            IOptions<QueryService> config, GetPOSkuLines getPOSkuLines)
        {
            _configuration = configuration;
            _logger = logger;
            _config = config;
            _getDataService = getDataService;
            _getPOSkuLines = getPOSkuLines;
            _index = _configuration.GetValue<string>("CurrentIndex");
        }

        public async Task<ApiResult<string>> UpsertPOChino(MMS.PO.Events.MMSPOEvent model)
        {

            try
            {
                string ponumber = model.PONumber;
                _logger.LogInformation("Passed in {PONumber}.", model.PONumber);

                _logger.LogDebug("Fetching existing data for {PONumber}", ponumber);
                var existing = await GetExistingPO(ponumber);

                if (existing.POChino != null)
                {
                    existing.POChino = model.MapEventtoOutput(existing.POChino);
                    if (existing.POChino.POSkus != null && existing.POChino.POSkus.Count > 0)
                    {
                        existing.POChino.POSkus = existing.POChino.POSkus.FindAll(x => Convert.ToInt32(x.OrderQty) > 0);
                    }
                    _logger.LogInformation("Going to update data for {PONumber} in elastic", ponumber);
                    var response = await _getDataService.UpdateItem<POChinoOutput>(existing.POChino, _index);

                    _logger.LogDebug("Updated data for {PONumber} in elastic", ponumber);
                }

                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertPOChino- Failed Updating ChinoPO: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }

        }

        public async Task<ApiResult<string>> UpsertPOChinoPOSku(MMS.PO.Events.MMSPOSkuEvent model)
        {
            try
            {
                string ponumber = model.PONumber;
                _logger.LogInformation("Passed in {PONumber}.", model.PONumber);

                _logger.LogDebug("Fetching existing data for {PONumber}", ponumber);
                var existing = await GetExistingPO(ponumber);
                if (existing.POChino != null)
                {                    
                    existing.POChino = model.MapEventtoOutput(existing.POChino);
                    if (existing.POChino.POSkus != null && existing.POChino.POSkus.Count > 0)
                    {
                        existing.POChino.POSkus = existing.POChino.POSkus.FindAll(x => Convert.ToInt32(x.OrderQty) > 0);
                    }
                    _logger.LogDebug("Going to update data for {PONumber} in elastic", ponumber);
                    
                    var response = await _getDataService.UpdateItem<POChinoOutput>(existing.POChino, _index);

                    _logger.LogDebug("Updated data for {PONumber} in elastic", ponumber);
                    //}
                }
                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertPOChinoPOSku-Failed Updating APLPO: {Reason}", ex.Message);
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
                if (existing.POChino != null)
                {

                    existing.POChino.ForceInclude = true;
                    _logger.LogDebug("Going to update data for {PONumber} in elastic", ponumber);
                    var response = await _getDataService.UpdateItem<POChinoOutput>(existing.POChino, _index);

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
        private async Task<POChinoResult> GetExistingPO(string ponumber)
        {
            POChinoResult poresult = new POChinoResult();
            DataResult result = new DataResult();

            ////call elastic to see if data is present. otherwise return data from DB
            //_logger.LogDebug("Going to call Elastic to retrieve data for {PONumber} ", ponumber);
            //var doc = await _getDataService.GetItem<POChinoOutput, POO>(ponumber, _index);

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
            //            var obtecttobemapped = propValue as POChinoOutput;
            //            poresult.POChino = obtecttobemapped;
            //            _logger.LogDebug("Retrieved data from Elastic for {PONumber} ", ponumber);
            //            break;
            //        }
            //    }
            //    //replace POSku data from DB as there are no posku events raised. hence elastic may not have the latest posku data at all.
            //    //Need to merge posku data from DB with POsku data for that PO from DB
            //    var poskus = await _getPOSkuLines.GetPOSkusfromDBFromPONumber(ponumber);

            //    if (poskus != null && poskus.Count > 0)
            //    {
            //        poresult.POChino.UpdatePOChinoPoSkuData(poskus);
            //    }
            //}
            //else
            //{
                var obtecttobemapped = await GetPOfromDB(ponumber);
                poresult.POChino = obtecttobemapped;
            //}
            return poresult;
        }

        private async Task<POChinoOutput> GetPOfromDB(string ponumber)
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
                            .Filter(x => x.PONumber == po && x.LocationNumber == _locationNumber && (x.StatusCode == "OP" || x.StatusCode == "VD"))
                            .FindEntryAsync();

                //call below only if conditions matched
                if (pocheck != null)
                {
                    var inputProduct = await client
                                .For<POO>()
                                .Key(po)
                                .Expand("POSkus")
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
                _logger.LogError(ex, "Failed Mapping POChino: {Reason}", ex.Message);
                return null;
            }
        }
    }
}
