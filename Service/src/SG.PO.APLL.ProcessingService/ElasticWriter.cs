using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using SG.PO.APLL.ProcessingService.Services;
using System.Xml.Linq;
using SG.PO.APLL.DataModel.Outputmodels;
using System.Threading.Tasks;
using SG.Shared.Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Xml;
using System.IO;

namespace SG.PO.APLL.ProcessingService
{
    public class ElasticWriter
    {
        private readonly ElasticClient _client;
        private readonly POAPLUtilities _poaplutilities;
        private readonly IOptions<OutputSettings> _outputSettings;
        private readonly ILogger _logger;
        private readonly string _currentIndex;
        private readonly string _previouIndex;
        private readonly string _archiveIndex;

        public ElasticWriter(ElasticClient client, IOptions<OutputSettings> outputSettings, ILogger<ElasticWriter> logger,
            POAPLUtilities poaplutilities, IConfiguration configuration)
        {
            _client = client;
            _outputSettings = outputSettings;
            _logger = logger;
            _poaplutilities = poaplutilities;
            _currentIndex = configuration.GetValue<string>("CurrentIndex");
            _previouIndex = configuration.GetValue<string>("PreviousIndex");
            _archiveIndex = configuration.GetValue<string>("ArchiveIndex");
        }

        public void WriteFile<TDoc>(TDoc document, string opFilePath) where TDoc : class
        {

            //read the config file for start and end positions
            //var configfile = XDocument.Load(_outputSettings.Value.ConfigFile);
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(string.Format("{0}.Helper.POAPLL.xml", typeof(ElasticWriter).Namespace)); // Embedded resource config file
            var configfile = XDocument.Load(stream, LoadOptions.None);

            List<POAPLLOutput> polist = new List<POAPLLOutput>();
            var poapl = document as POAPLLOutput;
            polist.Add(poapl);

            //flatten the object for output into a text file
            var flattenned = polist.SelectMany(po => po.POSkus
                      .Select(posku => new FlattenedPOAPL
                      {
                          ActivityCode = posku.ActivityCode,
                          ConsigneeNumber = GetConSigneeByLOB(po.LOB),
                          PONumber = posku.PONumber,
                          WarehouseDueDate = po.WarehouseDueDate,
                          ItemNumber = posku.ItemNumber,
                          EarlyShipDate = po.EarlyShipDate,
                          LastShipDate = po.LastShipDate,
                          CountryofOrigin = posku.POProduct?.CountryOfOrigin,
                          ItemQty = posku.ItemQty,
                          StoreCode = po.StoreCode,
                          ItemDescription = posku.POProduct?.ItemDescription,
                          ItemTotalQuantity = posku.ItemTotalQuantity,
                          CasePackQty = posku.POProduct?.CasePackQty,
                          ClassCode = posku.POProduct?.ClassCode,
                          TariffCode = posku.POProduct?.TariffCode,
                          VendorName = po.VendName,
                          VendorNumber = posku.POProduct?.VendorName,
                          BuyerCode = po.BuyerCode,
                          POCreationDate = posku.CreateDate,
                          DutyPerPiece = posku.DutyCost,
                          UnitCost = posku.UnitCost,
                          VendorShipDate = po.VendorShipDate,
                          DeparmentName = posku.POProduct?.DepartmentName,
                          ClassName = posku.POProduct?.ClassName,
                          DepartmentCode = posku.POProduct?.DepartmentCode,
                          ApprovalLetter = posku.ApprovalLetter,
                          SamplesRequired = posku.SamplesRequired,
                          EstimatedLandedCost = posku.EstimatedLandedCost,
                          MasterPackCubicFeet = posku.MasterPackCubicFeet,
                          DutyPctOfFOB = posku.DutyPctOfFOB,
                          LOB=po.LOB

                      }
                      ));

            //write data into txt output
            _poaplutilities.WiteTxtFormat(flattenned.ToList(), configfile.Descendants("WriteFixedWidth").FirstOrDefault(), opFilePath);
        }

