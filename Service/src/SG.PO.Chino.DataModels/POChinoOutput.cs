using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SG.PO.Chino.DataModels.Outputmodels
{
    public class POChinoOutput
    {
        public string OrderId { get; set; }
        public string PickupStart { get; set; }
        public string DeliveryStart { get; set; }
        public string DeliveryEnd { get; set; }
        [XmlIgnore]
        public string StatusCode { get; set; }
        public bool ForceInclude { get; set; }
        public List<POSkusOutput> POSkus { get; set; }
    }
}
