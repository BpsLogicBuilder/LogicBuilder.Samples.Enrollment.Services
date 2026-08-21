using Enrollment.Bsl.Flow.Interfaces;
using LogicBuilder.App.Bsl.Business.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Enrollment.Bsl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IFlowManager flowManager) : ControllerBase
    {
        private readonly IFlowManager flowManager = flowManager;

        [HttpPost("Delete")]
        public IActionResult Delete([FromBody] DeleteEntityRequest deleteUserRequest)
        {
            this.flowManager.FlowDataCache.Request = deleteUserRequest;
            this.flowManager.Start("deleteuser");
            return Ok(this.flowManager.FlowDataCache.Response);
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] SaveEntityRequest saveUserRequest)
        {
            this.flowManager.FlowDataCache.Request = saveUserRequest;
            this.flowManager.Start("saveuser");
            return Ok(this.flowManager.FlowDataCache.Response);
        }
    }
}
