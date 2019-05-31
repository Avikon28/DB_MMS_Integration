using SG.MMS.PO.Events;
using SG.PO.Intertek.CommandService.Core.Helper;
using SG.PO.Intertek.DataModels.Outputmodels;
using System.Collections.Generic;
using System;
using System.Linq;
using SG.MMS.QueryService.ODATA.Models.PO;

namespace SG.PO.Intertek.CommandService.Core.Mapper
{
    public static class UpdatePOIntertekData
    {
        public static void UpdateIntertekData(this POIntertekOutput poIntertek, MMSPOEvent model)
        {
            poIntertek.CancelDate = model.CancelDate?.FormatTo();
            poIntertek.DeliveryDate = model.DeliveryDate?.FormatTo();            
            poIntertek.LocationNumber = model.LocationNumber;
            poIntertek.EmployeeId = model.EmployeeID;
            poIntertek.LocationNumber = model.LocationNumber;
            poIntertek.PONumber = model.PONumber;
            poIntertek.ShipDate = model.ShipDate?.FormatTo();
            poIntertek.SubVendorNumber = model.SubVendorNumber;
            poIntertek.LOB = model.LineOfBusiness;
            poIntertek.DistributorId = model.DistributorId;
            poIntertek.CurrencyCode = model.CurrencyCode;
            poIntertek.StatusCode = model.StatusCode;
        }
        public static void UpdatePOIntertekPoSkuData(this POIntertekOutput poIntertek, List<POSkus> poskus)
        {
            if (poskus != null && poskus.Count > 0)
            {
                List<POIntertekSKUOutput> poskustobeaddedtoPO = new List<POIntertekSKUOutput>();

                poskus.ForEach(x =>
                {
                    var updatePoSKU = poIntertek.POSkus.Find(y => y.SKU == x.SKU);
                    if (updatePoSKU != null)
                    { 
                        updatePoSKU.BuyQuantity = x.BuyQuantity.HasValue?x.BuyQuantity.Value.ToString():"0";
                        updatePoSKU.CreateDate = x.CreateDate?.Date.ToString("yyyyMMdd");
                        updatePoSKU.DutyCost = x.DutyCost.HasValue ? Math.Round(x.DutyCost.GetValueOrDefault(),2).ToString(): "0";
                        updatePoSKU.FirstCost = x.FirstCost.HasValue? Math.Round(x.FirstCost.GetValueOrDefault(),2).ToString(): "0";
                        updatePoSKU.MasterPackCubicFeet = x.MasterPackCubicFeet.HasValue? Math.Round(x.MasterPackCubicFeet.GetValueOrDefault(), 2).ToString() : "0";
                        updatePoSKU.DutyPctOfFOB = x.DutyPctOfFOB.HasValue ? Math.Round(x.DutyPctOfFOB.GetValueOrDefault(), 2).ToString() : "0";
                        updatePoSKU.EstimatedLandedCost = x.EstimatedLandedCost.HasValue? Math.Round(x.EstimatedLandedCost.GetValueOrDefault(),2).ToString():"0";
                        updatePoSKU.PrepackId = x.PrepackId;
                        updatePoSKU.ApprovalLetter = x.ApprovalLetter.HasValue ? (x.ApprovalLetter.Value == true ? "Y" : "N") : string.Empty;
                        updatePoSKU.StatusCode = x.StatusCode;
                    }
                });

                var poskustobeadded = poskus.Where(x => !poIntertek.POSkus.Any(y => y.SKU == x.SKU));
                poskustobeadded?.ToList().ForEach(y =>
                {
                        poskustobeaddedtoPO.Add(new POIntertekSKUOutput
                        {
                            BuyQuantity = y.BuyQuantity.HasValue ? y.BuyQuantity.Value.ToString() : "0",
                            CreateDate = y.CreateDate.HasValue ? Convert.ToString(y.CreateDate.Value.Date.ToString("yyyyMMdd")) : string.Empty,
                            DutyCost = y.DutyCost.HasValue? Math.Round(y.DutyCost.GetValueOrDefault(),2).ToString():"0",
                            FirstCost = y.FirstCost.HasValue? Math.Round(y.FirstCost.GetValueOrDefault(),2).ToString():"0",
                            MasterPackCubicFeet = y.MasterPackCubicFeet.HasValue ? Math.Round(y.MasterPackCubicFeet.GetValueOrDefault(), 2).ToString() : "0",
                            DutyPctOfFOB = y.DutyPctOfFOB.HasValue ? Math.Round(y.DutyPctOfFOB.GetValueOrDefault(), 2).ToString() : "0",
                            EstimatedLandedCost = y.EstimatedLandedCost.HasValue ? Math.Round(y.EstimatedLandedCost.GetValueOrDefault(), 2).ToString() : "0",
                            PrepackId = y.PrepackId,
                            SKU = y.SKU,
                            ApprovalLetter = y.ApprovalLetter.HasValue ? (y.ApprovalLetter.Value == true ? "Y" : "N") : string.Empty,
                            StatusCode = y.StatusCode
                        });
                });

                if (poskustobeadded?.ToList().Count > 0)
                {
                    poIntertek.POSkus.AddRange(poskustobeaddedtoPO);
                }

            }
        }
    }
}
