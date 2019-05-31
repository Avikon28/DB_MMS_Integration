namespace SG.PO.FineLine.ProcessingService
{
    internal class FlattenedPOFineLine
    {
        public string PurchaseOrder { get; set; }
        public string SKUNumber { get; set; }
        public string PurchaseOrderDate { get; set; }
        public string PurchaseOrderReviseDate { get; set; }
        public string VendorNumber { get; set; }
        public string SubVendorNumber { get; set; }
        public string SKUDescription { get; set; }
        public string VendorStyleNumber { get; set; }
        public string TicketType { get; set; }
        public string TicketDescription { get; set; }
        public string TicketRetail { get; set; }
        public string ClassID { get; set; }
        public string ClassDescription { get; set; }
        public string SubClassID { get; set; }
        public string SubClassDescription { get; set; }
        public string OrderQuantity { get; set; }
        public string Currency { get; set; }
        public string Size { get; set; }
        public string ActivityCode { get; set; }
        public string ISOCountryCode { get; set; }        
    }
}