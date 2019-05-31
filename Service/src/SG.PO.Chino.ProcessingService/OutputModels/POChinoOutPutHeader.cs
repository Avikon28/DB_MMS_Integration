using System;
using System.Collections.Generic;
using System.Text;

namespace SG.PO.Chino.ProcessingService.Outputmodels
{
    public class Header
    {        
        public string Source { get; set; }
        public string Action_Type { get; set; }
        public string Batch_Id { get; set; }
        public string Message_Type { get; set; }
        public string Company_Id { get; set; }
        public string Version { get; set; }
        public string Internal_Reference_ID { get; set; }        
    }
}
