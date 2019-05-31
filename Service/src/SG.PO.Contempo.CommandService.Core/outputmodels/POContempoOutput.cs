using System;
using System.Collections.Generic;
using System.Text;

namespace SG.PO.Contempo.CommandService.Core.outputmodels
{
    public class POContempoOutput
    {
        public string ID_PO { get; set; } //PONumber
        public DateTime DA_PO_CRE { get; set; } //CreateDate
        public DateTime DA_PO_MOD { get; set; } //TBD
        public string ID_VENSUB { get; set; }  //SubVendor       
        public string ID_ISO_CRNCY_CS { get; set; }  //CurrencyCode
        public string ActivityCode { get; set; }
        public List<POContempoSKUOutput> POSkus { get; set; }
    }    
}
