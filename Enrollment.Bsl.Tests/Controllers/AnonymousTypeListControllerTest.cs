using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Enrollment.Bsl.Controllers;
using Enrollment.BSL.AutoMapperProfiles;
using Enrollment.Contexts;
using Enrollment.Data.Entities;
using Enrollment.Domain.Entities;
using Enrollment.Repositories;
using LogicBuilder.App.Bsl.Business.Requests;
using LogicBuilder.App.Bsl.Business.Responses;
using LogicBuilder.App.Bsl.Utils.Interfaces;
using LogicBuilder.EntityFrameworkCore.Mapping;
using LogicBuilder.Expressions.Utils.ExpressionDescriptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Enrollment.Bsl.Tests.Controllers
{
    [Collection("DatabaseCollection")]
    public class AnonymousTypeListControllerTest
    {
        static AnonymousTypeListControllerTest()
        {
            InitializeMapperConfiguration();
        }

        public AnonymousTypeListControllerTest(DatabaseFixture databaseFixture)
        {
            this.databaseFixture = databaseFixture;
            Initialize();
        }

        #region Fields
        private readonly DatabaseFixture databaseFixture;
        private static MapperConfiguration MapperConfiguration;
        private IServiceProvider? serviceProvider;
        #endregion Fields

        [Fact]
        public async Task GetAnonymousList_Generic_Returns_Dynamic_List()
        {
            //arrange
            var selectorLambdaOperatorDescriptor = GetExpressionDescriptor<IQueryable<UserModel>, IEnumerable<dynamic>>
            (
                GetUsersBodyOrderByName(),
                "q"
            );

            IRequestHelper requestHelper = serviceProvider!.GetRequiredService<IRequestHelper>();
            AnonymousTypeListController controller = new(requestHelper);

            //act
            var result = await controller.GetList
            (
                new GetObjectListRequest
                {
                    Selector = selectorLambdaOperatorDescriptor,
                    ModelType = typeof(UserModel).AssemblyQualifiedName,
                    DataType = typeof(User).AssemblyQualifiedName
                }
            );

            //assert
            Assert.True(result.Success);
            Assert.NotEmpty(((GetObjectListResponse)result).List);
        }

        #region Helpers
        private static OrderByDescriptor GetUsersBodyOrderByName()
            => new
            (
                new ParameterDescriptor("q"),
                new MemberSelectorDescriptor
                (
                    "UserName",
                    new ParameterDescriptor("d")
                ),
                LogicBuilder.Expressions.Utils.Strutures.ListSortDirection.Ascending,
                "d"
            );

        private static SelectorLambdaDescriptor GetExpressionDescriptor<T, TResult>(DescriptorBase selectorBody, string parameterName = "$it")
            => new(selectorBody, typeof(T).AssemblyQualifiedName!, parameterName, typeof(TResult).AssemblyQualifiedName!);

        [MemberNotNull(nameof(MapperConfiguration))]
        private static void InitializeMapperConfiguration()
        {
            MapperConfiguration ??= ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();

                cfg.AddProfile<ExpressionOperatorsMappingProfile>();
                cfg.AddProfile<ExpressionParameterToDescriptorMappingProfile>();
                cfg.AddProfile<ExpansionParameterToDescriptorMappingProfile>();
                cfg.AddProfile<ExpansionDescriptorToOperatorMappingProfile>();
                cfg.AddProfile<EnrollmentProfile>();
            });
            MapperConfiguration.AssertConfigurationIsValid();
        }

        [MemberNotNull(nameof(serviceProvider))]
        private void Initialize()
        {
            serviceProvider ??= new ServiceCollection()
                .AddDbContext<EnrollmentContext>
                (
                    options => options.UseSqlServer
                    (
                        databaseFixture.GetConnectionString($"{GetType().Name}_{Guid.NewGuid():N}"),
                        options => options.EnableRetryOnFailure()
                    ),
                    ServiceLifetime.Transient
                )
                .AddLogging()
                .AddEnrollmentBslFlowServices()
                .AddSingleton<AutoMapper.IConfigurationProvider>
                (
                    MapperConfiguration
                )
                .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<AutoMapper.IConfigurationProvider>(), sp.GetService))
                .BuildServiceProvider();

            ReCreateDataBase(serviceProvider.GetRequiredService<EnrollmentContext>()).GetAwaiter().GetResult();
            DatabaseSeeder.Seed_Database(serviceProvider.GetRequiredService<IEnrollmentRepository>()).GetAwaiter().GetResult();
        }

        private static async Task ReCreateDataBase(EnrollmentContext context)
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
        }
        #endregion Helpers
    }
}
