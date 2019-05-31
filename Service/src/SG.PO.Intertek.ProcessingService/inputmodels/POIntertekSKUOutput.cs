using System;
using System.Collections.Generic;
using System.Text;

namespace SG.PO.Intertek.FileWriter.inputmodels
{
    public class POIntertekSKUOutput
    {
        public string PONumber { get; set; }
        public string ItemNumber { get; set; }
        public string DeliveryDate { get; set; }
        public string SKU { get; set; }
        public string BuyQuantity { get; set; }        
        public string DutyCost { get; set; }
        public string FirstCost { get; set; }
        public POIntertekProduct POProduct { get; set; }
    }
}
