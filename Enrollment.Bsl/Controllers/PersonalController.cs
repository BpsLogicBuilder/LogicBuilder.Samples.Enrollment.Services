using Enrollment.Bsl.Flow.Interfaces;
using LogicBuilder.App.Bsl.Business.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Enrollment.Bsl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonalController(IFlowManager flowManager) : ControllerBase
    {
        private readonly IFlowManager flowManager = flowManager;

        [HttpPost("Delete")]
        public IActionResult Delete([FromBody] DeleteEntityRequest deletePersonalRequest)
        {
            this.flowManager.FlowDataCache.Request = deletePersonalRequest;
            this.flowManager.Start("deletepersonal");
            return Ok(this.flowManager.FlowDataCache.Response);
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] SaveEntityRequest savePersonalRequest)
        {
            this.flowManager.FlowDataCache.Request = savePersonalRequest;
            this.flowManager.Start("savepersonal");
            return Ok(this.flowManager.FlowDataCache.Response);
        }
    }
}
