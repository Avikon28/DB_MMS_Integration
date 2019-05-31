using System;
using System.Collections.Generic;
using System.Text;

namespace SG.PO.Contempo.CommandService.Core.inputmodels
{
    public class Mi9PO
    {
        public int PONumber { get; set; }
        public string Mi9ChannelCode { get; set; }
        public string SubVendorNumber { get; set; }
        public string Approved { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public DateTime? ShipDate { get; set; }
        public string LocationNumber { get; set; }
        public string Mi9POType { get; set; }
        public string Terms { get; set; }
        public string PointofOwnership { get; set; }
        public string EmployeeID { get; set; }
        public string SeasonCode { get; set; }
        public string CurrencyCode { get; set; }
        public string CalculatePOExtraCosts { get; set; }
        public string AutoCloseFlag { get; set; }
        public string StatusCode { get; set; }
        public List<POSku> POSkus { get; set; }
    }
}
