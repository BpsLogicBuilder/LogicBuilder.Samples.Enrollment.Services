using LogicBuilder.App.Bsl.Business.Requests;
using LogicBuilder.App.Bsl.Business.Responses;
using LogicBuilder.App.Bsl.Utils.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Enrollment.Bsl.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListController(IRequestHelper requestHelper) : ControllerBase
    {
        private readonly IRequestHelper _requestHelper = requestHelper;

        [HttpPost("GetList")]
        public async Task<BaseResponse> GetList([FromBody] GetTypedListRequest request)
        {
            return await _requestHelper.GetList
            (
                request
            );
        }
    }
}
