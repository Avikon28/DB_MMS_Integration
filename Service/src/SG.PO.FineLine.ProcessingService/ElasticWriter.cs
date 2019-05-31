using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using SG.PO.FineLine.DataModels;
using SG.PO.FineLine.ProcessingService.Helper;
using SG.PO.FineLine.ProcessingService.Services;
using SG.Shared.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using SG.PO.FineLine.DataModels.Outputmodels;

namespace SG.PO.FineLine.ProcessingService
{
    public class ElasticWriter
    {
        private readonly ElasticClient _client;
        private readonly POFineLineUtilities _pofinelineutilities;
        private readonly IOptions<OutputSettings> _outputSettings;
        private readonly ILogger _logger;
        private readonly string _currentIndex;
        private readonly string _previouIndex;
        private readonly string _archiveIndex;
        private const string _embeddedFineLineConfigFile = "SG.PO.FineLine.ProcessingService.Helper.POFineLine.xml";
        public ElasticWriter(ElasticClient client, IOptions<OutputSettings> outputSettings, ILogger<ElasticWriter> logger,
            POFineLineUtilities pofinelineutilities, IConfiguration configuration)
        {
            _client = client;
            _outputSettings = outputSettings;
            _logger = logger;
            _pofinelineutilities = pofinelineutilities;
            _currentIndex = configuration.GetValue<string>("CurrentIndex");
            _previouIndex = configuration.GetValue<string>("PreviousIndex");
            _archiveIndex = configuration.GetValue<string>("ArchiveIndex");
        }

        public void WriteFile<TDoc>(TDoc document, string opFilePath) where TDoc : class
        {

            //read the config file for start and end positions            
            //var configfile = XDocument.Load(_outputSettings.Value.ConfigFile);
            var assembly = Assembly.GetExecutingAssembly();
            //var stream = assembly.GetManifestResourceStream(_embeddedFineLineConfigFile); // Embedded resource config file
            var stream = assembly.GetManifestResourceStream(string.Format("{0}.Helper.POFineLine.xml", typeof(ElasticWriter).Namespace)); // Embedded resource config xml file
            var configfile = XDocument.Load(stream, LoadOptions.None);

            List<POFineLineOutput> polist = new List<POFineLineOutput>();
            var pofl = document as POFineLineOutput;
            polist.Add(pofl);

            //flatten the object for output into a text file
            var flattenned = polist.SelectMany(po => po.POSkus
                      .Select(posku => new FlattenedPOFineLine
                      {
                          ActivityCode = posku.ActivityCode,
                          PurchaseOrder = po.PurchaseOrder,
                          SKUNumber = posku.SKUNumber,
                          PurchaseOrderDate = posku.PurchaseOrderDate != null ? posku.PurchaseOrderDate.Value.ToString("MM/dd/yyyy") : "",
                          PurchaseOrderReviseDate = posku.ActivityCode == "" ? (posku.PurchaseOrderReviseDate != null ? posku.PurchaseOrderReviseDate.Value.ToString("MM/dd/yyyy") : "") : "",
                          VendorNumber = posku.POProduct?.VendorNumber,
                          SubVendorNumber = posku.POProduct?.SubVendorNumber,
                          SKUDescription = posku.ActivityCode == "" ? posku.POProduct?.SKUDescription : "",
                          VendorStyleNumber = posku.ActivityCode == "C" ? "" : posku.POProduct?.VendorStyleNumber,
                          TicketType = posku.ActivityCode == "" ? posku.POProduct?.TicketType : "",
                          TicketDescription = posku.ActivityCode == "" ? posku.POProduct?.TicketDescription : "",
                          TicketRetail = posku.ActivityCode == "" ? posku.POProduct?.TicketRetail.ToString() : "",
                          ClassID = posku.ActivityCode == "" ? posku.POProduct?.ClassID : "",
                          ClassDescription = posku.ActivityCode == "" ? posku.POProduct?.ClassDescription : "",
                          SubClassID = posku.ActivityCode == "" ? posku.POProduct?.SubClassID : "",
                          SubClassDescription = posku.ActivityCode == "" ? posku.POProduct?.SubClassDescription : "",
                          OrderQuantity = posku.ActivityCode == "" ? posku.OrderQuantity.ToString() : "",
                          Currency = posku.ActivityCode == "" ? po.Currency : "",
                          Size = posku.ActivityCode == "" ? posku.POProduct?.Size : "",
                          ISOCountryCode = posku.ActivityCode == "" ? posku.POProduct?.ISOCountryCode : ""
                      }
                    ));

            //write data into txt output
            _pofinelineutilities.WriteTxtFormat(flattenned.ToList(), configfile.Descendants("WriteFixedWidth").FirstOrDefault(), opFilePath);
        }

