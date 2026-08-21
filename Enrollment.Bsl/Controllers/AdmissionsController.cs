using Enrollment.Bsl.Flow.Interfaces;
using LogicBuilder.App.Bsl.Business.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Enrollment.Bsl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdmissionsController(IFlowManager flowManager) : ControllerBase
    {
        private readonly IFlowManager flowManager = flowManager;

        [HttpPost("Delete")]
        public IActionResult Delete([FromBody] DeleteEntityRequest deleteAdmissionsRequest)
        {
            this.flowManager.FlowDataCache.Request = deleteAdmissionsRequest;
            this.flowManager.Start("deleteadmissions");
            return Ok(this.flowManager.FlowDataCache.Response);
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] SaveEntityRequest saveAdmissionsRequest)
        {
            this.flowManager.FlowDataCache.Request = saveAdmissionsRequest;
            this.flowManager.Start("saveadmissions");
            return Ok(this.flowManager.FlowDataCache.Response);
        }
    }
}
