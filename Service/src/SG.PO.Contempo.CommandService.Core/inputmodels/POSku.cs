using System;
using System.Collections.Generic;
using System.Text;

namespace SG.PO.Contempo.CommandService.Core.inputmodels
{
    public class POSku
    {
        public int PONumber { get; set; }
        public string SKU { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public decimal? FirstCost { get; set; }
        public decimal? RetailPrice { get; set; }
        public int? BuyQuantity { get; set; }
        public int? ReceiptQuantity { get; set; }
        public decimal? DutyCost { get; set; }
        public decimal? FreightCost { get; set; }
        public decimal? MiscCost { get; set; }
        public string StatusCode { get; set; }
        public POProduct POProduct { get; set; }
    }
}
