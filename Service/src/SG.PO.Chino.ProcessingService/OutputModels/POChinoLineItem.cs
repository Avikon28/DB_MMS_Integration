namespace SG.PO.Chino.ProcessingService.Outputmodels
{
    public class LineItem
    {
        public string LineItemId { get; set; }
        public string ItemName { get; set; }        
        public POChinoLineItemQuantity Quantity { get; set; }
    }
}