        public async Task<ApiResult> WriteFileAsync<TDoc>() where TDoc : class
        {

            try
            {
                //Log message if directory not found and return
                if (!Directory.Exists(Path.GetDirectoryName(_outputSettings.Value.OutputFilePath)))
                {
                    _logger.LogError("WriteFileAsync - No Directory found with given path- POAPLL: {Reason}", _outputSettings.Value.OutputFilePath);
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
                //reads in chunks of 100, can be configured 
                var docCount = _client.Count<TDoc>(c => c.Index(_currentIndex));                
                _logger.LogInformation($"WriteFileAsync - Inspecting {docCount.Count} documents for changes...");
                var searchResponse = _client.Search<TDoc>(s => s
                                    .Size(100)
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
                                var outputcurrent = document as POAPLLOutput;                                
                                    //check if forceinclude has been set as true for the PO
                                    if (outputcurrent.ForceInclude)
                                    {
                                        if (outputcurrent.POSkus != null && outputcurrent.POSkus.Count > 0)
                                        {
                                            outputcurrent.POSkus.ForEach(posku =>
                                            {
                                                posku.ActivityCode = "A";
                                            });
                                            WriteFile<POAPLLOutput>(outputcurrent, path);
                                            //set the forceinclude flag to false and save in elastic
                                            atLeastOneRecord = true;
                                            outputcurrent.ForceInclude = false;
                                            var response = await _client.UpdateAsync<POAPLLOutput, object>(outputcurrent, u => u.Doc(outputcurrent).Index(_currentIndex).DocAsUpsert());
                                        }
                                        else
                                        {
                                             _logger.LogInformation($"WriteFileAsync - Skipping output for PO - {outputcurrent.PONumber}; no SKUs were found.");
                                        }
                                    }
                                    else
                                    {
                                        if (outputcurrent.StatusCode != "CL")
                                        {
                                            var doc = await _client.GetAsync<TDoc>(new DocumentPath<TDoc>(outputcurrent.PONumber), g => g.Index(_previouIndex));
                                            if (doc.Source != null)
                                            {
                                                var outputprevious = doc.Source as POAPLLOutput;
                                                var compareddata = _poaplutilities.CompareAPLLData(outputcurrent, outputprevious);
                                                if (compareddata != null && compareddata.POSkus != null && compareddata.POSkus.Count > 0)
                                                {
                                                    _logger.LogInformation($"WriteFileAsync - Outputting changes to {outputcurrent.PONumber} to file.");
                                                    WriteFile<POAPLLOutput>(compareddata, path);
                                                    atLeastOneRecord = true;
                                                }
                                                else
                                                {
                                                    _logger.LogInformation($"WriteFileAsync - Skipping output for {outputcurrent.PONumber}; no changes were found.");
                                                }

                                            }
                                            //PO data does not exist in previous, hence need to be added
                                            else
                                            {
                                                var podatanew = document as POAPLLOutput;
                                                if (podatanew.POSkus != null && podatanew.POSkus.Count > 0)
                                                {
                                                    _logger.LogInformation($"WriteFileAsync - Outputting {outputcurrent.PONumber} to file as new purchase order.");
                                                    podatanew.POSkus.ForEach(x => { x.ActivityCode = "A"; });
                                                    WriteFile<POAPLLOutput>(podatanew, path);
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
                                    var archiveresponse = await _client.UpdateAsync<POAPLLOutput, object>(outputcurrent, u => u.Doc(outputcurrent).Index(_archiveIndex).DocAsUpsert());

                                    //Save to Previous Index from Current
                                    var previousresponse = await _client.UpdateAsync<POAPLLOutput, object>(outputcurrent, u => u.Doc(outputcurrent).Index(_previouIndex).DocAsUpsert());

                                    //delete from current
                                    var deletedcurrent = await _client.DeleteAsync<POAPLLOutput>(outputcurrent, u => u.Index(_currentIndex));

                                    _logger.LogInformation($"WriteFileAsync - Archived APLL Index Successfully -- {outputcurrent.PONumber}.");
                                }
                                else
                                {
                                    _logger.LogInformation($"WriteFileAsync - No file updated. So, did not Archived APLL Index -- {outputcurrent.PONumber}.");
                                }

                                _logger.LogInformation($"WriteFileAsync - Successfully Processed PONumber -- {outputcurrent.PONumber} .");
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
                //{
                //    ///reindex
                //    ReIndex<TDoc>();
                //}

                return new ApiResult<string>();

            }
            catch (Exception ex)
            {
                _logger.LogError("Method Name -- WriteFileAsync: {Reason}", ex, ex.Message);
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

        private string GetConSigneeByLOB(string LOB)
        {
            switch (LOB)
            {
                case "01":
                    return "06049";                
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
