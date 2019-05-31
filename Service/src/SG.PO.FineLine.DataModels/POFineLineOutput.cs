using System.Collections.Generic;

namespace SG.PO.FineLine.DataModels.Outputmodels
{
    public class POFineLineOutput
    {
        public string PurchaseOrder { get; set; }
        public string Currency { get; set; }
        public string StatusCode { get; set; }
        public List<POFineLineSkuOutput> POSkus { get; set; }
        public string SubVendorNumber  { get; set; }
        public bool ForceInclude { get; set; }
    }
}
