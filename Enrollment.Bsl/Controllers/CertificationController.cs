using Enrollment.Bsl.Flow.Interfaces;
using LogicBuilder.App.Bsl.Business.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Enrollment.Bsl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificationController(IFlowManager flowManager) : ControllerBase
    {
        private readonly IFlowManager flowManager = flowManager;

        [HttpPost("Delete")]
        public IActionResult Delete([FromBody] DeleteEntityRequest deleteCertificationRequest)
        {
            this.flowManager.FlowDataCache.Request = deleteCertificationRequest;
            this.flowManager.Start("deletecertification");
            return Ok(this.flowManager.FlowDataCache.Response);
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] SaveEntityRequest saveCertificationRequest)
        {
            this.flowManager.FlowDataCache.Request = saveCertificationRequest;
            this.flowManager.Start("savecertification");
            return Ok(this.flowManager.FlowDataCache.Response);
        }
    }
}
