using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SG.MMS.Product.Retail.Events;
using SG.PO.Contempo.CommandService.Core.Services;
using SG.Shared.Api;
using System.Threading.Tasks;

namespace SG.PO.Contempo.CommandService.Controllers
{
    [Route("api/v1/POContempo")]
    public class POContempoController : Controller
    {
        private readonly ILogger _logger;
        private readonly POContempoService _pOContempoService;

        public POContempoController(ILogger<POContempoController> logger, POContempoService pOContempoService)
        {
            _logger = logger;
            _pOContempoService = pOContempoService;
        }

        [HttpPost, Route("cmd/create")]
        public async Task<IActionResult> POContempoCreated([FromBody] MMS.PO.Events.MMSPOCreatedEvent model)
        {
            _logger.LogDebug("POContempoCreated called- {PONumber}", model.PONumber);
            var results = await _pOContempoService.UpsertPOContempo(model);
            _logger.LogDebug("POContempoCreated returned- {PONumber}", model.PONumber);
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/update")]
        public async Task<IActionResult> POContempoUpdated([FromBody] MMS.PO.Events.MMSPOUpdatedEvent model)
        {
            _logger.LogDebug("POContempoUpdated called- {PONumber}", model.PONumber);
            var results = await _pOContempoService.UpsertPOContempo(model);
            _logger.LogDebug("POContempoUpdated returned- {PONumber}", model.PONumber);
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/posku/create")]
        public async Task<IActionResult> POContempoPoskuCreated([FromBody] MMS.PO.Events.MMSPOSkuCreatedEvent model)
        {
            _logger.LogDebug("POContempoPoskuCreated called- {PONumber}", model.PONumber);
            var results = await _pOContempoService.UpsertPOContempoPOSku(model);
            _logger.LogDebug("POContempoPoskuCreated returned- {PONumber}", model.PONumber);
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/posku/update")]
        public async Task<IActionResult> POContempoPoskuUpdated([FromBody] MMS.PO.Events.MMSPOSkuUpdatedEvent model)
        {
            _logger.LogDebug("POContempoPoskuUpdated called- {PONumber}", model.PONumber);
            var results = await _pOContempoService.UpsertPOContempoPOSku(model);
            _logger.LogDebug("POContempoPoskuUpdated returned- {PONumber}", model.PONumber);
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/product/create")]
        public async Task<IActionResult> POContempoProductCreated([FromBody] MMS.Product.Events.MMSProductCreatedEvent model)
        {
            _logger.LogDebug("POContempoProductCreated called- {Sku}", model.Sku);
            var results = await _pOContempoService.UpsertPOContempoProduct(model);
            _logger.LogDebug("POContempoProductCreated returned- {Sku}", model.Sku);
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/product/update")]
        public async Task<IActionResult> POContempoProductUpdated([FromBody] MMS.Product.Events.MMSProductUpdatedEvent model)
        {
            _logger.LogDebug("POContempoProductUpdated called- {Sku}", model.Sku);
            var results = await _pOContempoService.UpsertPOContempoProduct(model);
            _logger.LogDebug("POContempoProductUpdated returned- {Sku}", model.Sku);
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/lookup/create")]
        public async Task<IActionResult> POContempoLookUpCodeCreated([FromBody] MMS.LookupCode.Events.LookupCodeCreatedEvent model)
        {
            _logger.LogDebug("POContempoLookUpCodeCreated called- {Code}", model.Code);
            var results = await _pOContempoService.UpsertPOContempoLookUp(model);
            _logger.LogDebug("POContempoLookUpCodeCreated returned- {Code}", model.Code);
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/lookup/update")]
        public async Task<IActionResult> POContempoLookUpCodeUpdated([FromBody] MMS.LookupCode.Events.LookupCodeUpdatedEvent model)
        {
            _logger.LogDebug("POContempoLookUpCodeUpdated called- {Code}", model.Code);
            var results = await _pOContempoService.UpsertPOContempoLookUp(model);
            _logger.LogDebug("POContempoLookUpCodeUpdated returned- {Code}", model.Code);
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/productretail/create")]
        public async Task<IActionResult> POContempoProductRetailCreated([FromBody] MMSProductRetailCreatedEvent model)
        {
            _logger.LogDebug("POContempoProductRetailCreated called- {Sku}", model.Sku);
            var results = await _pOContempoService.UpsertPOContempoProductRetail(model);
            _logger.LogDebug("POContempoProductRetailCreated returned- {Sku}", model.Sku);
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/productretail/update")]
        public async Task<IActionResult> POContempoProductRetailUpdated([FromBody] MMSProductRetailUpdatedEvent model)
        {
            _logger.LogDebug("POContempoProductRetailUpdated called- {Sku}", model.Sku);
            var results = await _pOContempoService.UpsertPOContempoProductRetail(model);
            _logger.LogDebug("POContempoProductRetailUpdated returned- {Sku}", model.Sku);
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/forceinclude")]
        public async Task<IActionResult> POContempoForceInclude(int poNumber)
        {
            _logger.LogDebug("POContempoForceInclude called- {poNumber}", poNumber);
            var results = await _pOContempoService.ForceInclude(poNumber.ToString());
            _logger.LogDebug("POContempoForceInclude returned- {poNumber}", poNumber);
            return results.ProcessUpdateAction();
        }
    }
}
