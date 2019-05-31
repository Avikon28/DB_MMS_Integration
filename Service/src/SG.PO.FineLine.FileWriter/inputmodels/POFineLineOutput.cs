using SG.PO.FineLine.FileWriter.inputmodels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SG.PO.FineLine.FileWriter.Helper
{
    public class POFineLineOutput
    {
        public string ActivityCode { get; set; }
        public string Company { get; set; }
        public string PurchaseOrder { get; set; }
        public string PurchaseOrderDate { get; set; }
        public string Currency { get; set; }
        public Char ActionCode { get; set; }
        public List<POFineLineSkusOutput> POSkus { get; set; }
    }
}
