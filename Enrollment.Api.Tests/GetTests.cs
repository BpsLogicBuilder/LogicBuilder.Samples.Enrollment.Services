using Enrollment.Data.Entities;
using Enrollment.Domain.Entities;
using LogicBuilder.App.Bsl.Business.Requests;
using LogicBuilder.App.Bsl.Business.Responses;
using LogicBuilder.App.Utils.Web.Interfaces;
using LogicBuilder.Expressions.Utils.ExpansionDescriptors;
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

        [Fact]
        public async Task GetDropDownListRequest_As_LookUpsModel()
        {
            //arrange
            IHttpClientHelper helper = serviceProvider.GetRequiredService<IHttpClientHelper>();
            var selectorLambdaOperatorDescriptor = GetExpressionDescriptor<IQueryable<LookUpsModel>, IEnumerable<LookUpsModel>>
            (
                GetBodyForLookupsModel(),
                "q"
            );

            var result = await helper.PostAsync<GetListResponse>
            (
                $"{BaseUrl}api/List/GetList",
                JsonSerializer.Serialize
                (
                    new GetTypedListRequest
                    {
                        Selector = selectorLambdaOperatorDescriptor,
                        ModelType = typeof(LookUpsModel).AssemblyQualifiedName,
                        DataType = typeof(LookUps).AssemblyQualifiedName,
                        ModelReturnType = typeof(IEnumerable<LookUpsModel>).AssemblyQualifiedName,
                        DataReturnType = typeof(IEnumerable<LookUps>).AssemblyQualifiedName
                    }
                ),
                SerializationOptions.Default
            );

            Assert.True(result.List.Any());
        }

        [Fact]
        public async Task GetDropDownListRequest_As_SatesLiveInModel()
        {
            //arrange
            IHttpClientHelper helper = serviceProvider.GetRequiredService<IHttpClientHelper>();
            var selectorLambdaOperatorDescriptor = GetExpressionDescriptor<IQueryable<LookUpsModel>, IEnumerable<StateLivedInModel>>
            (
                GetBodyConvertLookupsModelToStatesLivedInModel(),
                "q"
            );

            var result = await helper.PostAsync<GetListResponse>
            (
                $"{BaseUrl}api/List/GetList",
                JsonSerializer.Serialize
                (
                    new GetTypedListRequest
                    {
                        Selector = selectorLambdaOperatorDescriptor,
                        ModelType = typeof(LookUpsModel).AssemblyQualifiedName,
                        DataType = typeof(LookUps).AssemblyQualifiedName,
                        ModelReturnType = typeof(IEnumerable<StateLivedInModel>).AssemblyQualifiedName,
                        DataReturnType = typeof(IEnumerable<StateLivedIn>).AssemblyQualifiedName
                    }
                ),
                SerializationOptions.Default
            );

            Assert.True(result.List.Any());
        }

        [Fact]
        public async Task GetEntityRequest_As_ResidencyModel()
        {
            //arrange
            IHttpClientHelper helper = serviceProvider.GetRequiredService<IHttpClientHelper>();

            //act
            var result = await helper.PostAsync<GetEntityResponse>
            (
                $"{BaseUrl}api/Entity/GetEntity",
                JsonSerializer.Serialize
                (
                    new GetEntityRequest
                    {
                        Filter = GetFilterExpressionDescriptor<ResidencyModel>
                        (
                            GetResidencyByIdFilterBody(1),
                            "q"
                        ),
                        SelectExpandDefinition = new SelectExpandDefinitionDescriptor
                        (
                            [],
                            [
                                new SelectExpandItemDescriptor("StatesLivedIn")
                            ]
                        ),
                        ModelType = typeof(ResidencyModel).AssemblyQualifiedName,
                        DataType = typeof(Residency).AssemblyQualifiedName
                    }
                ),
                SerializationOptions.Default
            );

            Assert.NotNull(result);
            Assert.NotNull(result.Entity);
            Assert.NotEmpty(((ResidencyModel)result.Entity).StatesLivedIn);
        }

        [Fact]
        public async Task GetEntityRequest_As_ResidencyModel_FromObjectConstant()
        {
            //arrange
            IHttpClientHelper helper = serviceProvider.GetRequiredService<IHttpClientHelper>();

            //act
            var result = await helper.PostAsync<GetEntityResponse>
            (
                $"{BaseUrl}api/Entity/GetEntity",
                JsonSerializer.Serialize
                (
                    new GetEntityRequest
                    {
                        Filter = GetFilterExpressionDescriptor<ResidencyModel>
                        (
                            GetResidencyByIdFilterBodyFromObjectConstant(new ResidencyModel { UserId = 1 }),
                            "q"
                        ),
                        SelectExpandDefinition = new SelectExpandDefinitionDescriptor
                        (
                            [],
                            [
                                new SelectExpandItemDescriptor("StatesLivedIn")
                            ]
                        ),
                        ModelType = typeof(ResidencyModel).AssemblyQualifiedName,
                        DataType = typeof(Residency).AssemblyQualifiedName
                    }
                ),
                SerializationOptions.Default
            );

            Assert.NotNull(result);
            Assert.NotNull(result.Entity);
            Assert.NotEmpty(((ResidencyModel)result.Entity).StatesLivedIn);
        }

        #region Helpers
        private static SelectDescriptor GetBodyConvertLookupsModelToStatesLivedInModel()
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
                                "states",
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
                        ["State"] = new MemberSelectorDescriptor
                        (
                            "Value",
                            new ParameterDescriptor("l")
                        )
                    },
                    typeof(StateLivedInModel).AssemblyQualifiedName
                ),
                "l"
            );

        private static SelectDescriptor GetBodyForLookupsModel()
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
                    },
                    typeof(LookUpsModel).AssemblyQualifiedName
                ),
                "l"
            );

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

        private static EqualsBinaryDescriptor GetResidencyByIdFilterBody(int id)
            => new
            (
                new MemberSelectorDescriptor
                (
                    "UserId",
                    new ParameterDescriptor("q")
                ),
                new ConstantDescriptor(id, typeof(int).AssemblyQualifiedName)
            );

        private static EqualsBinaryDescriptor GetResidencyByIdFilterBodyFromObjectConstant(ResidencyModel residency)
            => new
            (
                new MemberSelectorDescriptor
                (
                    "UserId",
                    new ParameterDescriptor("q")
                ),
                new MemberSelectorDescriptor
                (
                    "UserId",
                    new ConstantDescriptor(residency, typeof(ResidencyModel).AssemblyQualifiedName)
                )
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
            services.AddAppUtilsHttpClientHelper();
            services.Configure<UrlOptions>(configuration);
            serviceProvider = services.BuildServiceProvider();
        }
        #endregion Helpers
    }
}