        public async Task<ApiResult> WriteFileAsync<TDoc>() where TDoc : class
        {
            try
            {
                //Log message if directory not found and return
                if (!Directory.Exists(Path.GetDirectoryName(_outputSettings.Value.OutputFilePath)))
                {
                    _logger.LogError("WriteFileAsync - No Directory found with given path- POFineLine: {Reason}", _outputSettings.Value.OutputFilePath);
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
                                var pofloutputcurrent = document as POFineLineOutput;

                                //If we're supposed to include this no matter what, just do so
                                if (pofloutputcurrent.ForceInclude)
                                {
                                    if (pofloutputcurrent.POSkus != null && pofloutputcurrent.POSkus.Count > 0)
                                    {
                                        pofloutputcurrent.POSkus.ForEach(posku =>
                                        {
                                            posku.ActivityCode = "";
                                        });
                                        WriteFile<POFineLineOutput>(pofloutputcurrent, path);
                                        atLeastOneRecord = true;
                                        pofloutputcurrent.ForceInclude = false;
                                        var response = await _client.UpdateAsync<POFineLineOutput, object>(pofloutputcurrent, u => u.Doc(pofloutputcurrent).Index(_currentIndex).DocAsUpsert());
                                    }
                                    else
                                    {
                                        _logger.LogInformation($"WriteFileAsync - Skipping output for PO - {pofloutputcurrent.PurchaseOrder}; no SKUs were found.");
                                    }
                                }
                                else
                                {
                                    //Read from previous index for the document and compare with the current. 
                                    var doc = await _client.GetAsync<TDoc>(new DocumentPath<TDoc>(pofloutputcurrent.PurchaseOrder), g => g.Index(_previouIndex));
                                    if (doc.Source == null)
                                    {
                                        //PO data does not exist in previous, so send
                                        var podatanew = document as POFineLineOutput;
                                        if (podatanew.POSkus != null && podatanew.POSkus.Count > 0)
                                        {
                                            _logger.LogInformation($"WriteFileAsync - Outputting {pofloutputcurrent.PurchaseOrder} to file as new purchase order.");
                                            podatanew.POSkus.ForEach(x => { x.ActivityCode = ""; });
                                            WriteFile<POFineLineOutput>(podatanew, path);
                                            atLeastOneRecord = true;
                                        }
                                        else
                                        {
                                            _logger.LogInformation($"WriteFileAsync - Skipping output for PO - {podatanew.PurchaseOrder}; no SKUs were found.");
                                        }
                                    }
                                    else
                                    {
                                        //See if there is any difference between PO and or the POskus; if so, send the current
                                        var pofloutputprev = doc.Source as POFineLineOutput;
                                        var compareddata = _pofinelineutilities.CompareFineLineData(pofloutputcurrent, pofloutputprev, _outputSettings.Value.MembersToInclude);
                                        if (compareddata != null && compareddata.POSkus != null && compareddata.POSkus.Count > 0)
                                        {
                                            _logger.LogInformation($"WriteFileAsync - Outputting changes to {pofloutputcurrent.PurchaseOrder} to file.");
                                            WriteFile<POFineLineOutput>(compareddata, path);
                                            atLeastOneRecord = true;
                                        }
                                        else
                                            _logger.LogInformation($"WriteFileAsync - Skipping output for {pofloutputcurrent.PurchaseOrder}; no changes were found.");
                                    }
                                }
                                if (atLeastOneRecord)
                                {
                                    //save it to archive index before delete from current
                                    var archiveresponse = await _client.UpdateAsync<POFineLineOutput, object>(pofloutputcurrent, u => u.Doc(pofloutputcurrent).Index(_archiveIndex).DocAsUpsert());

                                    //Save to Previous Index from Current
                                    var previousresponse = await _client.UpdateAsync<POFineLineOutput, object>(pofloutputcurrent, u => u.Doc(pofloutputcurrent).Index(_previouIndex).DocAsUpsert());

                                    //delete from current
                                    var deletedcurrent = await _client.DeleteAsync<POFineLineOutput>(pofloutputcurrent, u => u.Index(_currentIndex));

                                    _logger.LogInformation($"WriteFileAsync - Archived Fineline Index Successfully -- {pofloutputcurrent.PurchaseOrder}.");
                                }
                                else
                                {
                                    _logger.LogInformation($"WriteFileAsync - No file updated. So, did not Archived Fineline Index -- {pofloutputcurrent.PurchaseOrder}.");
                                }

                                _logger.LogInformation($"WriteFileAsync - Successfully Processed PONumber -- {pofloutputcurrent.PurchaseOrder} .");
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
                //else {
                //    ReIndex<TDoc>();
                //}

                return new ApiResult<string>();
            }
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
    }
}
