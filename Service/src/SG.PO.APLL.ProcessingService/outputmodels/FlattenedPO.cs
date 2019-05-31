namespace SG.PO.APLL.ProcessingService
{
    internal class FlattenedPOAPL
    {
        public string ActivityCode { get; set; }
        public string ConsigneeNumber { get; set; }
        public string PONumber { get; set; }
        public string WarehouseDueDate { get; set; }

        public string Language { get; set; }
        public string ItemNumber { get; set; }
        public string EarlyShipDate { get; set; }
        public string LastShipDate { get; set; }
        public string CountryofOrigin { get; set; }
        public string LoadPort { get; set; }
        public string DischargePort { get; set; }
        public string DestinationCode { get; set; }
        public string ItemQty { get; set; }
        public string ItemPackageUnit { get; set; }


        public string StoreCode { get; set; }
        public string PartialShipmentFlag { get; set; }


        public string ItemDescription { get; set; }
        public string ItemTotalQuantity { get; set; }
        public string CasePackQty { get; set; }
        public string CasePackageUnit { get; set; }
        public string ClassCode { get; set; }


        public string TariffCode { get; set; }
        public string VendorName { get; set; }
        public string LCNumber { get; set; }


        public string VendorNumber { get; set; }
        public string DestinationName { get; set; }
        public string ApprovalLetterFlag { get; set; }
        public string TransportationType { get; set; }
        public string ADDate { get; set; }
        public string UPC { get; set; }
        public string BuyerCode { get; set; }
        public string DistributorCode { get; set; }
        public string PreDistributionQty { get; set; }
        public string POCreationDate { get; set; }
        public string SamplesRequiredFlag { get; set; }
        public string CubicFeetOfMasterPK { get; set; }
        public string LineOfBusiness { get; set; }
        public string DutyPercentange { get; set; }


        public string DutyPerPiece { get; set; }
        public string EstimatedLandedCost { get; set; }
        public string UnitCost { get; set; }
        public string VendorShipDate { get; set; }
        public string DeparmentName { get; set; }
        public string ClassName { get; set; }
        public string DepartmentCode { get; set; }
        public string TopItemCode { get; set; }
        public string NewItem { get; set; }
        public string LabTest { get; set; }
        public string PickPackLocation { get; set; }

        public string OrderType { get; set; }

        public string LOB { get; set; }

        public string ApprovalLetter { get; set; }

        public string SamplesRequired { get; set; }


        public string MasterPackCubicFeet { get; set; }

        public string DutyPctOfFOB { get; set; }
    }
}