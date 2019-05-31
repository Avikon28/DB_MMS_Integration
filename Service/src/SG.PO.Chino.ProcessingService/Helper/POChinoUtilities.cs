using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SG.PO.Chino.DataModels.Outputmodels;
using SG.PO.Chino.ProcessingService.Outputmodels;
using SG.PO.Chino.ProcessingService.Services;
using SG.Shared.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using POChinoOutput = SG.PO.Chino.DataModels.Outputmodels.POChinoOutput;

namespace SG.PO.Chino.ProcessingService
{
    public class POChinoUtilities
    {
        private readonly ILogger _logger;
        private readonly IOptions<OutputSettings> _outputSettings;
        private readonly IOptions<Header> _header;
        private readonly IOptions<POChinoLineItemQuantity> _lineItemSettings;
        private readonly IOptions<Order> _chinoOrderSettings;

        public POChinoUtilities(ILogger<ElasticWriter> logger, IOptions<OutputSettings> outputSettings,
            IOptions<Header> header, IOptions<POChinoLineItemQuantity> lineItemSettings, IOptions<Order> chinoOrderSettings)
        {
            _logger = logger;
            _outputSettings = outputSettings;
            _header = header;
            _lineItemSettings = lineItemSettings;
            _chinoOrderSettings = chinoOrderSettings;
        }

        public async Task<ApiResult> WriteChinoFile(List<POChinoOutput> chinoOrders)
        {
            //Log message if directory not found and return
            if (!Directory.Exists(Path.GetDirectoryName(_outputSettings.Value.OutputFilePath)))
            {
                _logger.LogError("WriteChinoFile - No Directory found with given path- POChino: {Reason}", _outputSettings.Value.OutputFilePath);
                return new ApiResult<string>();
            }
            //write data into xml output
            var chinoHeader = BuildChinoHeader();
            var chinoMessage = BuildChinoMessage(chinoOrders);

            Outputmodels.POChinoOutput chinoFileOutput = new Outputmodels.POChinoOutput
            {
                Header = chinoHeader,
                Message = chinoMessage
            };

            if (chinoMessage.Orders != null && chinoMessage.Orders.Count > 0)
            {
                string path = GetOutputFilepath();
                if (string.IsNullOrEmpty(path))
                {
                    _logger.LogError("WriteChinoFile - Output path not specified.");
                    return new ApiResult<string>();
                }

                WriteXmlFile(chinoFileOutput, path);

                //Make sure the file was actually written to disk before finalizing
                if (!File.Exists(path))
                {
                    _logger.LogError($"WriteChinoFile - Output failed to write successfully to {path}");
                    return new ApiResult<string>();
                }

                _logger.LogInformation($"Output successfully written to {path}.");
            }
            else
            {
                _logger.LogInformation($"No output written; no new or changed data found.");
            }

            return new ApiResult<string>();
        }

        private string GetOutputFilepath()
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

        private Message BuildChinoMessage(List<POChinoOutput> chinoOrders)
        {
            List<Order> pochinoOrders = new List<Order>();
            List<LineItem> pOChinoLineItems = new List<LineItem>();
            POChinoLineItemQuantity pOChinoLineItemQuantity = new POChinoLineItemQuantity();
            CustomFields CustomFields = new CustomFields();
            CustomFields.CustomField = " ";

            chinoOrders.ForEach(x =>
            {
                var counter = 1;
                x.POSkus.ForEach(y =>
                pOChinoLineItems.Add(new LineItem
                {
                    Quantity = new POChinoLineItemQuantity
                    {
                        OrderQty = Convert.ToInt32(y.OrderQty).ToString("0.0000"),
                        QtyUOM = _lineItemSettings.Value.QtyUOM
                    },
                    LineItemId = counter++.ToString(),
                    ItemName = y.ItemName

                }));



                pochinoOrders.Add(new Order
                {
                    OrderId = x.OrderId + "-01",
                    CustomFieldList = CustomFields,
                    DeliveryEnd = !string.IsNullOrEmpty(x.DeliveryEnd) ? x.DeliveryEnd : " ",
                    DeliveryStart = !string.IsNullOrEmpty(x.DeliveryStart) ? x.DeliveryStart : " ",
                    DestinationFacilityAliasId = _chinoOrderSettings.Value.DestinationFacilityAliasId,
                    PickupStart = !string.IsNullOrEmpty(x.PickupStart) ? x.PickupStart : " ",
                    PODate = DateTime.Now.ToString("MM/dd/yyyy HH:mm"),
                    LineItems = pOChinoLineItems,
                });
                pOChinoLineItems = new List<LineItem>();

            });

            Message pOChinoOutputMessage = new Message
            {
                Orders = pochinoOrders
            };

            return pOChinoOutputMessage;
        }

