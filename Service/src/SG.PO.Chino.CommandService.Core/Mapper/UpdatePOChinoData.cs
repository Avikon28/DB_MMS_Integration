using SG.MMS.PO.Events;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.Chino.DataModels.Outputmodels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SG.PO.Chino.CommandService.Core.Mapper
{
    public static class UpdatePOChinoData
    {
        public static void UpdateChinoData(this POChinoOutput poChino, MMSPOEvent model)
        {
            poChino.StatusCode = model.StatusCode;
            poChino.DeliveryStart = model.DeliveryDate;
            poChino.PickupStart = model.DeliveryDate;
            poChino.DeliveryEnd = model.EstimatedArrivalDate;
        }

        public static void UpdatePOChinoPoSkuData(this POChinoOutput pochino, List<POSkus> poskus)
        {
            if (poskus != null && poskus.Count > 0)
            {
                List<POSkusOutput> poskustobeaddedtoPO = new List<POSkusOutput>();
                pochino.POSkus.ForEach(x =>
                {
                    var skutobeupdatefrom = poskus.Find(y => y.SKU == x.ItemName);
                    if (skutobeupdatefrom != null)
                    {
                        x.OrderQty = skutobeupdatefrom.BuyQuantity.ToString();
                    }
                });
                var poskustobeadded = poskus.Where(x => !pochino.POSkus.Any(y => y.ItemName == x.SKU));
                poskustobeadded?.ToList().ForEach(y =>
                {
                    poskustobeaddedtoPO.Add(new POSkusOutput
                    {
                        ItemName = y.SKU,
                        OrderQty = y.BuyQuantity.ToString()
                    });
                });

                if (poskustobeadded?.ToList().Count > 0)
                {
                    pochino.POSkus.AddRange(poskustobeaddedtoPO);
                }

            }
        }
    }
}
