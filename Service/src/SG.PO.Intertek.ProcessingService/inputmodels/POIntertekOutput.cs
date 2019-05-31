using System.Collections.Generic;

namespace SG.PO.Intertek.FileWriter.inputmodels
{
    public class POIntertekOutput
    {
        public string ActivityCode { get; set; }
        //public string ConsigneeNumber { get; set; }
        public string PONumber { get; set; }
        public string ShipDate { get; set; }
        public string CreateDate { get; set; }
        public string CancelDate { get; set; }
        public string DeliveryDate { get; set; }
        public string LocationNumber { get; set; }
        public string EmployeeId { get; set; }
        public string CurrencyCode { get; set; }
        public string CalculatePOExtraCosts { get; set; }
        public List<POIntertekSKUOutput> POSkus { get; set; }
    }
}