        private void WriteXmlFile(Outputmodels.POChinoOutput document, string path)
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter tw = null;
            try
            {
                tw = new XmlTextWriter(path, Encoding.UTF8);
                tw.Formatting = Formatting.Indented;
                tw.Indentation = 4;
                tw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"");

                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                XmlSerializer serializer = new XmlSerializer(document.GetType());
                serializer.Serialize(tw, document, ns);
            }
            catch (Exception ex)
            {
                _logger.LogError("WriteXmlFile - Error while generating xml file", ex, ex.Message);
            }
            finally
            {
                sw.Close();
                if (tw != null)
                {
                    tw.Close();
                }
            }
        }

        private Header BuildChinoHeader()
        {
            Header header = new Header
            {
                Action_Type = _header.Value.Action_Type,
                Batch_Id = _header.Value.Batch_Id,
                Company_Id = _header.Value.Company_Id,
                Internal_Reference_ID = _header.Value.Internal_Reference_ID,
                Message_Type = _header.Value.Message_Type,
                Source = _header.Value.Source,
                Version = _header.Value.Version
            };
            return header;
        }

        //mehod to compare 2 Chino objects and return the POskulines that would be sent back
        public POChinoOutput CompareChinoData(POChinoOutput poskusOutputcurrent, POChinoOutput poskusOutputprev)
        {
            try
            {
                bool poskusupdated = false;
                List<POSkusOutput> diffSkus = new List<POSkusOutput>();
                int propertyCount = typeof(POChinoOutput).GetProperties().Length;
                CompareLogic basicComparison = new CompareLogic()
                { Config = new ComparisonConfig() { MaxDifferences = propertyCount } };
                List<Difference> diffs = basicComparison.Compare(poskusOutputcurrent, poskusOutputprev).Differences;
                List<Difference> diffposkus;
                poskusOutputcurrent.POSkus.ForEach(y =>
                {
                    //Look for the prior version sent
                    var poskutobecompared = poskusOutputprev.POSkus.Find(posku => posku.ItemName == y.ItemName);

                    if (poskutobecompared != null)
                    {
                        int propertyCountskus = typeof(POSkusOutput).GetProperties().Length;
                        CompareLogic basicComparisonposkus = new CompareLogic()
                        { Config = new ComparisonConfig() { MaxDifferences = propertyCountskus } };

                        diffposkus = basicComparison.Compare(y, poskutobecompared).Differences;
                        poskusupdated = diffposkus.Count > 0;

                        if (poskusupdated)
                            diffSkus.Add(y);

                    }
                });

                if (diffSkus.Count > 0 || diffs.Count > 0)
                {
                    POChinoOutput pOChinoOutput = new POChinoOutput
                    {
                        OrderId = poskusOutputcurrent.OrderId,
                        PickupStart = poskusOutputcurrent.PickupStart,
                        DeliveryStart = poskusOutputcurrent.DeliveryStart,
                        DeliveryEnd = poskusOutputcurrent.DeliveryStart,
                        POSkus = (diffSkus.Count > 0) ? diffSkus : poskusOutputcurrent.POSkus,
                        //PODate = poskusOutputcurrent.PODate
                    };
                    return pOChinoOutput;
                }
                return new POChinoOutput();
            }
            catch (Exception ex)
            {
                _logger.LogError("Method Name -- CompareChinoData : {Reason}", ex.Message);
                return new POChinoOutput();
            }
        }
    }
}
