using System.Xml.Serialization;

namespace SG.PO.Chino.ProcessingService.Outputmodels
{
    [XmlRoot(ElementName = "tXML")]
    public class POChinoOutput
    {
        [XmlIgnore]
        public string PONumber { get; set; }        
        public Header Header { get; set; }
        public Message Message { get; set; }
    }
}
