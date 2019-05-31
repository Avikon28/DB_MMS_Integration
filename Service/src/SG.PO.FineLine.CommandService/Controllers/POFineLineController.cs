using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SG.MMS.PO.Events;
using SG.PO.FineLine.CommandService.Core.Services;
using SG.Shared.Api;
using System.Threading.Tasks;

namespace SG.PO.FineLine.CommandService.Controllers
{
    [Route("api/v1/pofineline")]
    public class POFineLineController : Controller
    {
        private readonly ILogger _logger;
        private readonly POFineLineService _POFineLineService;

        public POFineLineController(ILogger<POFineLineController> logger, POFineLineService pOFineLineService)
        {
            _logger = logger;
            _POFineLineService = pOFineLineService;
        }

        [HttpPost, Route("cmd/create")]
        public async Task<IActionResult> POFineLineCreated([FromBody] MMSPOCreatedEvent model)
        {
            _logger.LogDebug("POFineLineCreated called");
            var results = await _POFineLineService.UpsertPOFineLine(model);

            _logger.LogDebug("POFineLineCreated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/update")]
        public async Task<IActionResult> POFineLineUpdated([FromBody] MMSPOUpdatedEvent model)
        {
            _logger.LogDebug("POFineLineUpdated called");

            var results = await _POFineLineService.UpsertPOFineLine(model);
            _logger.LogDebug("POFineLineUpdated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/posku/update")]
        public async Task<IActionResult> POFineLinePoskuUpdated([FromBody] MMS.PO.Events.MMSPOSkuUpdatedEvent model)
        {
            _logger.LogDebug("POFineLinePoskuUpdated called");

            var results = await _POFineLineService.UpsertPOFineLinePOSku(model);
            _logger.LogDebug("POFineLinePoskuUpdated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/posku/create")]
        public async Task<IActionResult> POFineLinePoskuCreated([FromBody] MMS.PO.Events.MMSPOSkuCreatedEvent model)
        {
            _logger.LogDebug("POFineLinePoskuCreated called");
            var results = await _POFineLineService.UpsertPOFineLinePOSku(model);
            _logger.LogDebug("POFineLinePoskuCreated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/product/update")]
        public async Task<IActionResult> POFineLineProductUpdated([FromBody] MMS.Product.Events.MMSProductUpdatedEvent model)
        {
            _logger.LogDebug("POFineLineProductUpdated called");
            var results = await _POFineLineService.UpsertPOFineLineProduct(model);
            _logger.LogDebug("POFineLineProductUpdated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/product/create")]
        public async Task<IActionResult> POFineLineProductCreated([FromBody] MMS.Product.Events.MMSProductCreatedEvent model)
        {
            _logger.LogDebug("POFineLineProductCreated called");
            var results = await _POFineLineService.UpsertPOFineLineProduct(model);
            _logger.LogDebug("POFineLineProductCreated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/lookuocode/create")]
        public async Task<IActionResult> POFineLineLookupCodeCreated([FromBody] SG.MMS.LookupCode.Events.LookupCodeCreatedEvent model)
        {
            _logger.LogDebug("POFineLineLookupCodeCreated called");
            var results =  await _POFineLineService.UpsertPOFineLineLookupCode(model);
            _logger.LogDebug("POFineLineLookupCodeCreated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/lookuocode/update")]
        public async Task<IActionResult> POFineLineLookupCodeUpdated([FromBody]SG.MMS.LookupCode.Events.LookupCodeUpdatedEvent model)
        {
            _logger.LogDebug("POFineLineLookupCodeUpdated called");
            var results = await _POFineLineService.UpsertPOFineLineLookupCode(model);
            _logger.LogDebug("POFineLineLookupCodeUpdated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/productretail/create")]
        public async Task<IActionResult> POFineLineProductRetailCreated([FromBody] SG.MMS.Product.Retail.Events.MMSProductRetailCreatedEvent model)
        {
            _logger.LogDebug("POFineLineProductRetailCreated called");
            var results = await _POFineLineService.UpsertPOFineLineProductRetail(model);
            _logger.LogDebug("POFineLineProductRetailCreated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/productretail/update")]
        public async Task<IActionResult> POFineLineProductRetailUpdated([FromBody]SG.MMS.Product.Retail.Events.MMSProductRetailUpdatedEvent model)
        {
            _logger.LogDebug("POFineLineProductRetailUpdated called");
            var results = await _POFineLineService.UpsertPOFineLineProductRetail(model);
            _logger.LogDebug("POFineLineProductRetailUpdated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/forceinclude")]
        public async Task<IActionResult> POFineLineForceInclude(int poNumber)
        {
            _logger.LogDebug("ForceInclude called");
            var results = await _POFineLineService.ForceInclude(poNumber.ToString());
            _logger.LogDebug("ForceInclude returned");
            return results.ProcessUpdateAction();

        }
    }
}
