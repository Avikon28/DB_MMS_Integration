using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using SG.MMS.Product.Events;
using SG.PO.Intertek.DataModels.Outputmodels;
using SG.Shared.POProduct.Services;
using SG.Vendor.MMS.Events;
using System;
using SG.PO.Intertek.CommandService.Core.Helper;
using System.Linq;
using SG.MMS.QueryService.ODATA.Models.PO;

namespace SG.PO.Intertek.CommandService.Core.Mapper
{
    public class POProductMapper
    {
        private static IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly ElasticClient _client;
        private readonly LookupDataService _lookUpService;


        public POProductMapper(IConfiguration configuration, ILogger<POProductMapper> logger, ElasticClient client,
            LookupDataService lookUpService)
        {
            _configuration = configuration;
            _logger = logger;
            _client = client;
            _lookUpService = lookUpService;
        }


        public async void UpdatePOObject(POIntertekOutput poobject, MMSProductEvent product, POSkus posku)
        {
            try
            {
                var prodhierarchy = _lookUpService.GetProductHierarchy(product.SubClass).Result;

                //check if exists
                if (poobject.POSkus!=null && poobject.POSkus.Count>0 && poobject.POSkus.Exists(y => y.SKU == posku.SKU))
                {
                    var poskutobeupdated = poobject.POSkus.Find(y => y.SKU == posku.SKU);

                    poskutobeupdated.POProduct.ClassCode = prodhierarchy.Find(y => y.SubClass == product.SubClass)?.Class;
                    poskutobeupdated.POProduct.DepartmentName = prodhierarchy.Find(y => y.SubClass == product.SubClass)?.Description;
                    poskutobeupdated.POProduct.ClassName = prodhierarchy.Find(y => y.SubClass == product.SubClass)?.Description;
                    poskutobeupdated.POProduct.DepartmentCode = prodhierarchy.Find(y => y.SubClass == product.SubClass)?.Department;
                    poskutobeupdated.POProduct.ClassShortDesc = prodhierarchy.Find(y => y.SubClass == product.SubClass)?.ShortDesc;

                    poskutobeupdated.POProduct.HTSCode = product.HTSCode;
                    poskutobeupdated.POProduct.Country = product.ProductVendors.Find(y => y.Sku == product.Sku)?.CountryOfOrigin;
                    poskutobeupdated.POProduct.SkuDesc = product.SkuDesc;
                    poskutobeupdated.POProduct.MasterPackQuantity = product.ProductVendors.Find(y => y.Sku == product.Sku)?.MasterPackQuantity;

                    // Product Flags
                    poskutobeupdated.POProduct.ProductFlagsOutput.IntlSafetyTransitTestRequired = !string.IsNullOrEmpty((product?.ProductFlags?.Find(y => y.FlagKey == ProductFlagSettings.IntlSafetyTransitTestRequired)?.FlagValue.ConvertToString())) ? product.ProductFlags.Find(y => y.FlagKey == ProductFlagSettings.IntlSafetyTransitTestRequired).FlagValue.ConvertToString() : "N";
                    poskutobeupdated.POProduct.ProductFlagsOutput.IsChildClothCostume = !string.IsNullOrEmpty(product?.ProductFlags?.Find(y => y.FlagKey == ProductFlagSettings.IsChildClothCostume)?.FlagValue.ConvertToString()) ? product.ProductFlags.Find(y => y.FlagKey == ProductFlagSettings.IsChildClothCostume).FlagValue.ConvertToString() : "N";
                    poskutobeupdated.POProduct.ProductFlagsOutput.IsGlassDishAdultJewelry = !string.IsNullOrEmpty(product?.ProductFlags?.Find(y => y.FlagKey == ProductFlagSettings.IsGlassDishAdultJewelry)?.FlagValue.ConvertToString()) ? product.ProductFlags.Find(y => y.FlagKey == ProductFlagSettings.IsGlassDishAdultJewelry).FlagValue.ConvertToString() : "N";
                    poskutobeupdated.POProduct.ProductFlagsOutput.NonPaintTestingRequired = !string.IsNullOrEmpty(product?.ProductFlags?.Find(y => y.FlagKey == ProductFlagSettings.NonPaintTestingRequired)?.FlagValue.ConvertToString()) ? product.ProductFlags.Find(y => y.FlagKey == ProductFlagSettings.NonPaintTestingRequired).FlagValue.ConvertToString() : "N";
                    poskutobeupdated.POProduct.ProductFlagsOutput.RandomInspectionRequired = !string.IsNullOrEmpty(product?.ProductFlags?.Find(y => y.FlagKey == ProductFlagSettings.RandomInspectionRequired)?.FlagValue.ConvertToString()) ? product.ProductFlags.Find(y => y.FlagKey == ProductFlagSettings.RandomInspectionRequired).FlagValue.ConvertToString() : "N";
                    poskutobeupdated.POProduct.ProductFlagsOutput.CPSIATestingRequired = !string.IsNullOrEmpty(product?.ProductFlags?.Find(y => y.FlagKey == ProductFlagSettings.CPSIATestingRequired)?.FlagValue.ConvertToString()) ? product.ProductFlags.Find(y => y.FlagKey == ProductFlagSettings.CPSIATestingRequired).FlagValue.ConvertToString() : "N";
                    poskutobeupdated.POProduct.ProductFlagsOutput.Sku = product.Sku;
                }
                else
                {

                    POProductFlagsOutput productFlagsOutput = new POProductFlagsOutput
                    {
                        Sku = product.Sku,
                        IntlSafetyTransitTestRequired = !string.IsNullOrEmpty((product?.ProductFlags?.Find(y => y.FlagKey == ProductFlagSettings.IntlSafetyTransitTestRequired)?.FlagValue.ConvertToString())) ? product.ProductFlags.Find(y => y.FlagKey == ProductFlagSettings.IntlSafetyTransitTestRequired).FlagValue.ConvertToString() : "N",
                        IsChildClothCostume = !string.IsNullOrEmpty(product?.ProductFlags?.Find(y => y.FlagKey == ProductFlagSettings.IsChildClothCostume)?.FlagValue.ConvertToString()) ? product.ProductFlags.Find(y => y.FlagKey == ProductFlagSettings.IsChildClothCostume).FlagValue.ConvertToString() : "N",
                        IsGlassDishAdultJewelry = !string.IsNullOrEmpty(product?.ProductFlags?.Find(y => y.FlagKey == ProductFlagSettings.IsGlassDishAdultJewelry)?.FlagValue.ConvertToString()) ? product.ProductFlags.Find(y => y.FlagKey == ProductFlagSettings.IsGlassDishAdultJewelry).FlagValue.ConvertToString() : "N",
                        NonPaintTestingRequired = !string.IsNullOrEmpty(product?.ProductFlags?.Find(y => y.FlagKey == ProductFlagSettings.NonPaintTestingRequired)?.FlagValue.ConvertToString()) ? product.ProductFlags.Find(y => y.FlagKey == ProductFlagSettings.NonPaintTestingRequired).FlagValue.ConvertToString() : "N",
                        RandomInspectionRequired = !string.IsNullOrEmpty(product?.ProductFlags?.Find(y => y.FlagKey == ProductFlagSettings.RandomInspectionRequired)?.FlagValue.ConvertToString()) ? product.ProductFlags.Find(y => y.FlagKey == ProductFlagSettings.RandomInspectionRequired).FlagValue.ConvertToString() : "N",
                        CPSIATestingRequired = !string.IsNullOrEmpty(product?.ProductFlags?.Find(y => y.FlagKey == ProductFlagSettings.CPSIATestingRequired)?.FlagValue.ConvertToString()) ? product.ProductFlags.Find(y => y.FlagKey == ProductFlagSettings.CPSIATestingRequired).FlagValue.ConvertToString() : "N",
                    };
                    POProductOutput poProductOutput = new POProductOutput
                    {
                        ClassCode = prodhierarchy.Find(y => y.SubClass == product.SubClass)?.Class,
                        DepartmentName = prodhierarchy.Find(y => y.SubClass == product.SubClass)?.Department,
                        ClassName = prodhierarchy.Find(y => y.SubClass == product.SubClass)?.Description,
                        ClassShortDesc = prodhierarchy.Find(y => y.SubClass == product.SubClass)?.ShortDesc,
                        DepartmentCode = prodhierarchy.Find(y => y.SubClass == product.SubClass)?.Department,

                        HTSCode = product.HTSCode,
                        Country = product.ProductVendors?.Find(y => y.Sku == product.Sku)?.CountryOfOrigin,
                        MasterPackQuantity = !string.IsNullOrEmpty(product.ProductVendors?.Find(y => y.Sku == product.Sku)?.MasterPackQuantity) ? product.ProductVendors?.Find(y => y.Sku == product.Sku)?.MasterPackQuantity : "0",
                        SkuDesc = product.SkuDesc,
                        SKU = product.Sku,
                        //Product Flags
                        ProductFlagsOutput = productFlagsOutput

                    };
                    POIntertekSKUOutput POSkusOutput = new POIntertekSKUOutput
                    {
                        PONumber = posku.PONumber.ToString(),
                        BuyQuantity =posku.BuyQuantity.HasValue? posku.BuyQuantity.Value.ToString():"0",
                        CreateDate = posku.CreateDate.Value.Date.ToString("yyyyMMdd"),
                        DutyCost = posku.DutyCost.HasValue ? Math.Round(posku.DutyCost.GetValueOrDefault(), 2).ToString() :"0",
                        FirstCost = posku.FirstCost.HasValue ? Math.Round(posku.FirstCost.GetValueOrDefault(), 2).ToString() : "0",
                        SKU = posku.SKU,
                        DutyPctOfFOB = posku.DutyPctOfFOB.HasValue? Math.Round(posku.DutyPctOfFOB.GetValueOrDefault(),2).ToString():"0",
                        EstimatedLandedCost = posku.EstimatedLandedCost.HasValue ? Math.Round(posku.EstimatedLandedCost.GetValueOrDefault(), 2).ToString() : "0",
                        MasterPackCubicFeet = posku.MasterPackCubicFeet.HasValue? Math.Round(posku.MasterPackCubicFeet.GetValueOrDefault(),2).ToString():"0",
                        PrepackId = posku.PrepackId,
                        ApprovalLetter = posku.ApprovalLetter.HasValue ? (posku.ApprovalLetter.Value == true ? "Y" : "N") : "N",
                        StatusCode = posku.StatusCode,
                        POProduct = poProductOutput,
                    };

                    if (poobject.POSkus == null)
                    {
                        poobject.POSkus = new System.Collections.Generic.List<POIntertekSKUOutput>();
                    }
                    poobject.POSkus.Add(POSkusOutput);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdatePOObject - Failed Updating IntertekPO: {Reason}", ex.Message);
            }

        }


        public async void UpdatePOObjectForVendor(POIntertekOutput poobject, MMSSubVendorEvent vendor)
        {
            try
            {
                poobject.VendorName = vendor.VendName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdatePOObjectForVendor - Failed Updating IntertekPO: {Reason}", ex.Message);
            }

        }

    }
}