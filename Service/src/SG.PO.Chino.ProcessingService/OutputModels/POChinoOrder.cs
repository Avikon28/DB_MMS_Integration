using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace SG.PO.Chino.ProcessingService.Outputmodels
{
    public class Order
    {
        public string OrderId { get; set; }
        public string PickupStart { get; set; }
        public string DeliveryStart { get; set; }
        public string DeliveryEnd { get; set; }
        public string DestinationFacilityAliasId { get; set; }
        public string PODate { get; set; }        
        public CustomFields CustomFieldList { get; set; }
        [XmlElement("LineItem")]
        public List<LineItem> LineItems { get; set; }
    }
}
