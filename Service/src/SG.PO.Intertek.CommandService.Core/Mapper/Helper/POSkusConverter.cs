using AutoMapper;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.Intertek.CommandService.Core.Helper;
using SG.PO.Intertek.DataModels.Outputmodels;

using System;
using System.Collections.Generic;
using System.Linq;

namespace SG.PO.Intertek.CommandService.Core.Mapper.Helper
{
    public class POSkusConverter : ITypeConverter<POO, ICollection<POIntertekSKUOutput>>
    {
        ICollection<POIntertekSKUOutput> ITypeConverter<POO, ICollection<POIntertekSKUOutput>>.Convert(POO source, ICollection<POIntertekSKUOutput> destination, ResolutionContext context)
        {
            List<POIntertekSKUOutput> poSkus = new List<POIntertekSKUOutput>();
            POProductOutput poIntertekProduct = new POProductOutput();

            source.POSkus?.ToList().ForEach(x =>
            {
                POProductFlagsOutput productFlagsOutput = new POProductFlagsOutput
                {
                    Sku = x.SKU,
                    IntlSafetyTransitTestRequired = !string.IsNullOrEmpty((x.POProduct?.ProductFlags?.Find(y => y.FlagKey == ProductFlagSettings.IntlSafetyTransitTestRequired)?.FlagValue.ConvertToString())) ? x.POProduct.ProductFlags.Find(y => y.FlagKey == ProductFlagSettings.IntlSafetyTransitTestRequired).FlagValue.ConvertToString() : "N",
                    IsChildClothCostume = !string.IsNullOrEmpty(x.POProduct?.ProductFlags?.Find(y => y.FlagKey == ProductFlagSettings.IsChildClothCostume)?.FlagValue.ConvertToString()) ? x.POProduct.ProductFlags.Find(y => y.FlagKey == ProductFlagSettings.IsChildClothCostume).FlagValue.ConvertToString() : "N",
                    IsGlassDishAdultJewelry = !string.IsNullOrEmpty(x.POProduct?.ProductFlags?.Find(y => y.FlagKey == ProductFlagSettings.IsGlassDishAdultJewelry)?.FlagValue.ConvertToString()) ? x.POProduct.ProductFlags.Find(y => y.FlagKey == ProductFlagSettings.IsGlassDishAdultJewelry).FlagValue.ConvertToString() : "N",
                    NonPaintTestingRequired = !string.IsNullOrEmpty(x.POProduct?.ProductFlags?.Find(y => y.FlagKey == ProductFlagSettings.NonPaintTestingRequired)?.FlagValue.ConvertToString()) ? x.POProduct.ProductFlags.Find(y => y.FlagKey == ProductFlagSettings.NonPaintTestingRequired).FlagValue.ConvertToString() : "N",
                    RandomInspectionRequired = !string.IsNullOrEmpty(x.POProduct?.ProductFlags?.Find(y => y.FlagKey == ProductFlagSettings.RandomInspectionRequired)?.FlagValue.ConvertToString()) ? x.POProduct.ProductFlags.Find(y => y.FlagKey == ProductFlagSettings.RandomInspectionRequired).FlagValue.ConvertToString() : "N",
                    CPSIATestingRequired = !string.IsNullOrEmpty(x.POProduct?.ProductFlags?.Find(y => y.FlagKey == ProductFlagSettings.CPSIATestingRequired)?.FlagValue.ConvertToString()) ? x.POProduct.ProductFlags.Find(y => y.FlagKey == ProductFlagSettings.CPSIATestingRequired).FlagValue.ConvertToString() : "N",
                };


                if (x.POProduct != null)
                {
                    poIntertekProduct = new POProductOutput
                    {
                        SKU = x.SKU,
                        ClassCode = x.POProduct.Class,
                        DepartmentName = x.POProduct.ClassDescription,
                        ClassName = x.POProduct.ClassDescription,
                        DepartmentCode = x.POProduct.Department,
                        HTSCode = x.POProduct.HTSCode,
                        MasterPackQuantity = x.POProduct.MasterPackQuantity.HasValue ? x.POProduct.MasterPackQuantity.Value.ToString() : "0",
                        ClassShortDesc = x.POProduct.ClassShortDesc,
                        SkuDesc = x.POProduct.SkuDesc,
                        SubClass = x.POProduct.SubClass,
                        SubclassDescription = x.POProduct.SubclassDescription,
                        Country = x.POProduct.CountryOfOrigin,
                        PrepackChildQuantity = (x.POProduct.ProductPrepack != null && x.POProduct.ProductPrepack.Count > 0) ? Convert.ToString(x.POProduct.ProductPrepack.Find(y => y.Prepack == x.PrepackId && y.Sku == x.SKU).Quantity) : string.Empty,
                        PrepackTotalQuantity = (x.POProduct.ProductPrepack != null && x.POProduct.ProductPrepack.Count > 0) ? Convert.ToString(x.POProduct.ProductPrepack.Where(y => y.Prepack == x.PrepackId).Sum(y => y.Quantity)) : "0",
                        ProductFlagsOutput = productFlagsOutput
                    };
                }
                poSkus.Add(new POIntertekSKUOutput
                {
                    PONumber = x.PONumber.ToString(),
                    BuyQuantity = x.BuyQuantity.HasValue ? x.BuyQuantity.Value.ToString() : "0",
                    CreateDate = x.CreateDate.HasValue ? x.CreateDate.Value.Date.ToString("yyyyMMdd") : string.Empty,
                    DutyCost = x.DutyCost.HasValue ? Math.Round(x.DutyCost.GetValueOrDefault(), 2).ToString() : "0",
                    FirstCost = x.FirstCost.HasValue ? Math.Round(x.FirstCost.GetValueOrDefault(), 2).ToString() : "0",
                    SKU = x.SKU,
                    POProduct = poIntertekProduct,
                    StatusCode = x.StatusCode,
                    DutyPctOfFOB = x.DutyPctOfFOB.HasValue ? Math.Round(x.DutyPctOfFOB.GetValueOrDefault(),2).ToString() : "0",
                    EstimatedLandedCost = x.EstimatedLandedCost.HasValue ? Math.Round(x.EstimatedLandedCost.GetValueOrDefault(), 2).ToString() : "0",
                    MasterPackCubicFeet = x.MasterPackCubicFeet.HasValue ? Math.Round(x.MasterPackCubicFeet.GetValueOrDefault(),2).ToString() : "0",
                    PrepackId = x.PrepackId,
                    ApprovalLetter = x.ApprovalLetter.HasValue ? (x.ApprovalLetter.Value == true ? "Y" : "N") : "N"
                });
            });

            return poSkus;
        }
    }
}
