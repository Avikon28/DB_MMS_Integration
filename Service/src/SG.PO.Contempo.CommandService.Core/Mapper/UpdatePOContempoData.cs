using SG.MMS.PO.Events;
using SG.PO.Contempo.CommandService.Core.OutputModels;
using SG.PO.Contempo.DataModels.Outputmodels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.Contempo.CommandService.Core.Helper;

namespace SG.PO.Contempo.CommandService.Core.Mapper
{
    public static class UpdatePOContempoData
    {
        public static void UpdatePOContempo(this POContempoOutput pocntmp, MMSPOEvent model)
        {
            pocntmp.CurrencyCode = model?.CurrencyCode;
            pocntmp.PONumber = model?.PONumber;
            pocntmp.SubVendor = model?.SubVendorNumber;
            pocntmp.StatusCode = model?.StatusCode;
        }

        public static void UpdatePOContempoPoSkuData(this POContempoOutput pocntmp, List<POSkus> poskus)
        {
            //check if the POSku exists, update accordingly
            if (poskus != null && poskus.Count > 0)
            {
                List<POContempoSkuOutput> poskustobeaddedtoPO = new List<POContempoSkuOutput>();

                poskus.ForEach(x =>
                {
                    var updatePoSKU = pocntmp.POSkus?.Find(p => p.SKU == x.SKU);
                    if (updatePoSKU != null)
                    {
                        updatePoSKU.SKU = x.SKU;
                        updatePoSKU.BuyQuanity = (x.BuyQuantity == null) ? 0 : x.BuyQuantity.Value;
                        updatePoSKU.StatusCode = x.StatusCode;
                        //CreateDate, ModifiedDate properties
                        updatePoSKU.CreateDate = x.CreateDate != null ? x.CreateDate.Value : new DateTime?();
                        updatePoSKU.ModifiedDate = x.ModifiedDate != null ? x.ModifiedDate.Value : new DateTime?();
                    }
                });
                //add the poskus if applicable

                var poskustobeadded = poskus.Where(x => !pocntmp.POSkus.Any(y => y.SKU == x.SKU));
                poskustobeadded?.ToList().ForEach(y =>
                {
                    poskustobeaddedtoPO.Add(new POContempoSkuOutput
                    {
                        SKU = y.SKU,
                        BuyQuanity = (y.BuyQuantity == null) ? 0 : y.BuyQuantity.Value,
                        StatusCode = y.StatusCode,
                        CreateDate = y.CreateDate != null ? y.CreateDate.Value : new DateTime?(),
                        ModifiedDate = y.ModifiedDate != null ? y.ModifiedDate.Value : new DateTime?(),
                    });
                });

                if (poskustobeadded?.ToList().Count > 0)
                {
                    pocntmp.POSkus.AddRange(poskustobeaddedtoPO);
                }
            }
        }

        public static string GetRetailPrice(this POSkus rtlPrice)
        {
            decimal retRetailPrice = 0.0m;
            if (rtlPrice.CreateDate != null && rtlPrice.CreateDate.Value.ToString("MM/dd/yyyy") == DateTime.Today.ToString("MM/dd/yyyy"))
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
