using Enrollment.Domain.Entities;
using LogicBuilder.App.Bsl.Business.Responses;
using LogicBuilder.App.Utils.Json;
using LogicBuilder.Domain.Json;
using LogicBuilder.Expressions.Utils.Json;
using System.Text.Json;

namespace Enrollment.Api
{
    public static class SerializationOptions
    {
        private static JsonSerializerOptions? _default;
        public static JsonSerializerOptions Default
        {
            get
            {
                if (_default != null)
                    return _default;

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                options.Converters.Add(new DescriptorConverter());
                options.Converters.Add(new ModelConverter());
                options.Converters.Add(new ObjectConverter(typeof(WeatherForecast).Assembly, typeof(DescriptorConverter).Assembly, typeof(UserModel).Assembly, typeof(GetListResponse).Assembly));

                _default = options;

                return _default;
            }
        }
    }
}
