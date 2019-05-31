using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using SG.MMS.Product.Events;
using SG.PO.Contempo.DataModels.Outputmodels;
using SG.Shared.POProduct.Services;
using SG.Vendor.MMS.Events;
using System;
using System.Threading.Tasks;
using System.Linq;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.Contempo.CommandService.Core.Helper;

namespace SG.PO.Contempo.CommandService.Core.Mapper
{
    public class POContempoProductMapper
    {
        private static IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly ElasticClient _client;
        private readonly LookupDataService _lookUpService;        

        public POContempoProductMapper(IConfiguration configuration, ILogger<POContempoProductMapper> logger, ElasticClient client,
            LookupDataService lookUpService)
        {
            _configuration = configuration;
            _logger = logger;
            _client = client;
            _lookUpService = lookUpService;
        }
        
        public async Task<POContempoOutput> UpdatePOObject(POContempoOutput poobject, MMSProductEvent product, POSkus posku)
        {
            try
            {
                var prodhierarchy = await _lookUpService.GetProductHierarchy(product.SubClass);
                var productlabelcode = await _lookUpService.GetProductLabelDescription(product.LabelType);

                //check if exists
                if (poobject.POSkus != null && poobject.POSkus.Count > 0 && poobject.POSkus.Exists(y => y.SKU == posku.SKU) &&
                    (product?.Department == "JEP" || product?.Department == "JSF") && product?.SubClass != "JOSAC")
                {
                    var poskutobeupdated = poobject.POSkus.Find(y => y.SKU == posku.SKU);
                    poskutobeupdated.POProduct.Class = prodhierarchy?.Find(y => y.SubClass == product?.SubClass)?.Class;
                    poskutobeupdated.POProduct.CountryOfOrigin = product?.ProductVendors?.Find(y => y.Sku == product.Sku && y.SubVendor == poobject.SubVendor)?.CountryOfOrigin;
                    poskutobeupdated.POProduct.SubClass = prodhierarchy?.Find(y => y.SubClass == product?.SubClass)?.SubClass;
                    poskutobeupdated.POProduct.APVendor = posku.POProduct?.APVendor;
                    poskutobeupdated.POProduct.VendorSKUCode = product?.ProductVendors?.Find(y => y.Sku == product.Sku && y.SubVendor == poobject.SubVendor)?.VendorSkuCode;
                    poskutobeupdated.POProduct.ClassLevelDesc = prodhierarchy?.Find(y => y.SubClass == product?.SubClass)?.Description;
                    poskutobeupdated.POProduct.Size = posku.POProduct?.Size;
                    poskutobeupdated.POProduct.LabelType = product?.LabelType;
                    //TODO
                    //This LabelDescription value should be populated from "MMSProductEvent" ProductVendors objects labeldescription
                    //For now, it is coming from database so that we know the value is populating.
                    poskutobeupdated.POProduct.LabelDescription = productlabelcode?.Description;
                    poskutobeupdated.POProduct.SkuDesc = product?.SkuDescShrt;
                    poskutobeupdated.POProduct.SubClassLevelDesc = prodhierarchy?.Find(y => y.SubClass == product?.SubClass)?.SubclassDescription;
                    poskutobeupdated.POProduct.RetailPrice = posku.GetRetailPrice();
                    //Update CreateDate value once it is available in POSkus object
                    poskutobeupdated.CreateDate = posku.CreateDate != null ? posku.CreateDate.Value : new DateTime?();
                    poskutobeupdated.ModifiedDate = posku.ModifiedDate != null ? posku.ModifiedDate.Value : new DateTime?();
                }
                else
                {
                    if ((product?.Department == "JEP" || product?.Department == "JSF") && product?.SubClass != "JOSAC")
                    {
                        POContempoProductOutput poProductOutput = new POContempoProductOutput
                        {
                            LabelType = product?.LabelType,
                            //TODO - need to figure out how we're getting these from MMS
                            //This LabelDescription value should be populated from "MMSProductEvent" ProductVendors objects labeldescription
                            //For now, it is coming from database so that we know the value is populating.
                            LabelDescription = productlabelcode?.Description,
                            Class = prodhierarchy?.Find(y => y.SubClass == product?.SubClass)?.Class,
                            CountryOfOrigin = posku.POProduct?.CountryOfOrigin,
                            SubClass = prodhierarchy.Find(y => y.SubClass == product?.SubClass)?.SubClass,
                            APVendor = posku.POProduct?.APVendor,
                            VendorSKUCode = product?.ProductVendors?.Find(y => y.Sku == product?.Sku && y.SubVendor == poobject.SubVendor)?.VendorSkuCode,
                            ClassLevelDesc = prodhierarchy?.Find(y => y.SubClass == product?.SubClass)?.Description,
                            Size = posku.POProduct?.Size,
                            SkuDesc = product?.SkuDescShrt,
                            SubClassLevelDesc = prodhierarchy?.Find(y => y.SubClass == product?.SubClass)?.SubclassDescription,
                            RetailPrice = posku.GetRetailPrice()
                        };
                        POContempoSkuOutput POSkusOutput = new POContempoSkuOutput
                        {
                            SKU = posku.SKU,
                            BuyQuanity = posku.BuyQuantity != null ? Convert.ToInt32(posku.BuyQuantity) : 0,
                            POProduct = poProductOutput,
                            //Update CreateDate value once it is available in POSkus object
                            CreateDate = posku.CreateDate != null ? posku.CreateDate.Value : new DateTime?(),
                            ModifiedDate = posku.ModifiedDate != null ? posku.ModifiedDate.Value : new DateTime?(),
                            StatusCode = posku.StatusCode,
                        };

                        if(poobject.POSkus == null)
                        {
                            poobject.POSkus = new System.Collections.Generic.List<POContempoSkuOutput>();
                        }
                        poobject.POSkus.Add(POSkusOutput);
                    }
                }
                return poobject;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdatePOObject - Failed Updating ContempoPO: {Reason}", ex.Message);
                return null;
            }            
        }
    }
}
