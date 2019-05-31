using Nest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.Options;
using SG.PO.FineLine.FileWriter.Services;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;
using System.IO;
using Newtonsoft.Json.Linq;
using SG.PO.FineLine.FileWriter.Helper;

namespace SG.PO.FineLine.FileWriter
{
    public class ElasticWriter
    {
        private readonly ElasticClient _client;
        private readonly ProduceOutput _produceOutput;
        private readonly IOptions<OutputSettings> _outputSettings;
        public ElasticWriter(ElasticClient client, ProduceOutput produceOutput, IOptions<OutputSettings> outputSettings)
        {
            _client = client;
            _produceOutput = produceOutput;
            _outputSettings = outputSettings;
        }

        public void WriteFile<TDoc>() where TDoc : class
        {

            //read the config file for start and end positions
            var configfile = XDocument.Load(_outputSettings.Value.ConfigFile);

            //read stubbed output for now//need to be  changed after the data is read from elastic


            TDoc doc;
            using (StreamReader r = new StreamReader("C:\\PO_Output\\testpo.Json"))
            {
                string sjson = r.ReadToEnd();

                doc = JObject.Parse(sjson).ToObject<TDoc>();
            }

            List<POFineLineOutput> polist = new List<POFineLineOutput>();
            var pofl = doc as POFineLineOutput;
            polist.Add(pofl);

            //flatten the object for output into a text file
            var flattenned = polist.SelectMany(po => po.POSkus
                      .Select(posku => new FlattenedPOFineLine
                      {
                            
                          ActivityCode = po.ActivityCode,
                          Company = po.Company,
                          PurchaseOrder = (po.PurchaseOrder == null ? "" : po.PurchaseOrder),
                          SKUNumber = posku.SKUNumber,
                          PurchaseOrderDate=po.PurchaseOrderDate,
                          PurchaseOrderReviseDate = po.PurchaseOrderDate,
                          VendorNumber = (posku.POProduct.VendorStyleNumber == null ? "" : posku.POProduct.VendorStyleNumber),
                          SubVendorNumber = posku.POProduct.SubVendorNumber,
                          SKUDescription = posku.POProduct.SKUDescription,
                          VendorStyleNumber = (posku.POProduct.VendorStyleNumber == null ? "" : posku.POProduct.VendorStyleNumber),
                          TicketType = posku.POProduct.TicketType,
                          TicketDescription = string.Empty,
                          TicketRetail=posku.TicketRetail,
                          ClassID=posku.POProduct.ClassID,
                          ClassDescription=(posku.POProduct.SubClassDescription==null ?"": posku.POProduct.SubClassDescription),
                          SubClassID=posku.POProduct.SubClassID,
                          SubClassDescription=(posku.POProduct.SubClassDescription==null? "" :posku.POProduct.SubClassDescription),
                          OrderQuantity =posku.OrderQuantity,
                          Currency=po.Currency,
                          Size=posku.POProduct.Size,
                          ActionCode=po.ActionCode,
                          ISOCountryCode=posku.POProduct.ISOCountryCode
                      }
                    ));

            //write data into txt output
            _produceOutput.WiteTxtFormat(flattenned.ToList(), configfile.Descendants("WriteFixedWidth").FirstOrDefault());
            }

        public void WriteFile<TDoc>(string index, int PageSize = 10000) where TDoc : class
        {
            var docCount = _client.Count<TDoc>(c => c.Index(index));

            //int Start = 0;         
            int CurrentCount = 0;

            var searchResponse = _client.Search<TDoc>(s => s
                                .Size(1000)
                                .Scroll("1m")
                                .Index(index));

            while (searchResponse.Documents.Any())
            {
                switch (_outputSettings.Value.Type)
                {
                    case "Txt":
                        foreach (var document in searchResponse.Documents)
                        {
                            //this will have to enabled after elastic read implementation

                            // WriteFile<TDoc>(document, CurrentCount);


                        }
                        break;
                }
               
                CurrentCount++;

                searchResponse = _client.Scroll<TDoc>("1m", searchResponse.ScrollId);
            }

            _client.ClearScroll(c => c.ScrollId(searchResponse.ScrollId));
         
        }
    }
}
