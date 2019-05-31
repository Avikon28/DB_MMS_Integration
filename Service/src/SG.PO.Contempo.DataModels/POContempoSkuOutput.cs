using System;
using System.Collections.Generic;
using System.Text;

namespace SG.PO.Contempo.DataModels.Outputmodels
{
    public class POContempoSkuOutput
    {
        public string SKU { get; set; } //SKU
        public int? BuyQuanity { get; set; }  //BuyQuanity
        //public decimal RetailPrice { get; set; }  //RetailPrice
        public POContempoProductOutput POProduct { get; set; }
        public string ActivityCode { get; set; }   //TBD  - C = CANCELED AT PO LEVEL X = CANCELLED AT LINE ITEM LEVEL
        public string StatusCode { get; set; }
        public DateTime? CreateDate { get; set; } //CreateDate
        public DateTime? ModifiedDate { get; set; } //TBD
    }
}
