using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using SG.PO.Intertek.DataModels.Outputmodels;
using SG.PO.Intertek.ProcessingService.Services;
using SG.Shared.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Xml;
using System.IO;

namespace SG.PO.Intertek.ProcessingService
{
    public class ElasticWriter
    {
        private readonly ElasticClient _client;
        private readonly POIntertekUtilities _pointertekutilities;
        private readonly IOptions<OutputSettings> _outputSettings;
        private readonly ILogger _logger;
        private readonly string _currentIndex;
        private readonly string _previouIndex;
        private readonly string _archiveIndex;
        public ElasticWriter(ElasticClient client, POIntertekUtilities pointertekutilities, IOptions<OutputSettings> outputSettings, ILogger<ElasticWriter> logger, IConfiguration configuration)
        {
            _client = client;
            _logger = logger;
            _pointertekutilities = pointertekutilities;
            _outputSettings = outputSettings;
            _currentIndex = configuration.GetValue<string>("CurrentIndex");
            _previouIndex = configuration.GetValue<string>("PreviousIndex");
            _archiveIndex= configuration.GetValue<string>("ArchiveIndex");
        }

        public void WriteFile<TDoc>(TDoc document, string opFilePath) where TDoc : class
        {

            //read the config file for start and end positions
            //var configfile = XDocument.Load(_outputSettings.Value.POIntertekConfigFile);
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(string.Format("{0}.Helper.POIntertekConfig.xml", typeof(ElasticWriter).Namespace)); // Embedded resource config file
            var configfile = XDocument.Load(stream, LoadOptions.None);

            List<POIntertekOutput> polist = new List<POIntertekOutput>();
            var intertekpo = document as POIntertekOutput;
            polist.Add(intertekpo);

            //flatten the object for output into a text file
            var flattenned = polist.SelectMany(po => po.POSkus
                      .Select(posku => new FlattenedPOIntertek
                      {
                          ActivityCode = posku.ActivityCode,
                          ConSignee = GetConSigneeByLOB(po.LOB),
                          LOB = po.LOB,
                          PONumber = po.PONumber,
                          DeliveryDate = po.DeliveryDate,
                          CalculatedField1 = (posku.POProduct?.ClassCode == "D" || po.DeliveryDate == null || po.ShipDate == null || po.CurrencyCode != "USD") ?
                                                    string.Empty : ((DateTime.ParseExact(po.DeliveryDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture)
                                                    .Subtract((DateTime.ParseExact(po.ShipDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture).Date)).TotalDays < 38) ? "W" : "E"),
                          SKU = posku.SKU,
                          CalculatedField2 = !string.IsNullOrEmpty(po.ShipDate) ? (DateTime.ParseExact(po.ShipDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture).Date.AddDays(-5)).ToString("yyyyMMdd") : string.Empty,
                          CancelDate = po.CancelDate,
                          Country = posku.POProduct?.Country,
                          BuyQuantity = posku.BuyQuantity,
                          LocationNumber = po.LocationNumber,
                          SkuDesc = posku.POProduct?.SkuDesc,
                          MasterPackQuantity = posku.POProduct?.MasterPackQuantity,
                          ClassCode = posku.POProduct?.ClassCode,
                          HTSCode = posku.POProduct?.HTSCode,
                          VendorName = po.VendorName,
                          ShipDate = po.ShipDate,
                          SubVendorNumber = po.SubVendorNumber,
                          CalculatedField3 = !string.IsNullOrEmpty(posku.ApprovalLetter) ? posku.ApprovalLetter : "N",
                          EmployeeId = po.EmployeeId,
                          DistributorId = po.DistributorId,
                          CreateDate = posku.CreateDate,
                          CalculatedField4 = (posku.POProduct != null && posku.POProduct?.ProductFlagsOutput != null && !string.IsNullOrEmpty(posku.POProduct?.ProductFlagsOutput.RandomInspectionRequired)) ? (posku.POProduct?.ProductFlagsOutput.RandomInspectionRequired) : string.Empty,
                          MasterPackCubicFeet = posku.MasterPackCubicFeet,
                          DutyPctOfFOB = posku.DutyPctOfFOB,
                          DutyCost = posku.DutyCost,
                          EstimatedLandedCost = posku.EstimatedLandedCost,
                          FirstCost = posku.FirstCost,
                          DepartmentName = (posku.POProduct != null) ? posku.POProduct?.DepartmentName : string.Empty,
                          ClassName = (posku.POProduct != null) ? posku.POProduct?.ClassShortDesc : string.Empty,
                          DepartmentCode = (posku.POProduct != null) ? posku.POProduct?.DepartmentCode : string.Empty,
                          CalculatedField5 = (posku.POProduct != null && posku.POProduct?.ProductFlagsOutput != null) ? ((posku.POProduct?.ProductFlagsOutput.IsChildClothCostume == "Y" || posku.POProduct?.ProductFlagsOutput.NonPaintTestingRequired == "Y" ||
                                              posku.POProduct?.ProductFlagsOutput.CPSIATestingRequired == "Y" || posku.POProduct?.ProductFlagsOutput.IntlSafetyTransitTestRequired == "Y"
                                              || posku.POProduct?.ProductFlagsOutput.IsGlassDishAdultJewelry == "Y") ? "Y" : "N") : string.Empty,
                          PrepackId = posku.PrepackId,
                          CalculatedField6 = string.IsNullOrEmpty(posku.PrepackId) ? "   " : posku.POProduct?.PrepackChildQuantity,
                          CalculatedField7 = ComputeCalculatedField7(posku),
                          CPSIATestingRequired = posku.POProduct?.ProductFlagsOutput.CPSIATestingRequired,
                          NonPaintTestingRequired = posku.POProduct?.ProductFlagsOutput.NonPaintTestingRequired,
                          IsChildClothCostume = posku.POProduct?.ProductFlagsOutput.IsChildClothCostume,
                          IsGlassDishAdultJewelry = posku.POProduct?.ProductFlagsOutput.IsGlassDishAdultJewelry,
                          IntlSafetyTransitTestRequired = posku.POProduct?.ProductFlagsOutput.IntlSafetyTransitTestRequired,
                          PreDistributionQty = "0"
                      }
                      ));

            //write data into txt output
            _pointertekutilities.WriteTxtFormat(flattenned.ToList(), configfile.Descendants("WriteFixedWidth").FirstOrDefault(), opFilePath);
        }
        
