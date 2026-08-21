using Enrollment.Bsl.Flow.Interfaces;
using LogicBuilder.App.Bsl.Business.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Enrollment.Bsl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResidencyController(IFlowManager flowManager) : ControllerBase
    {
        private readonly IFlowManager flowManager = flowManager;

        [HttpPost("Delete")]
        public IActionResult Delete([FromBody] DeleteEntityRequest deleteResidencyRequest)
        {
            this.flowManager.FlowDataCache.Request = deleteResidencyRequest;
            this.flowManager.Start("deleteresidency");
            return Ok(this.flowManager.FlowDataCache.Response);
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] SaveEntityRequest saveResidencyRequest)
        {
            this.flowManager.FlowDataCache.Request = saveResidencyRequest;
            this.flowManager.Start("saveresidency");
            return Ok(this.flowManager.FlowDataCache.Response);
        }
    }
}
