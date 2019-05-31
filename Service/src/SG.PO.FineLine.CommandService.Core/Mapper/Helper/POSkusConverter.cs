using AutoMapper;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.FineLine.DataModels;
using SG.PO.FineLine.DataModels.Outputmodels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SG.PO.FineLine.CommandService.Core.Mapper.Helper
{
    public class POSkusConverter : ITypeConverter<POO, ICollection<POFineLineSkuOutput>>
    {
        ICollection<POFineLineSkuOutput> ITypeConverter<POO, ICollection<POFineLineSkuOutput>>.Convert(POO source, ICollection<POFineLineSkuOutput> destination, ResolutionContext context)
        {
            List<POFineLineSkuOutput> poFLSkus = new List<POFineLineSkuOutput>();

            source.POSkus?.ToList().ForEach(x =>
            {
                POFineLineProductOutput poFlProduct = new POFineLineProductOutput
                {
                    VendorNumber = x.POProduct.APVendor,
                    SubVendorNumber = x.POProduct.SubVendor,
                    SKUDescription = x.POProduct.SkuDescShrt,
                    VendorStyleNumber = x.POProduct.VendorSkuCode,
                    TicketType = x.POProduct.LabelType,
                    TicketDescription = x.POProduct.LabelDescription,
                    ClassID = x.POProduct.Class,
                    ClassDescription = x.POProduct.ClassDescription,
                    SubClassID = x.POProduct.SubClass,
                    SubClassDescription = x.POProduct.SubclassDescription,
                    Size = x.POProduct.Size,
                    ISOCountryCode = x.POProduct.CountryOfOrigin,
                    TicketRetail = x.GetRetailPrice()
                };
                poFLSkus.Add(new POFineLineSkuOutput
                {
                    SKUNumber = x.SKU,
                    PurchaseOrderDate = x.CreateDate != null ? x.CreateDate.Value : new DateTime?(),
                    PurchaseOrderReviseDate = x.ModifiedDate != null ? x.ModifiedDate.Value : new DateTime?(),
                    OrderQuantity = x.BuyQuantity != null ? Convert.ToInt32(x.BuyQuantity) : 0,
                    StatusCode = x.StatusCode,
                    POProduct = poFlProduct
                });
            });

            return poFLSkus;
        }
    }
}
