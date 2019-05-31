using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SG.PO.Intertek.CommandService.Core.Services;
using SG.Shared.Api;
using System.Threading.Tasks;

namespace SG.PO.Intertek.CommandService.Controllers
{
    [Route("api/v1/poIntertek")]
    public class POIntertekController : Controller
    {
        private readonly ILogger _logger;        
        private readonly POIntertekService _POIntertekService;

        public POIntertekController(ILogger<POIntertekController> logger, POIntertekService poIntertekService)
        {
            _logger = logger;            
            _POIntertekService = poIntertekService;
        }

        [HttpPost, Route("cmd/create")]
        public async Task<IActionResult> POIntertekCreated([FromBody] MMS.PO.Events.MMSPOCreatedEvent model)
        {
            _logger.LogDebug("POIntertekCreated called");
            var results = await _POIntertekService.UpsertPOIntertek(model);

            _logger.LogDebug("POIntertekCreated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/update")]
        public async Task<IActionResult> POIntertekUpdated([FromBody] MMS.PO.Events.MMSPOUpdatedEvent model)
        {
            _logger.LogDebug("POIntertekUpdated called");
            var results = await _POIntertekService.UpsertPOIntertek(model);

            _logger.LogDebug("POIntertekUpdated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/posku/update")]
        public async Task<IActionResult> POIntertekPoskuUpdated([FromBody] MMS.PO.Events.MMSPOSkuUpdatedEvent model)
        {
            _logger.LogDebug("POIntertekPoskuUpdated called");
            var results = await _POIntertekService.UpsertPOIntertekPOSku(model);

            _logger.LogDebug("POIntertekPoskuUpdated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/posku/create")]
        public async Task<IActionResult> POIntertekPoskuCreated([FromBody] MMS.PO.Events.MMSPOSkuCreatedEvent model)
        {
            _logger.LogDebug("POIntertekPoskuCreated called");
            var results = await _POIntertekService.UpsertPOIntertekPOSku(model);

            _logger.LogDebug("POIntertekPoskuCreated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/product/update")]
        public async Task<IActionResult> POIntertekProductUpdated([FromBody] MMS.Product.Events.MMSProductUpdatedEvent model)
        {
            _logger.LogDebug("POIntertekProductUpdated called");
            var results = await _POIntertekService.UpsertPOIntertekProduct(model);

            _logger.LogDebug("POIntertekProductUpdated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/product/create")]
        public async Task<IActionResult> POIntertekProductCreated([FromBody] MMS.Product.Events.MMSProductCreatedEvent model)
        {
            _logger.LogDebug("POIntertekProductCreated called");
            var results = await _POIntertekService.UpsertPOIntertekProduct(model);

            _logger.LogDebug("POIntertekProductCreated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/vendor/create")]
        public async Task<IActionResult> POIntertekVendorCreated([FromBody] Vendor.MMS.Events.MMSSubVendorCreatedEvent model)
        {
            _logger.LogDebug("POIntertekVendorCreated called");
            var results = await _POIntertekService.UpsertPOIntertekVendor(model);

            _logger.LogDebug("POIntertekVendorCreated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/vendor/update")]
        public async Task<IActionResult> POIntertekVendorUpdated([FromBody] Vendor.MMS.Events.MMSSubVendorUpdatedEvent model)
        {
            _logger.LogDebug("POIntertekVendorUpdated called");
            var results = await _POIntertekService.UpsertPOIntertekVendor(model);

            _logger.LogDebug("POIntertekVendorUpdated returned");
            return results.ProcessUpdateAction();
        }
        

        [HttpPost, Route("cmd/forceinclude")]
        public async Task<IActionResult> POIntertekForceInclude(int poNumber)
        {
            _logger.LogDebug("forceinclude called");
            var results = await _POIntertekService.ForceInclude(poNumber.ToString());
            _logger.LogDebug("forceinclude returned");
            return results.ProcessUpdateAction();

        }
    }
}
