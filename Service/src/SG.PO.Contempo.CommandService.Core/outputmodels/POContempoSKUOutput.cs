using System;
using System.Collections.Generic;
using System.Text;

namespace SG.PO.Contempo.CommandService.Core.outputmodels
{
    public class POContempoSKUOutput
    {
        public string ID_PO { get; set; } //PONumber
        public string ID_ITM_SKU { get; set; } //SKU
        public int QU_ITM_ORD { get; set; }  //BuyQuanity
        public decimal AM_ITM_TRET_PRC { get; set; }  //RetailPrice
        public POContempoProductOutput POProduct { get; set; }
    }    
}
