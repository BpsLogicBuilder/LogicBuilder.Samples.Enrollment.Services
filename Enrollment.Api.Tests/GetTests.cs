using Enrollment.Data.Entities;
using Enrollment.Domain.Entities;
using LogicBuilder.App.Bsl.Business.Requests;
using LogicBuilder.App.Bsl.Business.Responses;
using LogicBuilder.App.Utils.Web;
using LogicBuilder.App.Utils.Web.Interfaces;
using LogicBuilder.Expressions.Utils.ExpressionDescriptors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Enrollment.Api.Tests
{
    public class GetTests
    {
        public GetTests()
        {
            Initialize();
        }

        #region Fields
        private IServiceProvider serviceProvider;
        #endregion Fields

        #region Properties
        private string BaseUrl
        {
            get
            {
                IOptions<UrlOptions> options = serviceProvider.GetRequiredService<IOptions<UrlOptions>>();
                string url = options.Value.BaseBslUrl;
                return url.EndsWith('/') ? url : $"{url}/";
            }
        }
        #endregion Properties

        [Fact]
        public async Task GetDropDownListRequest_As_AnonymousTypes()
        {
            //arrange
            IHttpClientHelper helper = serviceProvider.GetRequiredService<IHttpClientHelper>();
            var selectorLambdaDescriptor = GetExpressionDescriptor<IQueryable<LookUpsModel>, IEnumerable<object>>
            (
                GetBodyForLookupsModelAsAnonymousTypes(),
                "q"
            );

            //act
            var result = await helper.PostAsync<GetObjectListResponse>
            (
                $"{BaseUrl}api/AnonymousTypeList/GetList",
                JsonSerializer.Serialize
                (
                    new GetObjectListRequest
                    {
                        Selector = selectorLambdaDescriptor,
                        ModelType = typeof(LookUpsModel).AssemblyQualifiedName,
                        DataType = typeof(LookUps).AssemblyQualifiedName
                    }
                ),
                SerializationOptions.Default
            );

            //assert
            Assert.True(result.List.Any());
        }

        #region Helpers
        private static SelectDescriptor GetBodyForLookupsModelAsAnonymousTypes()
            => new
            (
                new OrderByDescriptor
                (
                    new WhereDescriptor
                    (
                        new ParameterDescriptor("q"),
                        new EqualsBinaryDescriptor
                        (
                            new MemberSelectorDescriptor
                            (
                                "ListName",
                                new ParameterDescriptor("l")
                            ),
                            new ConstantDescriptor
                            (
                                "militaryBranch",
                                typeof(string).AssemblyQualifiedName
                            )
                        ),
                        "l"
                    ),
                    new MemberSelectorDescriptor
                    (
                        "Text",
                        new ParameterDescriptor("l")
                    ),
                    LogicBuilder.Expressions.Utils.Strutures.ListSortDirection.Descending,
                    "l"
                ),
                new MemberInitDescriptor
                (
                    new Dictionary<string, DescriptorBase>
                    {
                        ["Value"] = new MemberSelectorDescriptor
                        (
                            "Value",
                            new ParameterDescriptor("l")
                        ),
                        ["Text"] = new MemberSelectorDescriptor
                        (
                            "Text",
                            new ParameterDescriptor("l")
                        )
                    }
                ),
                "l"
            );

        private static SelectorLambdaDescriptor GetExpressionDescriptor<T, TResult>(DescriptorBase selectorBody, string parameterName = "$it")
            => new
            (
                selectorBody,
                typeof(T).AssemblyQualifiedName!,
                parameterName,
                typeof(TResult).AssemblyQualifiedName
            );

        private static FilterLambdaDescriptor GetFilterExpressionDescriptor<T>(DescriptorBase filterBody, string parameterName = "$it")
            => new
            (
                filterBody,
                typeof(T).AssemblyQualifiedName!,
                parameterName
            );

        [MemberNotNull(nameof(serviceProvider))]
        private void Initialize()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            IServiceCollection services = new ServiceCollection();
            services.AddHttpClient();
            services.AddTransient<IHttpClientHelper, HttpClientHelper>();
            services.Configure<UrlOptions>(configuration);
            serviceProvider = services.BuildServiceProvider();

        }
        #endregion Helpers
    }
}
