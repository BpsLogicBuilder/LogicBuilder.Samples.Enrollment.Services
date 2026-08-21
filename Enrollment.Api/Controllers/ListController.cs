using LogicBuilder.App.Bsl.Business.Requests;
using LogicBuilder.App.Bsl.Business.Responses;
using LogicBuilder.App.Utils.Web.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Threading.Tasks;

namespace Enrollment.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ListController(IHttpClientHelper httpClientHelper, IOptions<UrlOptions> optionsAccessor) : ControllerBase
    {
        private readonly IHttpClientHelper _httpClientHelper = httpClientHelper;
        private readonly UrlOptions urlOptions = optionsAccessor.Value;

        private string BaseUrl
        {
            get
            {
                string url = urlOptions.BaseBslUrl;
                return url.EndsWith('/') ? url : $"{url}/";
            }
        }

        [HttpPost("GetList")]
        public Task<BaseResponse> GetList([FromBody] GetTypedListRequest request)
            => _httpClientHelper.PostAsync<BaseResponse>
            (
                $"{BaseUrl}api/List/GetList",
                JsonSerializer.Serialize(request),
                SerializationOptions.Default,
                HttpClientOptions.BslClientName
            );
    }
}
