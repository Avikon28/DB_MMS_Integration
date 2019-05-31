using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SG.PO.APLL.CommandService.Core.Services;
using SG.Shared.Api;
using System.Threading.Tasks;

namespace SG.PO.APLL.CommandService.Controllers
{
    [Route("api/v1/poapl")]
    public class POAPLController : Controller
    {
        private readonly ILogger _logger;
        private readonly POAPLService _POAPLService;

        public POAPLController(ILogger<POAPLController> logger, POAPLService pOAPLService)
        {
            _logger = logger;
            _POAPLService = pOAPLService;
        }

        [HttpPost, Route("cmd/create")]
        public async Task<IActionResult> POAPLCreated([FromBody] MMS.PO.Events.MMSPOCreatedEvent model)
        {
            _logger.LogDebug("POAPLCreated called");
            var results = await _POAPLService.UpsertPOAPL(model);

            _logger.LogDebug("POAPLCreated returned");
            return results.ProcessUpdateAction();
             
        }

        [HttpPost, Route("cmd/update")]
        public async Task<IActionResult> POAPLUpdated([FromBody] MMS.PO.Events.MMSPOUpdatedEvent model)
        {
            _logger.LogDebug("POAPLUpdated called");

            var results = await _POAPLService.UpsertPOAPL(model);
            _logger.LogDebug("POAPLUpdated returned");
            return results.ProcessUpdateAction();
 
        }

        [HttpPost, Route("cmd/posku/update")]
        public async Task<IActionResult> POAPLPoskuUpdated([FromBody] MMS.PO.Events.MMSPOSkuUpdatedEvent model)
        {
            _logger.LogDebug("POAPLPoskuUpdated called");

            var results = await _POAPLService.UpsertPOAPLPOSku(model);
            _logger.LogDebug("POAPLPoskuUpdated returned");
            return results.ProcessUpdateAction();
           

        }

        [HttpPost, Route("cmd/posku/create")]
        public async Task<IActionResult> POAPLPoskuCreated([FromBody] MMS.PO.Events.MMSPOSkuCreatedEvent model)
        {
            _logger.LogDebug("POAPLPoskuCreated called");
            var results = await _POAPLService.UpsertPOAPLPOSku(model);
            _logger.LogDebug("POAPLPoskuCreated returned");
            return results.ProcessUpdateAction();
           

        }

        [HttpPost, Route("cmd/product/update")]
        public async Task<IActionResult> POAPLProductUpdated([FromBody] MMS.Product.Events.MMSProductUpdatedEvent model)
        {
            _logger.LogDebug("POAPLProductUpdated called");
            var results = await _POAPLService.UpsertPOAPLProduct(model);
            _logger.LogDebug("POAPLProductUpdated returned");
            return results.ProcessUpdateAction();

        }

        [HttpPost, Route("cmd/product/create")]
        public async Task<IActionResult> POAPLProductCreated([FromBody] MMS.Product.Events.MMSProductCreatedEvent model)
        {
            _logger.LogDebug("POAPLProductCreated called");
            var results = await _POAPLService.UpsertPOAPLProduct(model);
            _logger.LogDebug("POAPLProductCreated returned");
            return results.ProcessUpdateAction();

        }

        [HttpPost, Route("cmd/vendor/create")]
        public async Task<IActionResult> POAPLVendorCreated([FromBody] Vendor.MMS.Events.MMSSubVendorCreatedEvent model)
        {
            _logger.LogDebug("POAPLVendorCreated called");
            var results = await _POAPLService.UpsertPOAPLVendor(model);
            _logger.LogDebug("POAPLVendorCreated returned");
            return results.ProcessUpdateAction();

        }

        [HttpPost, Route("cmd/vendor/update")]
        public async Task<IActionResult> POAPLVendorUpdated([FromBody] Vendor.MMS.Events.MMSSubVendorUpdatedEvent model)
        {
            _logger.LogDebug("POAPLVendorUpdated called");
            var results = await _POAPLService.UpsertPOAPLVendor(model);
            _logger.LogDebug("POAPLVendorUpdated returned");
            return results.ProcessUpdateAction();

        }

        [HttpPost, Route("cmd/forceinlcude")]
        public async Task<IActionResult> POAPLForceInclude(int poNumber)
        {
            _logger.LogDebug("forceinlcude called");
            var results = await _POAPLService.ForceInclude(poNumber.ToString());
            _logger.LogDebug("forceinlcude returned");
            return results.ProcessUpdateAction();

        }


    }
}
