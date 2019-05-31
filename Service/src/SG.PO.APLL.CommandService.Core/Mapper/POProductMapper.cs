using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using SG.MMS.Product.Events;
using SG.MMS.QueryService.ODATA.Models.PO;
using SG.PO.APLL.DataModel.Outputmodels;
using SG.Shared.POProduct.Services;
using SG.Vendor.MMS.Events;
using System;

namespace SG.Shared.POProduct.Mapper
{
    public class POProductMapper
    {
        private static IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly ElasticClient _client;
        private readonly LookupDataService _lookUpService;
        

        public POProductMapper(IConfiguration configuration, ILogger<POProductMapper> logger, ElasticClient client,
            LookupDataService lookUpService)
        {
            _configuration = configuration;
            _logger = logger;
            _client = client;
            _lookUpService = lookUpService;
        }


        public async void UpdatePOObject(POAPLLOutput poobject, MMSProductEvent product, POSkus posku)
        {
            try
            {
                var prodhierarchy = _lookUpService.GetProductHierarchy(product.SubClass).Result;

                //check if exists
                if (poobject.POSkus != null && poobject.POSkus.Count > 0 && poobject.POSkus.Exists(y => y.POProduct.Sku == posku.SKU))
                {
                    var poskutobeupdated = poobject.POSkus.Find(y => y.POProduct.Sku == posku.SKU);

                    poskutobeupdated.POProduct.CountryOfOrigin = product.ProductVendors.Find(y => y.Sku == product.Sku && y.SubVendor == poobject.SubVendorNumber)?.CountryOfOrigin;
                    poskutobeupdated.POProduct.CasePackQty = product.ProductVendors.Find(y => y.Sku == product.Sku && y.SubVendor == poobject.SubVendorNumber)?.MasterPackQuantity.ToString();
                    poskutobeupdated.POProduct.VendorName = product.ProductVendors.Find(y => y.Sku == product.Sku && y.SubVendor == poobject.SubVendorNumber)?.SubVendor;

                    poskutobeupdated.POProduct.ClassCode = prodhierarchy.Find(y => y.SubClass == product.SubClass)?.Class;
                    poskutobeupdated.POProduct.DepartmentName = prodhierarchy.Find(y => y.SubClass == product.SubClass)?.Description;
                    poskutobeupdated.POProduct.ClassName = prodhierarchy.Find(y => y.SubClass == product.SubClass)?.Description;
                    poskutobeupdated.POProduct.DepartmentCode = prodhierarchy.Find(y => y.SubClass == product.SubClass)?.Department;

                    poskutobeupdated.POProduct.ItemDescription = product?.SkuDesc;
                    poskutobeupdated.POProduct.TariffCode = product?.HTSCode;
                    // poskutobeupdated.ActivityCode = "U";
                }
                else
                {
                    POProductOutput poProductOutput = new POProductOutput
                    {
                        CountryOfOrigin = product.ProductVendors.Find(y => y.Sku == product.Sku && y.SubVendor == poobject.SubVendorNumber)?.CountryOfOrigin,
                        CasePackQty = product.ProductVendors.Find(y => y.Sku == product.Sku && y.SubVendor == poobject.SubVendorNumber)?.MasterPackQuantity.ToString(),
                        VendorName = product.ProductVendors.Find(y => y.Sku == product.Sku && y.SubVendor == poobject.SubVendorNumber)?.SubVendor,

                        ClassCode = prodhierarchy.Find(y => y.SubClass == product.SubClass)?.Class,
                        DepartmentName = prodhierarchy.Find(y => y.SubClass == product.SubClass)?.Description,
                        ClassName = prodhierarchy.Find(y => y.SubClass == product.SubClass)?.Description,
                        DepartmentCode = prodhierarchy.Find(y => y.SubClass == product.SubClass)?.Department,

                        ItemDescription = product?.SkuDesc,

                        TariffCode = product?.HTSCode,
                    };
                    POSkusOutput POSkusOutput = new POSkusOutput
                    {
                        PONumber = posku.PONumber.ToString(),
                        ItemNumber = posku.SKU,
                        ItemQty = posku.BuyQuantity.HasValue ? posku.BuyQuantity.ToString() : "0",
                        ItemTotalQuantity = posku.BuyQuantity.HasValue ? posku.BuyQuantity.ToString() : "0",
                        UnitCost = Math.Round(posku.FirstCost.GetValueOrDefault(), 2).ToString(),
                        POProduct = poProductOutput,
                        //ActivityCode = "A",
                    };
                    if (poobject.POSkus == null)
                    {
                        poobject.POSkus = new System.Collections.Generic.List<POSkusOutput>();
                    }
                    poobject.POSkus.Add(POSkusOutput);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdatePOObject - Failed Updating APLPO: {Reason}", ex.Message);
            }
            
        }

        public async void UpdatePOObjectForVendor(POAPLLOutput poobject, MMSSubVendorEvent vendor)
        {
            try
            {
                poobject.VendName = vendor.VendName;
                //poobject.ActivityCode = "U";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdatePOObjectForVendor - Failed Updating APLPO: {Reason}", ex.Message);
            }
        }
    }
}
