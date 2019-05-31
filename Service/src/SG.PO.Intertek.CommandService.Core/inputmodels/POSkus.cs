namespace SG.PO.Intertek.CommandService.Core.inputmodels
{
    public class POSkus
    {
        public string PONumber { get; set; }
        public string SKU { get; set; }
        public string DeliveryDate { get; set; }
        public string FirstCost { get; set; }
        public string RetailPrice { get; set; }
        public string BuyQuantity { get; set; }
        public string ReceiptQuantity { get; set; }
        public string DutyCost { get; set; }
        public string FreightCost { get; set; }
        public string MiscCost { get; set; }
        public string StatusCode { get; set; }
        public POProduct POProduct { get; set; }
    }
}
