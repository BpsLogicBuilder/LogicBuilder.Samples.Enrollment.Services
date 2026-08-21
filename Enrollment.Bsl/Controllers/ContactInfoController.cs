using Enrollment.Bsl.Flow.Interfaces;
using LogicBuilder.App.Bsl.Business.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Enrollment.Bsl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactInfoController(IFlowManager flowManager) : ControllerBase
    {
        private readonly IFlowManager flowManager = flowManager;

        [HttpPost("Delete")]
        public IActionResult Delete([FromBody] DeleteEntityRequest deleteContactInfoRequest)
        {
            this.flowManager.FlowDataCache.Request = deleteContactInfoRequest;
            this.flowManager.Start("deletecontactInfo");
            return Ok(this.flowManager.FlowDataCache.Response);
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] SaveEntityRequest saveContactInfoRequest)
        {
            this.flowManager.FlowDataCache.Request = saveContactInfoRequest;
            this.flowManager.Start("savecontactInfo");
            return Ok(this.flowManager.FlowDataCache.Response);
        }
    }
}
