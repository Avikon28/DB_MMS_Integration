using System.Collections.Generic;

namespace SG.PO.APLL.DataModel.Outputmodels
{
    public class POAPLLOutput
    {
        public string ActivityCode { get; set; }
        public string ConsigneeNumber { get; set; }
        public int PONumber { get; set; }
        public string WarehouseDueDate { get; set; }
        public string EarlyShipDate { get; set; }
        public string LastShipDate { get; set; }
        public string VendorShipDate { get; set; }
        public string StoreCode { get; set; }
        public string PartialShipmentFlag { get; set; }
        public string BuyerCode { get; set; }
        public string DutyPerPiece { get; set; }
        public string VendName { get; set; }
        public string StatusCode { get; set; }
        public string SubVendorNumber { get; set; }
        public string LOB { get; set; }
        public bool ForceInclude { get; set; }
        
        public List<POSkusOutput> POSkus { get; set; }
    }
}
