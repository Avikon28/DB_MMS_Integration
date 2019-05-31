using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Options;
using SG.PO.Contempo.ProcessingService.Services;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;
using System.IO;
using Newtonsoft.Json.Linq;
using SG.PO.Contempo.ProcessingService.Helper;
using SG.PO.Contempo.DataModels.Outputmodels;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using SG.Shared.Api;
using SG.PO.Contempo.ProcessingService.OutputModels;
using Microsoft.Extensions.Configuration;

namespace SG.PO.Contempo.ProcessingService
{
    public class ElasticWriter
    {
        private readonly ElasticClient _client;
        private readonly POContempoUtilities _poCtnmpoOutput;
        private readonly IOptions<OutputSettings> _outputSettings;
        private readonly ILogger _logger;
        private readonly string _currentIndex;
        private readonly string _previousIndex;
        private readonly string _archiveIndex;
        public ElasticWriter(ElasticClient client, IOptions<OutputSettings> outputSettings, ILogger<ElasticWriter> logger,
                               POContempoUtilities poCtnmpoOutput, IConfiguration configuration)
        {
            _client = client;
            _poCtnmpoOutput = poCtnmpoOutput;
            _outputSettings = outputSettings;
            _logger = logger;
            _currentIndex = configuration.GetValue<string>("CurrentIndex");
            _previousIndex = configuration.GetValue<string>("PreviousIndex");
            _archiveIndex = configuration.GetValue<string>("ArchiveIndex");
        }

        public void WriteFile<TDoc>(TDoc document, string opFilePath) where TDoc : class
        {
            //read the config file
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream(string.Format("{0}.Helper.POContempo.xml", typeof(ElasticWriter).Namespace)); // Embedded resource config xml file
            var configfile = XDocument.Load(stream, LoadOptions.None);

            List<POContempoOutput> polist = new List<POContempoOutput>();
            var pocntmp = document as POContempoOutput;
            polist.Add(pocntmp);

            //flatten the object for output into a text file
            var flattenned = polist.SelectMany(po => po.POSkus
                      .Select(posku => new FlattenedPOContempo
                      {
                          ActivityCode = posku.ActivityCode,
                          TicketRetail = posku.ActivityCode == "" ? posku.POProduct?.RetailPrice.ToString() : "",
                          TicketType = posku.ActivityCode == "" ? posku.POProduct?.LabelType : "",
                          TicketDescription = posku.ActivityCode == "" ? posku.POProduct?.LabelDescription : "",
                          PurchaseOrderDate = posku.CreateDate != null ? posku.CreateDate.Value.ToString("MM/dd/yyyy") : "",
                          PurchaseOrderReviseDate = posku.ActivityCode == "" ? (posku.ModifiedDate != null ? posku.ModifiedDate.Value.ToString("MM/dd/yyyy") : "") : "",
                          ClassID = posku.ActivityCode == "" ? posku.POProduct?.Class : "",
                          ISOCountryCode = posku.ActivityCode == "" ? posku.POProduct?.CountryOfOrigin : "",
                          Currency = posku.ActivityCode == "" ? po.CurrencyCode : "",
                          SKUNumber = posku.SKU,
                          PurchaseOrder = po.PONumber,
                          SubClassID = posku.ActivityCode == "" ? posku.POProduct?.SubClass : "",
                          VendorNumber = posku.POProduct?.APVendor,
                          SubVendorNumber = po.SubVendor,
                          VendorStyleNumber = posku.ActivityCode == "C" ? "" : posku.POProduct?.VendorSKUCode,
                          OrderQuantity = posku.ActivityCode == "" ? posku.BuyQuanity.ToString() : "",
                          ClassDescription = posku.ActivityCode == "" ? posku.POProduct?.ClassLevelDesc : "",
                          Size = posku.ActivityCode == "" ? posku.POProduct?.Size : "",
                          SKUDescription = posku.ActivityCode == "" ? posku.POProduct?.SkuDesc : "",
                          SubClassDescription = posku.ActivityCode == "" ? posku.POProduct?.SubClassLevelDesc : ""
                      }
                ));

            _poCtnmpoOutput.WriteTxtFormat(flattenned.ToList(), configfile.Descendants("WriteFixedWidth").FirstOrDefault(), opFilePath);
        }

