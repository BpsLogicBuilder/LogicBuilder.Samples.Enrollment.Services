using LogicBuilder.App.Utils.Json;
using LogicBuilder.Domain.Json;
using LogicBuilder.Expressions.Utils.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Enrollment.Api.Tests
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
                options.Converters.Add(new ObjectConverter());

                _default = options;

                return _default;
            }
        }
    }
}
