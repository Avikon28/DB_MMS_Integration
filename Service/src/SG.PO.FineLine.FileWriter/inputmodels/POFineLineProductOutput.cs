using System;
using System.Collections.Generic;
using System.Text;

namespace SG.PO.FineLine.FileWriter.inputmodels
{
    public class POFineLineProductOutput
    {
        public string SKUNumber { get; set; }
        public string SKUDescription { get; set; }
        public string SubClassID { get; set; }
        public string TicketType { get; set; }
        public string Size { get; set; }
        public DateTime? PurchaseOrderDate { get; set; }
        public string ISOCountryCode { get; set; }
        public string ClassID { get; set; }
        public string ClassDescription { get; set; }
        public string VendorStyleNumber { get; set; }
        public string SubClassDescription { get; set; }
        public string SubVendorNumber { get; set; }
    }
}
