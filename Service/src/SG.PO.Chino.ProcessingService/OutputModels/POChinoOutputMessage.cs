using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace SG.PO.Chino.ProcessingService.Outputmodels
{
    public class Message
    {
        [XmlElement("Order")]
        public List<Order> Orders { get; set; }
    }
}
