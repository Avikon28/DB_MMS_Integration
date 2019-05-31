namespace SG.PO.APLL.DataModel.Outputmodels
{
    public class POSkusOutput
    {
        public string PONumber { get; set; }
        public string ActivityCode { get; set; }

        public string ItemNumber { get; set; }

        public string DeliveryDate { get; set; }
        public string POCreationDate { get; set; }
        public string UnitCost { get; set; }
        public string RetailPrice { get; set; }
        public string ItemQty { get; set; }
        public string ItemPackageUnit { get; set; }

        public string ItemTotalQuantity { get; set; }
        public string ReceiptQuantity { get; set; }
        public string DutyCost { get; set; }

        public string StatusCode { get; set; }
        public string ModifiedDate { get; set; }

        public string CreateDate { get; set; }

        public string ApprovalLetter { get; set; }

        public string SamplesRequired { get; set; }

        public string EstimatedLandedCost { get; set; }

        public string MasterPackCubicFeet { get; set; }

        public string DutyPctOfFOB { get; set; }
        public POProductOutput POProduct { get; set; }
    }
}