        public async Task<ApiResult> WriteFileAsync<TDoc>() where TDoc : class
        {
            try
            {
                //Log message if directory not found and return
                if (!Directory.Exists(Path.GetDirectoryName(_outputSettings.Value.OutputFilePath)))
                {
                    _logger.LogError("WriteFileAsync - No Directory found with given path- POIntertek: {Reason}", _outputSettings.Value.OutputFilePath);
                    return new ApiResult<string>();
                }

                string path = GetOutputFilePath();
                if (string.IsNullOrEmpty(path))
                {
                    _logger.LogError("WriteFileAsync - Output path not specified.");
                    return new ApiResult<string>();
                }

                bool atLeastOneRecord = false;

                //reads for that day from elastic
                //reads in chunks of 1000, can be changed 
                var docCount = _client.Count<TDoc>(c => c.Index(_currentIndex));
                _logger.LogInformation($"WriteFileAsync - Inspecting {docCount.Count} documents for changes...");
                var searchResponse = _client.Search<TDoc>(s => s
                                    .Size(1000)
                                    .Scroll("1m")
                                    .Index(_currentIndex));

                var outputType = _outputSettings.Value.Type.ToLower();

                switch (outputType)
                {
                    case "text":
                    case "txt":

                        while (searchResponse.Documents.Any())
                        {
                            foreach (var document in searchResponse.Documents)
                            {
                                //Read from previous index for the document. 
                                //Compare the current and previous. If there is any difference between POs/POskus send that
                                var pointertekoutputcurrent = document as POIntertekOutput;
                                //check if forceinclude has been set as true for the PO
                                if (pointertekoutputcurrent.ForceInclude)
                                {
                                    if (pointertekoutputcurrent.POSkus != null && pointertekoutputcurrent.POSkus.Count > 0)
                                    {
                                        pointertekoutputcurrent.POSkus.ForEach(posku =>
                                    {
                                        posku.ActivityCode = "A";
                                    });
                                        WriteFile<POIntertekOutput>(pointertekoutputcurrent, path);
                                        atLeastOneRecord = true;
                                        pointertekoutputcurrent.ForceInclude = false;
                                        var response = await _client.UpdateAsync<POIntertekOutput, object>(pointertekoutputcurrent, u => u.Doc(pointertekoutputcurrent).Index(_currentIndex).DocAsUpsert());
                                    }
                                    else
                                    {
                                        _logger.LogInformation($"WriteFileAsync - Skipping output for PO - {pointertekoutputcurrent.PONumber}; no SKUs were found.");
                                    }
                                }
                                else
                                {
                                    if (pointertekoutputcurrent.StatusCode != "CL")
                                    {
                                        var doc = await _client.GetAsync<TDoc>(new DocumentPath<TDoc>(pointertekoutputcurrent.PONumber), g => g.Index(_previouIndex));

                                        if (doc.Source != null)
                                        {
                                            var pointertekoutputprev = doc.Source as POIntertekOutput;
                                            var compareddata = _pointertekutilities.CompareIntertekData(pointertekoutputcurrent, pointertekoutputprev);
                                            if (compareddata != null && compareddata.POSkus != null && compareddata.POSkus.Count > 0)
                                            {
                                                _logger.LogInformation($"WriteFileAsync - Outputting changes to {pointertekoutputcurrent.PONumber} to file.");
                                                WriteFile<POIntertekOutput>(compareddata, path);
                                                atLeastOneRecord = true;
                                            }
                                            else
                                            {
                                                _logger.LogInformation($"WriteFileAsync - Skipping output for {pointertekoutputcurrent.PONumber}; no changes were found.");
                                            }
                                        }
                                        //PO data does not exist in previous, hence need to be added
                                        else
                                        {
                                            var podatanew = document as POIntertekOutput;
                                            if (podatanew.POSkus != null && podatanew.POSkus.Count > 0)
                                            {
                                                _logger.LogInformation($"WriteFileAsync - Outputting {pointertekoutputcurrent.PONumber} to file as new purchase order.");
                                                podatanew.POSkus.ForEach(x => { x.ActivityCode = "A"; });
                                                WriteFile<POIntertekOutput>(podatanew, path);
                                                atLeastOneRecord = true;
                                            }
                                            else
                                            {
                                                _logger.LogInformation($"WriteFileAsync - Skipping output for PO - {podatanew.PONumber}; no SKUs were found.");
                                            }
                                        }
                                    }
                                }
                                if (atLeastOneRecord)
                                {
                                    //save it to archive index before delete from current
                                    var archiveresponse = await _client.UpdateAsync<POIntertekOutput, object>(pointertekoutputcurrent, u => u.Doc(pointertekoutputcurrent).Index(_archiveIndex).DocAsUpsert());

                                    //Save to Previous Index from Current
                                    var previousresponse = await _client.UpdateAsync<POIntertekOutput, object>(pointertekoutputcurrent, u => u.Doc(pointertekoutputcurrent).Index(_previouIndex).DocAsUpsert());

                                    //delete from current
                                    var deletedcurrent = await _client.DeleteAsync<POIntertekOutput>(pointertekoutputcurrent, u => u.Index(_currentIndex));

                                    _logger.LogInformation($"WriteFileAsync - Archived Intertek Index Successfully -- {pointertekoutputcurrent.PONumber}.");
                                }
                                else
                                {
                                    _logger.LogInformation($"WriteFileAsync - No file updated. So, did not Archived Intertek Index -- {pointertekoutputcurrent.PONumber}.");
                                }

                                _logger.LogInformation($"WriteFileAsync - Successfully Processed PONumber -- {pointertekoutputcurrent.PONumber} .");
                            }

                            //CurrentCount++;
                            searchResponse = _client.Scroll<TDoc>("1m", searchResponse.ScrollId);
                        }

                        break;
                    default:
                        _logger.LogError($"WriteFileAsync - Invalid output type {outputType} specified.");
                        return new ApiResult<string>();

                }

                _client.ClearScroll(c => c.ScrollId(searchResponse.ScrollId));

                //Make sure the file was actually written to disk before finalizing
                if (atLeastOneRecord && !File.Exists(path))
                    _logger.LogError($"WriteFileAsync - Output failed to write successfully to {path}");
                //else
                /////reindex
                //ReIndex<TDoc>();

                return new ApiResult<string>();
            }
            //rename/repoint the alias to the new index
            catch (Exception ex)
            {
                _logger.LogError("Method Name -- WriteFileAsync: {Reason}", ex.Message);
                return new ApiResult<string>(new[] { ex.Message });
            }
        }

