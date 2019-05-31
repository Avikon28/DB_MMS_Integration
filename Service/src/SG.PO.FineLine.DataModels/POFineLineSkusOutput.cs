using System;

namespace SG.PO.FineLine.DataModels.Outputmodels
{
    public class POFineLineSkuOutput
    {
        public string SKUNumber { get; set; }
        public DateTime? PurchaseOrderDate { get; set; }
        public DateTime? PurchaseOrderReviseDate { get; set; }
        public int? OrderQuantity { get; set; }
        public POFineLineProductOutput POProduct { get; set; }
        public string ActivityCode { get; set; }
        public string StatusCode { get; set; }
    }
}