using System.Collections.Generic;

namespace SG.PO.Intertek.DataModels.Outputmodels
{
    public class POIntertekOutput
    {
        public string PONumber { get; set; }
        public string ShipDate { get; set; }
        public string DeliveryDate { get; set; }        
        public string CancelDate { get; set; }
        public string LocationNumber { get; set; }
        public string EmployeeId { get; set; }  
        public string VendorName { get; set; }
        public string SubVendorNumber { get; set; }
        public string StatusCode { get; set; }
        public string LOB { get; set; }
        public string DistributorId { get; set; }
        public string CurrencyCode { get; set; }
        public bool ForceInclude { get; set; }
        public List<POIntertekSKUOutput> POSkus { get; set; }
    }    
}