        private string GetOutputFilePath()
        {
            string path = string.Empty;
            var OpFileNamedateFormat = _outputSettings.Value.OutputFileNameDateFormat;
            if (_outputSettings.Value.OutputFilePath.Contains("{0}"))
            {
                path = string.Format(_outputSettings.Value.OutputFilePath, DateTime.Now.ToString(OpFileNamedateFormat));
            }
            else
            {
                bool fileExists = File.Exists(_outputSettings.Value.OutputFilePath);

                if (fileExists)
                {
                    string newFileName = string.Format(Path.GetFileNameWithoutExtension(_outputSettings.Value.OutputFilePath) + "_{0}" + Path.GetExtension(_outputSettings.Value.OutputFilePath), DateTime.Now.ToString(OpFileNamedateFormat));
                    path = Path.Combine(Path.GetDirectoryName(_outputSettings.Value.OutputFilePath), newFileName);
                }
                else
                    path = _outputSettings.Value.OutputFilePath;
            }

            return path;
        }

        private void ReIndex<TDoc>() where TDoc : class
        {
            try
            {
                //will update existing document from source to destination.
                //will add missing documents

                var reindexResponse = _client.ReindexOnServer(r => r
                                                    .Source(s => s
                                                        .Index(_currentIndex)
                                                    )
                                                    .Destination(d => d
                                                    .Index(_previouIndex)
                                                    .VersionType(Elasticsearch.Net.VersionType.ExternalGte)
                                                    )
                                                    .WaitForCompletion(true)
                                                    );


            }
            catch (Exception ex)
            {
                _logger.LogError("Method Name -- ReIndex", ex, ex.Message);
            }
        }

        private string ComputeCalculatedField7(POIntertekSKUOutput sku)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(sku.POProduct?.PrepackTotalQuantity) && sku.POProduct?.PrepackTotalQuantity != "0")
                result = string.IsNullOrEmpty(sku.PrepackId) ? "0" : (100 * (!string.IsNullOrEmpty(sku.POProduct?.PrepackChildQuantity) ? Convert.ToInt32(sku.POProduct?.PrepackChildQuantity) : 0) / Convert.ToInt32(sku.POProduct?.PrepackTotalQuantity)).ToString();
            else
                result = string.Empty;
            return result;
        }

        private string GetConSigneeByLOB(string LOB)
        {
            switch (LOB)
            {
                case "01":
                    return "06049";
                case "02":
                    return "07041";
                case "03":
                    return "05609";
                case "04":
                    return "05007";
                case "05":
                    return "05332";
                case "06":
                    return "03472";
                case "08":
                    return "06049";
                default:
                    return "";
            }
        }

    }
}

