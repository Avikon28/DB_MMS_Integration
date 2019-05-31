using AutoMapper;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.APLL.DataModel.Outputmodels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SG.PO.APLL.CommandService.Core.Mapper.Helper
{
    public class POSkusConverter : ITypeConverter<POO, ICollection<POSkusOutput>>
    {
        ICollection<POSkusOutput> ITypeConverter<POO, ICollection<POSkusOutput>>.Convert(POO source, ICollection<POSkusOutput> destination, ResolutionContext context)
        {
            List<POSkusOutput> poSkus = new List<POSkusOutput>();
            

            source.POSkus?.ToList().ForEach(x =>
            {
                POProductOutput poProductOutput = new POProductOutput
                {
                    Sku= x.POProduct.Sku,
                    CountryOfOrigin = x.POProduct.CountryOfOrigin,
                    ItemDescription = x.POProduct.SkuDesc,
                    CasePackQty = x.POProduct.MasterPackQuantity.HasValue? x.POProduct.MasterPackQuantity.ToString() :"0",
                    ClassCode=x.POProduct.Class,
                    TariffCode=x.POProduct.HTSCode,
                    VendorName=x.POProduct.SubVendor,
                    VendorNumber=x.POProduct.VendorName,
                    DepartmentName=x.POProduct.ClassDescription,
                    ClassName=x.POProduct.ClassDescription,
                    DepartmentCode=x.POProduct.Department
                };
                poSkus.Add(new POSkusOutput
                {
                    PONumber = x.PONumber.ToString(),
                    ItemNumber = x.SKU,
                    ItemQty = x.BuyQuantity.HasValue? x.BuyQuantity.ToString() : "0",
                    ItemTotalQuantity = x.BuyQuantity.HasValue ? x.BuyQuantity.ToString() :"0",
                    CreateDate = x.CreateDate.HasValue ? x.CreateDate.Value.Date.ToString("yyyyMMdd") : string.Empty,
                    ModifiedDate = x.ModifiedDate.HasValue ? x.ModifiedDate.Value.Date.ToString("yyyyMMdd") : string.Empty,
                    ApprovalLetter = x.ApprovalLetter.HasValue ? (x.ApprovalLetter.Value == true ? "Y" : "N") : "N",
                    SamplesRequired = x.SamplesRequired.ToString(),

                    EstimatedLandedCost = x.EstimatedLandedCost!=null? Math.Round(x.EstimatedLandedCost.GetValueOrDefault(), 2).ToString() : "0",
                    MasterPackCubicFeet = x.MasterPackCubicFeet.ToString(),
                    DutyPctOfFOB = x.DutyPctOfFOB.ToString(),
                    DutyCost = x.DutyCost.HasValue ? Math.Round(x.DutyCost.GetValueOrDefault(), 2).ToString() : "0",
                    //POCreationDate
                    UnitCost = x.FirstCost!=null? Math.Round(x.FirstCost.GetValueOrDefault(), 2).ToString() :"0",
                    POProduct= poProductOutput,
                });
            });

            return poSkus;
        }
    }
}
