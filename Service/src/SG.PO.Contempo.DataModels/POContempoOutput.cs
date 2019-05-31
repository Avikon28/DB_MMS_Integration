using System;
using System.Collections.Generic;

namespace SG.PO.Contempo.DataModels.Outputmodels
{
    public class POContempoOutput
    {
        public string PONumber { get; set; } //PONumber
        public string SubVendor { get; set; }  //SubVendor       
        public string CurrencyCode { get; set; }  //CurrencyCode
        public string StatusCode { get; set; }
        public bool ForceInclude { get; set; }
        public List<POContempoSkuOutput> POSkus { get; set; }
    }
}
