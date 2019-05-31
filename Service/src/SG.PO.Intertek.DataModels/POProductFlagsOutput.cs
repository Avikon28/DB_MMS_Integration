using System;
using System.Collections.Generic;
using System.Text;

namespace SG.PO.Intertek.DataModels.Outputmodels
{
    public class POProductFlagsOutput
    {
        public string Sku { get; set; }
        public string IsChildClothCostume { get; set; }
        public string NonPaintTestingRequired { get; set; }
        public string CPSIATestingRequired { get; set; }
        public string IntlSafetyTransitTestRequired { get; set; }
        public string IsGlassDishAdultJewelry { get; set; }
        public string RandomInspectionRequired { get; set; }
    }
}
