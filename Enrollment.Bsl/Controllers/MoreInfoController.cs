using Enrollment.Bsl.Flow.Interfaces;
using LogicBuilder.App.Bsl.Business.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Enrollment.Bsl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoreInfoController(IFlowManager flowManager) : ControllerBase
    {
        private readonly IFlowManager flowManager = flowManager;

        [HttpPost("Delete")]
        public IActionResult Delete([FromBody] DeleteEntityRequest deleteMoreInfoRequest)
        {
            this.flowManager.FlowDataCache.Request = deleteMoreInfoRequest;
            this.flowManager.Start("deletemoreInfo");
            return Ok(this.flowManager.FlowDataCache.Response);
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] SaveEntityRequest saveMoreInfoRequest)
        {
            this.flowManager.FlowDataCache.Request = saveMoreInfoRequest;
            this.flowManager.Start("savemoreInfo");
            return Ok(this.flowManager.FlowDataCache.Response);
        }
    }
}
