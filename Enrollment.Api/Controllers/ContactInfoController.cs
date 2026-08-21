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
    public class ContactInfoController(IHttpClientHelper httpClientHelper, IOptions<UrlOptions> optionsAccessor) : ControllerBase
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

        [HttpPost("Delete")]
        public async Task<BaseResponse> Delete([FromBody] DeleteEntityRequest deleteCourseRequest)
            => await _httpClientHelper.PostAsync<BaseResponse>
            (
                $"{BaseUrl}api/ContactInfo/Delete",
                JsonSerializer.Serialize(deleteCourseRequest),
                SerializationOptions.Default,
                HttpClientOptions.BslClientName
            );

        [HttpPost("Save")]
        public async Task<BaseResponse> Save([FromBody] SaveEntityRequest saveCourseRequest)
            => await _httpClientHelper.PostAsync<BaseResponse>
            (
                $"{BaseUrl}api/ContactInfo/Save",
                JsonSerializer.Serialize(saveCourseRequest),
                SerializationOptions.Default,
                HttpClientOptions.BslClientName
            );
    }
}
