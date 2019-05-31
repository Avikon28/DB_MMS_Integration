using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using SG.PO.Chino.DataModels.Outputmodels;
using SG.PO.Chino.ProcessingService.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using SG.Shared.Api;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace SG.PO.Chino.ProcessingService
{
    public class ElasticWriter
    {
        private readonly ElasticClient _client;
        private readonly IOptions<OutputSettings> _outputSettings;
        private readonly POChinoUtilities _poChinoutilities;
        private readonly string _currentIndex;
        private readonly string _previousIndex;
        private readonly string _archiveIndex;
        private readonly ILogger _logger;

        public ElasticWriter(ElasticClient client, IOptions<OutputSettings> outputSettings, POChinoUtilities poChinoutilities,
            ILogger<ElasticWriter> logger, IConfiguration config)
        {
            _client = client;
            _outputSettings = outputSettings;
            _poChinoutilities = poChinoutilities;
            _logger = logger;
            _currentIndex = config.GetValue<string>("CurrentIndex");
            _previousIndex = config.GetValue<string>("PreviousIndex");
            _archiveIndex = config.GetValue<string>("ArchiveIndex");
        }

        public async Task<ApiResult> WriteFileAsync<TDoc>(string index, int PageSize = 10000) where TDoc : class
        {
            int CurrentCount = 0;

            //reads for that day from elastic
            //reads in chunks of 100, can be changed             
            var docCount = _client.Count<TDoc>(c => c.Index(_currentIndex));
            _logger.LogInformation($"WriteFileAsync - Inspecting {docCount.Count} documents for changes...");

            var searchResponse = _client.Search<TDoc>(s => s
                                .Size(100)
                                .Scroll("1m")
                                .Index(_currentIndex));
            List<POChinoOutput> finalChinoOutput = new List<POChinoOutput>();
            var outputType = _outputSettings.Value.Type.ToLower();
            while (searchResponse.Documents.Any())
            {
                switch (outputType)
                {
                    case "xml":
                        foreach (var document in searchResponse.Documents)
                        {
                            var pochinooutputcurrent = document as POChinoOutput;
                            if (pochinooutputcurrent.ForceInclude)
                            {
                                finalChinoOutput.Add(pochinooutputcurrent);
                            }
                            else
                            {
                                //Only send the PO if it's 'Open'
                                if (pochinooutputcurrent.StatusCode == "OP")
                                {
                                    //Read from previous index for the document. If sent before, we don't send it again
                                    var doc = await _client.GetAsync<TDoc>(new DocumentPath<TDoc>(pochinooutputcurrent.OrderId), g => g.Index(_previousIndex));
                                    if (doc.Source == null)
                                    {
                                        var podatanew = document as POChinoOutput;
                                        finalChinoOutput.Add(podatanew);
                                    }
                                }
                            }
                            //save it to archive index before delete from current
                            var archiveresponse = await _client.UpdateAsync<POChinoOutput, object>(pochinooutputcurrent, u => u.Doc(pochinooutputcurrent).Index(_archiveIndex).DocAsUpsert());

                            //Save to Previous Index from Current
                            var previousresponse = await _client.UpdateAsync<POChinoOutput, object>(pochinooutputcurrent, u => u.Doc(pochinooutputcurrent).Index(_previousIndex).DocAsUpsert());

                            //delete from current
                            var deletedcurrent = await _client.DeleteAsync<POChinoOutput>(pochinooutputcurrent, u => u.Index(_currentIndex));

                            _logger.LogInformation($"WriteFileAsync - Successfully Processed PONumber -- {pochinooutputcurrent.OrderId} .");
                        }
                        _poChinoutilities.WriteChinoFile(finalChinoOutput);

                        break;
                    default:
                        _logger.LogError($"WriteFileAsync - Invalid output type {outputType} specified.");
                        return new ApiResult<string>();
                }
                CurrentCount++;

                searchResponse = _client.Scroll<TDoc>("1m", searchResponse.ScrollId);
            }
            _client.ClearScroll(c => c.ScrollId(searchResponse.ScrollId));

            //ReIndex<TDoc>();

            return new ApiResult<string>();
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
