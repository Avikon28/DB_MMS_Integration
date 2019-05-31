using System;
using System.Collections.Generic;
using System.Text;

namespace SG.PO.Contempo.CommandService.Core.outputmodels
{
    public class POContempoProductOutput
    {
        public string ID_ITM_SKU { get; set; } //SKU
        public string ID_VENDOR { get; set; }  //AP_Vendor
        public string ID_VENSUB { get; set; }  //SubVendor
        public string ID_CLASS { get; set; }  //class
        public string TX_CLASS { get; set; }  //Class level description
        public string ID_SUBCLASS { get; set; }  //subclass
        public string TX_SUBCLASS { get; set; }  //Subclass level description
        public string TX_ITM_MDSE_SZ { get; set; }  //Size
        public string IN_ACTION { get; set; }    //TBD  - C = CANCELED AT PO LEVEL X = CANCELLED AT LINE ITEM LEVEL
        public string ID_ISO_COUNTRY { get; set; }  //CountryOfOrigin
        public string TX_ITM_SH { get; set; }  //SkuDesc
        public string ID_VTM_STYL_NUM { get; set; }  //VendorSKUCode
        public string CD_ITM_MDSE_TKT { get; set; }  //LabelType
        public string TX_ITM_MDSE_TKT { get; set; }  //GLU
    }
}
