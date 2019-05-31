using System;
using System.Collections.Generic;
using System.Text;

namespace SG.PO.Intertek.DataModels.Outputmodels
{
    public class POProductOutput
    {
        public string SKU { get; set; }
        public string SkuDesc { get; set; }
        public string MasterPackQuantity { get; set; }
        public string SubClass { get; set; }       
        
        public string ClassCode { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        public string ClassName { get; set; }
        public string ClassShortDesc { get; set; }
        public string SubclassDescription { get; set; }
        public string HTSCode { get; set; }
        public string Country { get; set; }
        public string PrepackChildQuantity { get; set; }
        public string PrepackTotalQuantity { get; set; }
        public POProductFlagsOutput ProductFlagsOutput { get; set; }
    }
}
