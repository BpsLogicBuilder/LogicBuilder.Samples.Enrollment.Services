using Enrollment.Bsl.Flow.Interfaces;
using LogicBuilder.App.Bsl.Business.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Enrollment.Bsl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcademicController(IFlowManager flowManager) : ControllerBase
    {
        private readonly IFlowManager flowManager = flowManager;

        [HttpPost("Delete")]
        public IActionResult Delete([FromBody] DeleteEntityRequest deleteAcademicRequest)
        {
            this.flowManager.FlowDataCache.Request = deleteAcademicRequest;
            this.flowManager.Start("deleteacademic");
            return Ok(this.flowManager.FlowDataCache.Response);
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] SaveEntityRequest saveAcademicRequest)
        {
            this.flowManager.FlowDataCache.Request = saveAcademicRequest;
            this.flowManager.Start("saveacademic");
            return Ok(this.flowManager.FlowDataCache.Response);
        }
    }
}
