using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using SG.MMS.Product.Events;
using SG.PO.FineLine.DataModels;
using SG.Shared.POProduct.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using SG.PO.FineLine.CommandService.Core.Helper;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.FineLine.DataModels.Outputmodels;

namespace SG.PO.FineLine.CommandService.Core.Mapper
{
    public class POFineLineProductMapper
    {
        private static IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly ElasticClient _client;
        private readonly LookupDataService _lookUpService;

        public POFineLineProductMapper(IConfiguration configuration, ILogger<POFineLineProductMapper> logger, ElasticClient client,
            LookupDataService lookUpService)
        {
            _configuration = configuration;
            _logger = logger;
            _client = client;
            _lookUpService = lookUpService;
        }

        public async Task<POFineLineOutput> UpdatePOObject(POFineLineOutput poobject, MMSProductEvent product, POSkus posku)
        {
            try
            {
                var prodhierarchy = await _lookUpService.GetProductHierarchy(product.SubClass);
                var productlabel = await _lookUpService.GetProductLabelDescription(product.LabelType);
                //check if exists
                if (poobject.POSkus != null && poobject.POSkus.Count > 0 && poobject.POSkus.Exists(y => y.SKUNumber == posku.SKU))
                {
                    var poskutobeupdated = poobject.POSkus.Find(y => y.SKUNumber == posku.SKU);
                    poskutobeupdated.POProduct.SKUDescription = product?.SkuDescShrt;
                    poskutobeupdated.POProduct.SubClassID = product?.SubClass;
                    poskutobeupdated.POProduct.TicketType = productlabel?.Code;
                    poskutobeupdated.POProduct.TicketDescription = productlabel?.Description;
                    poskutobeupdated.POProduct.VendorNumber = posku.POProduct?.APVendor;
                    poskutobeupdated.POProduct.Size = product?.Size;
                    poskutobeupdated.POProduct.ISOCountryCode = product.ProductVendors?.Find(y => y.Sku == posku.SKU)?.CountryOfOrigin;
                    poskutobeupdated.POProduct.ClassID = prodhierarchy?.Find(y => y.SubClass == product?.SubClass)?.Class;
                    poskutobeupdated.POProduct.ClassDescription = prodhierarchy?.Find(y => y.SubClass == product?.SubClass)?.Description;
                    poskutobeupdated.POProduct.VendorStyleNumber = product.ProductVendors?.Find(y => y.Sku == posku.SKU)?.VendorSkuCode;
                    poskutobeupdated.POProduct.SubClassDescription = prodhierarchy?.Find(y => y.SubClass == product?.SubClass)?.SubclassDescription;
                    poskutobeupdated.POProduct.SubVendorNumber = product.ProductVendors?.Find(y => y.Sku == posku.SKU)?.SubVendor;
                    poskutobeupdated.POProduct.TicketRetail = posku.GetRetailPrice();
                    return poobject;
                }
                else
                {
                    POFineLineProductOutput poFLProductOutput = new POFineLineProductOutput
                    {
                        VendorNumber =posku.POProduct?.APVendor,
                        SubVendorNumber = product.ProductVendors?.Find(y => y.Sku == posku.SKU)?.SubVendor,
                        SKUDescription = product?.SkuDescShrt,
                        VendorStyleNumber = product.ProductVendors?.Find(y => y.Sku == posku.SKU)?.VendorSkuCode,
                        TicketType = productlabel?.Code,
                        TicketDescription = productlabel?.Description,
                        ClassID = prodhierarchy?.Find(y => y.SubClass == product?.SubClass)?.Class,
                        ClassDescription = prodhierarchy?.Find(y => y.SubClass == product?.SubClass)?.Description,
                        SubClassID = product?.SubClass,
                        SubClassDescription = prodhierarchy?.Find(y => y.SubClass == product?.SubClass)?.SubclassDescription,
                        Size = product?.Size,
                        ISOCountryCode = product.ProductVendors?.Find(y => y.Sku == posku.SKU)?.CountryOfOrigin,
                        TicketRetail = posku.GetRetailPrice()
                    };
                    POFineLineSkuOutput POFLSkusOutput = new POFineLineSkuOutput
                    {
                        SKUNumber = posku.SKU,
                        PurchaseOrderDate = posku.CreateDate != null ? posku.CreateDate.Value : new DateTime?(),
                        PurchaseOrderReviseDate = posku.ModifiedDate != null ? posku.ModifiedDate.Value : new DateTime?(),
                        OrderQuantity = posku.BuyQuantity != null ? Convert.ToInt32(posku.BuyQuantity) : 0,
                        POProduct = poFLProductOutput,
                        StatusCode = posku.StatusCode,
                    };

                    if (poobject.POSkus == null)
                    {
                        poobject.POSkus = new System.Collections.Generic.List<POFineLineSkuOutput>();
                    }
                    poobject.POSkus.Add(POFLSkusOutput);
                    return poobject;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdatePOObject - Failed Updating FineLinePO: {Reason} -- {PONumber} -- {Sku }", ex.Message, poobject.PurchaseOrder, posku.SKU);
                return null;
            }
        }
    }
}
