using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SG.PO.Chino.CommandService.Core.Services;
using SG.Shared.Api;
using System.Threading.Tasks;

namespace SG.PO.Chino.CommandService.Controllers
{
    [Route("api/v1/poChino")]
    public class POChinoController : Controller
    {
        private readonly ILogger _logger;
        private readonly POChinoService _POChinoService;

        public POChinoController(ILogger<POChinoController> logger, POChinoService poChinoService)
        {
            _logger = logger;
            _POChinoService = poChinoService;
        }

        [HttpPost, Route("cmd/create")]
        public async Task<IActionResult> POChinoCreated([FromBody] MMS.PO.Events.MMSPOCreatedEvent model)
        {
            _logger.LogDebug("POChinoCreated called");
            var results = await _POChinoService.UpsertPOChino(model);

            _logger.LogDebug("POChinoCreated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/update")]
        public async Task<IActionResult> POChinoUpdated([FromBody] MMS.PO.Events.MMSPOUpdatedEvent model)
        {
            _logger.LogDebug("POChinoUpdated called");
            var results = await _POChinoService.UpsertPOChino(model);

            _logger.LogDebug("POChinoUpdated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/posku/update")]
        public async Task<IActionResult> POChinoPoskuUpdated([FromBody] MMS.PO.Events.MMSPOSkuUpdatedEvent model)
        {
            _logger.LogDebug("POChinoPoskuUpdated called");

            var results = await _POChinoService.UpsertPOChinoPOSku(model);
            _logger.LogDebug("POChinoPoskuUpdated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/posku/create")]
        public async Task<IActionResult> POChinoPoskuCreated([FromBody] MMS.PO.Events.MMSPOSkuCreatedEvent model)
        {
            _logger.LogDebug("POChinoPoskuCreated called");
            var results = await _POChinoService.UpsertPOChinoPOSku(model);
            _logger.LogDebug("POChinoPoskuCreated returned");
            return results.ProcessUpdateAction();
        }

        [HttpPost, Route("cmd/forceinlcude")]
        public async Task<IActionResult> POChinoForceInclude(int poNumber)
        {
            _logger.LogDebug("forceinlcude called");
            var results = await _POChinoService.ForceInclude(poNumber.ToString());
            _logger.LogDebug("forceinlcude returned");
            return results.ProcessUpdateAction();

        }

    }
}
