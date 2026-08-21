using LogicBuilder.App.Bsl.Business.Requests;
using LogicBuilder.App.Bsl.Business.Responses;
using LogicBuilder.App.Bsl.Utils.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Enrollment.Bsl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntityController(IRequestHelper requestHelper) : ControllerBase
    {
        private readonly IRequestHelper _requestHelper = requestHelper;

        [HttpPost("GetEntity")]
        public async Task<BaseResponse> GetEntity([FromBody] GetEntityRequest request)
        {
            return await _requestHelper.GetEntity
            (
                request
            );
        }
    }
}
