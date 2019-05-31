namespace SG.PO.Intertek.CommandService.Core.outputmodels
{
    public class POIntertekProduct
    {
        public string Sku { get; set; }
        public string MasterSku { get; set; }
        public string SkuDescShrt { get; set; }
        public string MasterPackQuantity { get; set; }
        public string SubClass { get; set; }        
        public string Consignment { get; set; }        
        public string VendorName { get; set; }
        public string SubVendorNumber { get; set; } 
        public string ClassCode { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        public string ClassName { get; set; }
        public string ClassShortDesc { get; set; }        
        public string SubclassDescription { get; set; }
        public string HTSCode { get; set; }
    }
}
