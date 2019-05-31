using AutoMapper;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.Chino.DataModels.Outputmodels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SG.PO.Chino.CommandService.Core.Mapper.Helper
{
    public class POSkusConverter : ITypeConverter<POO, ICollection<POSkusOutput>>
    {
        ICollection<POSkusOutput> ITypeConverter<POO, ICollection<POSkusOutput>>.Convert(POO source, ICollection<POSkusOutput> destination, ResolutionContext context)
        {
            List<POSkusOutput> poSkus = new List<POSkusOutput>();
            source.POSkus?.ToList().ForEach(x =>
            {
                poSkus.Add(new POSkusOutput
                {
                    ItemName = x.SKU,
                    OrderQty = x.BuyQuantity.ToString()
                });
            });

            return poSkus;
        }
    }
}
