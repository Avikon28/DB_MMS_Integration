using System;
using System.Collections.Generic;
using System.Text;

namespace SG.PO.FineLine.FileWriter.inputmodels
{
    public class POFineLineSkusOutput
    {
        public string PurchaseOrder { get; set; }
        public string SKUNumber { get; set; }
        public string TicketRetail { get; set; }
        public string OrderQuantity { get; set; }        
        public POFineLineProductOutput POProduct { get; set; }
    }
}
