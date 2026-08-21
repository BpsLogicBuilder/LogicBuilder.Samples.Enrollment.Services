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
    public class EntityControllerTest
    {
        static EntityControllerTest()
        {
            InitializeMapperConfiguration();
        }

        public EntityControllerTest(DatabaseFixture databaseFixture)
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
        public async Task GetEntity_succeeds()
        {
            //arrange
            var filterLambdaOperatorDescriptor = GetFilterExpressionDescriptor<UserModel>
            (
                GetUserByIdFilterBody(1),
                "q"
            );

            IRequestHelper requestHelper = serviceProvider!.GetRequiredService<IRequestHelper>();
            EntityController controller = new(requestHelper);

            //act
            var result = (GetEntityResponse) await controller.GetEntity
            (
                new GetEntityRequest
                {
                    Filter = filterLambdaOperatorDescriptor,
                    ModelType = typeof(UserModel).AssemblyQualifiedName,
                    DataType = typeof(User).AssemblyQualifiedName,
                }
            );

            //assert
            Assert.NotNull(result.Entity);
            Assert.Equal(1, ((UserModel)result.Entity).UserId);
        }

        #region Helpers
        private static EqualsBinaryDescriptor GetUserByIdFilterBody(int id)
            => new
            (
                new MemberSelectorDescriptor
                (
                    "UserId",
                    new ParameterDescriptor("q")
                ),
                new ConstantDescriptor(id, typeof(int).AssemblyQualifiedName)
            );

        private static FilterLambdaDescriptor GetFilterExpressionDescriptor<T>(DescriptorBase filterBody, string parameterName = "$it")
            => new(filterBody, typeof(T).AssemblyQualifiedName!, parameterName);

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
