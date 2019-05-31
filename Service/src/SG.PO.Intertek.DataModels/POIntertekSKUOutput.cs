using System;

namespace SG.PO.Intertek.DataModels.Outputmodels
{
    public class POIntertekSKUOutput
    {
        public string ActivityCode { get; set; }
        public string PONumber { get; set; }
        public string CreateDate { get; set; }
        public string SKU { get; set; }
        public string BuyQuantity { get; set; }       
        public string DutyCost { get; set; }
        public string FirstCost { get; set; }
        public string StatusCode { get; set; }
        public string MasterPackCubicFeet { get; set; }
        public string DutyPctOfFOB { get; set; }
        public string EstimatedLandedCost { get; set; }
        public string PrepackId { get; set; }
        public string ApprovalLetter { get; set; }
        public POProductOutput POProduct { get; set; }
    }
}
