using System;
using System.Collections.Generic;
using System.Text;

namespace SG.PO.Contempo.CommandService.Core.inputmodels
{
    public class POProduct
    {
        private string p_color;
        private string p_size;
        private string p_royaltyvendorcode;

        public string Sku { get; set; }
        public string MasterSku { get; set; }

        public string SkuDesc { get; set; }
        public string SkuDescShrt { get; set; }
        public string StyleStatus { get; set; }
        public string SubClass { get; set; }
        public string LabelType { get; set; }
        public decimal? InitialRetailBase { get; set; }
        public DateTime? IntitialRetailBaseDate { get; set; }
        public decimal? InitialRetailCanada { get; set; }
        public DateTime? InitialRetailCanadaDate { get; set; }
        public string Color
        {
            get
            {
                if (string.IsNullOrEmpty(this.p_color))
                {
                    return "NA";
                }
                else
                {
                    return this.p_color;
                }

            }
            set
            {
                this.p_color = value;
            }
        }
        public string Size
        {
            get
            {
                if (string.IsNullOrEmpty(this.p_size))
                {
                    return "NA";
                }
                else
                {
                    return this.p_size;
                }

            }
            set
            {
                this.p_size = value;
            }
        }

        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Depth { get; set; }
        public string Material { get; set; }
        public string HTSCode { get; set; }
        public string CoordinateGroup { get; set; }
        public string Consignment { get; set; }
        public string AllowPriceOverrideInPOS { get; set; }

        public string RoyaltyVendorCode
        {
            get
            {
                if (string.IsNullOrEmpty(this.p_royaltyvendorcode))
                {
                    return "NA";
                }
                else
                {
                    return this.p_royaltyvendorcode;
                }

            }
            set
            {
                this.p_royaltyvendorcode = value;
            }
        }

        public string RoyaltyVendorName { get; set; }
        public string Stockable { get; set; }
        public DateTime? StatChangeDate { get; set; }
        public DateTime? IntroDate { get; set; }

        public string CustomsDescription { get; set; }
        public string DevelopedInHouse { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ShelfPackQuantity { get; set; }
        public string DropShip { get; set; }
        public DateTime? ForceUpdate { get; set; }

        public string TaxCategory { get; set; }
        public string VendorName { get; set; }
        public string CountryOfOrigin { get; set; }
        public string VendorSkuCode { get; set; }
        public string Brand { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Class { get; set; }

        public string ClassDescription { get; set; }

        public string ClassShortDesc { get; set; }

        public string SizeChartName { get; set; }

        public string APVendor { get; set; }

        public string SubVendor { get; set; }
        public string Vendor_SKU_Code { get; set; }
        public string SubclassDescription { get; set; }
    }
}
