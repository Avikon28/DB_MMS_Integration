using AutoMapper;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.Contempo.CommandService.Core.Helper;
using SG.PO.Contempo.DataModels.Outputmodels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SG.PO.Contempo.CommandService.Core.Mapper.Helper
{
    public class POContempoSkusConverter : ITypeConverter<POO, IList<POContempoSkuOutput>>
    {
        IList<POContempoSkuOutput> ITypeConverter<POO, IList<POContempoSkuOutput>>.Convert(POO source, IList<POContempoSkuOutput> destination, ResolutionContext context)
        {
            List<POContempoSkuOutput> poSkus = new List<POContempoSkuOutput>();

            source.POSkus?.ToList().ForEach(x =>
            {
                if ((x.POProduct.Department == "JEP" || x.POProduct.Department == "JSF") && x.POProduct.SubClass != "JOSAC")
                {
                    POContempoProductOutput poCntmpProductOutput = new POContempoProductOutput
                    {
                        APVendor = x.POProduct.APVendor,
                        Class = x.POProduct.Class,
                        ClassLevelDesc = x.POProduct.ClassDescription,
                        SubClass = x.POProduct.SubClass,
                        SubClassLevelDesc = x.POProduct.SubclassDescription,
                        Size = x.POProduct.Size,
                        CountryOfOrigin = x.POProduct.CountryOfOrigin,
                        SkuDesc = x.POProduct.SkuDescShrt,
                        VendorSKUCode = x.POProduct.VendorSkuCode,
                        LabelType = x.POProduct?.LabelType,
                        LabelDescription = x.POProduct?.LabelDescription,
                        RetailPrice = x.GetRetailPrice()
                    };

                    poSkus.Add(new POContempoSkuOutput
                    {
                        SKU = x.SKU,
                        BuyQuanity = x.BuyQuantity != null ? Convert.ToInt32(x.BuyQuantity) : 0,
                        POProduct = poCntmpProductOutput,
                        //POSkus object is updated with CreateDate property.
                        CreateDate = x.CreateDate != null ? x.CreateDate.Value : new DateTime?(),
                        ModifiedDate = x.ModifiedDate != null ? x.ModifiedDate.Value : new DateTime?(),
                        StatusCode = x.StatusCode,
                    });
                }
            });

            return poSkus;
        }
    }
}