        public async Task<ApiResult> WriteFileAsync<TDoc>() where TDoc : class
        {
            try
            {
                //Log message if directory not found and return
                if (!Directory.Exists(Path.GetDirectoryName(_outputSettings.Value.OutputFilePath)))
                {
                    _logger.LogError("WriteFileAsync - No Directory found with given path- POContempo: {Reason}", _outputSettings.Value.OutputFilePath);
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
                                var poctmpoutputcurrent = document as POContempoOutput;
                                if (poctmpoutputcurrent.ForceInclude)
                                {
                                    if (poctmpoutputcurrent.POSkus != null && poctmpoutputcurrent.POSkus.Count > 0)
                                    {
                                        poctmpoutputcurrent.POSkus.ForEach(posku =>
                                        {
                                            posku.ActivityCode = "";
                                        });
                                        WriteFile<POContempoOutput>(poctmpoutputcurrent, path);
                                        atLeastOneRecord = true;
                                        poctmpoutputcurrent.ForceInclude = false;
                                        var response = await _client.UpdateAsync<POContempoOutput, object>(poctmpoutputcurrent, u => u.Doc(poctmpoutputcurrent).Index(_currentIndex).DocAsUpsert());
                                    }
                                    else
                                    {
                                        _logger.LogInformation($"WriteFileAsync - Skipping output for PO - {poctmpoutputcurrent.PONumber}; no SKUs were found.");
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(poctmpoutputcurrent.PONumber))
                                    {
                                        var doc = await _client.GetAsync<TDoc>(new DocumentPath<TDoc>(poctmpoutputcurrent.PONumber), g => g.Index(_previousIndex));
                                        if (doc.Source != null)
                                        {
                                            var poctmpoutputprev = doc.Source as POContempoOutput;
                                            var compareddata = _poCtnmpoOutput.CompareContempoData(poctmpoutputcurrent, poctmpoutputprev, _outputSettings.Value.MembersToInclude);
                                            if (compareddata != null && compareddata.POSkus != null && compareddata.POSkus.Count > 0)
                                            {
                                                _logger.LogInformation($"WriteFileAsync - Outputting changes to {poctmpoutputcurrent.PONumber} to file.");
                                                WriteFile<POContempoOutput>(compareddata, path);
                                                atLeastOneRecord = true;
                                            }
                                            else
                                                _logger.LogInformation($"WriteFileAsync - Skipping output for {poctmpoutputcurrent.PONumber}; no changes were found.");
                                        }
                                        //PO data does not exist in previous, hence need to be added
                                        else
                                        {
                                            var podatanew = document as POContempoOutput;
                                            if (podatanew.POSkus != null && podatanew.POSkus.Count > 0)
                                            {
                                                _logger.LogInformation($"WriteFileAsync - Outputting {podatanew.PONumber} to file as new purchase order.");
                                                podatanew.POSkus.ForEach(x => { x.ActivityCode = ""; });
                                                WriteFile<POContempoOutput>(podatanew, path);
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
                                    var archiveresponse = await _client.UpdateAsync<POContempoOutput, object>(poctmpoutputcurrent, u => u.Doc(poctmpoutputcurrent).Index(_archiveIndex).DocAsUpsert());

                                    //Save to Previous Index from Current
                                    var previousresponse = await _client.UpdateAsync<POContempoOutput, object>(poctmpoutputcurrent, u => u.Doc(poctmpoutputcurrent).Index(_previousIndex).DocAsUpsert());

                                    //delete from current
                                    var deletedcurrent = await _client.DeleteAsync<POContempoOutput>(poctmpoutputcurrent, u => u.Index(_currentIndex));

                                    _logger.LogInformation($"WriteFileAsync - Archived Contempo Index Successfully -- {poctmpoutputcurrent.PONumber}.");
                                }
                                else
                                {
                                    _logger.LogInformation($"WriteFileAsync - No file updated. So, did not Archived Contempo Index -- {poctmpoutputcurrent.PONumber}.");
                                }

                                _logger.LogInformation($"WriteFileAsync - Successfully Processed PONumber -- {poctmpoutputcurrent.PONumber} .");
                            }

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
                //    ///reindex
                //    ReIndex<TDoc>();

                return new ApiResult<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Method Name -- WriteFileAsync: {Reason}, {InnerEx}", ex?.Message, ex?.InnerException?.Message);
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
                    string newFileName = string.Format("{0}_{1}{2}", Path.GetFileNameWithoutExtension(_outputSettings.Value.OutputFilePath), DateTime.Now.ToString(OpFileNamedateFormat), Path.GetExtension(_outputSettings.Value.OutputFilePath));
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
                                                    .Index(_previousIndex)
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
    }
}
