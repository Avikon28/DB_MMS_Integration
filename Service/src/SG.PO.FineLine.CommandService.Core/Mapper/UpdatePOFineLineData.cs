using SG.MMS.PO.Events;
using SG.PO.FineLine.DataModels;
using System.Collections.Generic;
using SG.PO.FineLine.CommandService.Core.Helper;
using System;
using System.Linq;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.FineLine.DataModels.Outputmodels;

namespace SG.PO.FineLine.CommandService.Core.Mapper
{
    public static class UpdatePOFineLineData
    {
        public static void UpdatePOFineLine(this POFineLineOutput pofineline, MMSPOEvent model)
        {
            pofineline.PurchaseOrder = model.PONumber;
            pofineline.Currency = model.CurrencyCode;
            pofineline.StatusCode = model.StatusCode;
            pofineline.SubVendorNumber = model.SubVendorNumber;
        }

        public static void UpdatePOFineLinePoSkuData(this POFineLineOutput pofineline, List<POSkus> poskus)
        {
            if (poskus != null && poskus.Count > 0)
            {
                List<POFineLineSkuOutput> poskustobeaddedtoPO = new List<POFineLineSkuOutput>();
                pofineline.POSkus.ForEach(x =>
                {
                    var skutobeupdatefrom = poskus.Find(y => y.SKU == x.SKUNumber);
                    if (skutobeupdatefrom != null)
                    {
                        x.PurchaseOrderDate = skutobeupdatefrom.CreateDate != null ? skutobeupdatefrom.CreateDate.Value : new DateTime?();
                        x.PurchaseOrderReviseDate = skutobeupdatefrom.ModifiedDate != null ? skutobeupdatefrom.ModifiedDate.Value : new DateTime?();

                        x.OrderQuantity = (skutobeupdatefrom.BuyQuantity == null) ? 0 : skutobeupdatefrom.BuyQuantity.Value;
                        x.StatusCode = skutobeupdatefrom.StatusCode;
                    }
                    

                });

                //add the poskus if applicable

                var poskustobeadded = poskus.Where(x => !pofineline.POSkus.Any(y=>y.SKUNumber == x.SKU));
                poskustobeadded?.ToList().ForEach(y =>
                {
                    poskustobeaddedtoPO.Add(new POFineLineSkuOutput
                    {
                        SKUNumber = y.SKU,
                        PurchaseOrderDate = y.CreateDate != null ? y.CreateDate.Value : new DateTime?(),
                        PurchaseOrderReviseDate = y.ModifiedDate != null ? y.ModifiedDate.Value : new DateTime?(),
                        OrderQuantity = (y.BuyQuantity == null) ? 0 : y.BuyQuantity.Value,
                        StatusCode = y.StatusCode,
                    });
                });


                if (poskustobeadded?.ToList().Count > 0)
                {
                    pofineline.POSkus.AddRange(poskustobeaddedtoPO);
                }

            }
        }
        public static string GetRetailPrice(this POSkus rtlPrice)
        {
            decimal retRetailPrice = 0.0m;
            if (rtlPrice.CreateDate != null && rtlPrice.CreateDate.Value.ToString("MM/dd/yyyy") == DateTime.Today.ToString("MM/dd/yy"))
            {
                retRetailPrice = (rtlPrice.RetailPrice == null) ? 0 : Math.Round(rtlPrice.RetailPrice.GetValueOrDefault(), 2);
            }
            else
            {
                ProductRetail prdRtl = rtlPrice.POProduct?.ProductRetail?.Where(p => p.Sku == rtlPrice.SKU && p.RetailType == "Ticket" && p.CurrencyCode == "USD").FirstOrDefault();
                retRetailPrice = prdRtl != null ? (prdRtl.Retail != null ? Math.Round(prdRtl.Retail.GetValueOrDefault(), 2) : 0) : 0;
            }
            return retRetailPrice.ToString();
        }
        public static string GetRetailPrice(this decimal rtlPrice)
        {
            return ((rtlPrice == 0) ? "0" : Math.Round(rtlPrice, 2).ToString());
        }
    }
}
