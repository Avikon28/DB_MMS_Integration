using System;
using System.Collections.Generic;
using System.Text;

namespace SG.PO.Contempo.DataModels.Outputmodels
{
    public class POContempoProductOutput
    {
        public string APVendor { get; set; }  //AP_Vendor
        public string Class { get; set; }  //class
        public string ClassLevelDesc { get; set; }  //Class level description
        public string SubClass { get; set; }  //subclass
        public string SubClassLevelDesc { get; set; }  //Subclass level description
        public string Size { get; set; }  //Size
        public string CountryOfOrigin { get; set; }  //CountryOfOrigin
        public string SkuDesc { get; set; }  //SkuDesc
        public string VendorSKUCode { get; set; }  //VendorSKUCode
        public string LabelType { get; set; }  //LabelType
        public string LabelDescription { get; set; }
        public string RetailPrice { get; set; }  //RetailPrice
    }
}
