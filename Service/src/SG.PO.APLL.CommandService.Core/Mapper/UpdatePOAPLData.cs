using SG.MMS.PO.Events;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.APLL.CommandService.Core.Helper;
using SG.PO.APLL.DataModel.Outputmodels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SG.PO.APLL.CommandService.Core.Mapper
{
    public static class UpdatePOAPLData 
    {
        public static void UpdatePOAPL(this POAPLLOutput poapl, MMSPOEvent model)
        {

            poapl.StatusCode = model.StatusCode;
            //poapl.Mi9ChannelCode = model.Mi9ChannelCode;
            poapl.SubVendorNumber = model.SubVendorNumber;
            //poapl.Approved = model.Approved;
            poapl.WarehouseDueDate = model.DeliveryDate?.FormatTo();
            poapl.LastShipDate = model.CancelDate?.FormatTo();
            poapl.EarlyShipDate = model.ShipDate?.FormatTo();
            poapl.StoreCode = model.LocationNumber;
            //poapl.Mi9POType = model.Mi9POType;
            //poapl.Terms = model.Terms;
            //poapl.PointofOwnership = model.PointofOwnership;
            poapl.BuyerCode = model.EmployeeID;
            //poapl.SeasonCode = model.SeasonCode;
            //poapl.CurrencyCode = model.CurrencyCode;
            //poapl.CalculatePOExtraCosts = model.CalculatePOExtraCosts;
            //poapl.AutoCloseFlag = model.AutoCloseFlag;

            //TODO
            //poapl.LOB=model.LOB;



        }

        public static void UpdatePOAPLPoSkuData(this POAPLLOutput poapl, List<POSkus> poskus)
        {
            if (poskus != null && poskus.Count > 0)
            {
                List<POSkusOutput> poskustobeaddedtoPO = new List<POSkusOutput>();
                poapl.POSkus.ForEach(x =>
                {
                    var skutobeupdatefrom = poskus.Find(y => y.SKU == x.ItemNumber);
                    if (skutobeupdatefrom != null)
                    {
                        x.DeliveryDate = skutobeupdatefrom.DeliveryDate?.ToString();
                        x.UnitCost = skutobeupdatefrom.FirstCost!=null ? Math.Round(skutobeupdatefrom.FirstCost.GetValueOrDefault(), 2).ToString(): "0";
                        x.RetailPrice = skutobeupdatefrom.RetailPrice?.ToString();
                        x.ItemQty = skutobeupdatefrom.BuyQuantity.HasValue ? skutobeupdatefrom.BuyQuantity.ToString() : "0";
                        x.ItemTotalQuantity = skutobeupdatefrom.BuyQuantity.HasValue? skutobeupdatefrom.BuyQuantity.ToString() : "0";
                        x.CreateDate = skutobeupdatefrom.CreateDate?.ToString();
                        x.ModifiedDate = skutobeupdatefrom.ModifiedDate?.ToString();
                        x.ApprovalLetter = skutobeupdatefrom.ApprovalLetter?.ToString();
                        x.SamplesRequired = skutobeupdatefrom.SamplesRequired?.ToString();
                        x.EstimatedLandedCost = skutobeupdatefrom.EstimatedLandedCost!=null? Math.Round(skutobeupdatefrom.EstimatedLandedCost.GetValueOrDefault(), 2).ToString() : "0";
                        x.DutyPctOfFOB = skutobeupdatefrom.DutyPctOfFOB?.ToString();
                    }

                   

                });

                //add the poskus if applicable
                var poskustobeadded = poskus.Where(x => !poapl.POSkus.Any(y => y.ItemNumber == x.SKU));
                poskustobeadded?.ToList().ForEach(y =>
                {
                    poskustobeaddedtoPO.Add(new POSkusOutput
                    {
                        ItemNumber = y.SKU,
                        DeliveryDate = y.DeliveryDate?.ToString(),
                        UnitCost = y.FirstCost != null ? Math.Round(y.FirstCost.GetValueOrDefault(), 2).ToString() : "0",
                        RetailPrice = y.RetailPrice?.ToString(),
                        ItemQty = y.BuyQuantity.HasValue ? y.BuyQuantity.ToString() : "0",
                        ItemTotalQuantity = y.BuyQuantity.HasValue ? y.BuyQuantity.ToString() : "0",
                        CreateDate = y.CreateDate?.ToString(),
                        ModifiedDate = y.ModifiedDate?.ToString(),
                        ApprovalLetter = y.ApprovalLetter?.ToString(),
                        SamplesRequired = y.SamplesRequired?.ToString(),
                        EstimatedLandedCost = y.EstimatedLandedCost != null ? Math.Round(y.EstimatedLandedCost.GetValueOrDefault(), 2).ToString() : "0",
                        DutyPctOfFOB = y.DutyPctOfFOB?.ToString()
                    });
                });

                if (poskustobeadded?.ToList().Count>0)
                {
                    poapl.POSkus.AddRange(poskustobeaddedtoPO);
                }

            }
        }


    }
}
