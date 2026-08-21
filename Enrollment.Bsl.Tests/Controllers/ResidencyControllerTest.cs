using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Enrollment.Bsl.Controllers;
using Enrollment.Bsl.Flow.Interfaces;
using Enrollment.BSL.AutoMapperProfiles;
using Enrollment.Contexts;
using Enrollment.Data.Entities;
using Enrollment.Domain.Entities;
using Enrollment.Repositories;
using LogicBuilder.App.Bsl.Business.Requests;
using LogicBuilder.App.Bsl.Business.Responses;
using LogicBuilder.EntityFrameworkCore.Mapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Enrollment.Bsl.Tests.Controllers
{
    [Collection("DatabaseCollection")]
    public class ResidencyControllerTest
    {
        static ResidencyControllerTest()
        {
            InitializeMapperConfiguration();
        }

        public ResidencyControllerTest(DatabaseFixture databaseFixture, ITestOutputHelper output)
        {
            this.databaseFixture = databaseFixture;
            this.output = output;
            Initialize();
        }

        #region Fields
        private readonly DatabaseFixture databaseFixture;
        private readonly ITestOutputHelper output;
        private static MapperConfiguration MapperConfiguration;
        private IServiceProvider? serviceProvider;
        #endregion Fields

        [Fact]
        public async Task SaveResidency()
        {
            //arrange
            IFlowManager flowManager = serviceProvider!.GetRequiredService<IFlowManager>();
            IEnrollmentRepository enrollmentRepository = serviceProvider!.GetRequiredService<IEnrollmentRepository>();
            var residency = (await enrollmentRepository.GetAsync<ResidencyModel, Residency>
            (
                s => s.UserId == 1,
                null,
                new LogicBuilder.Expressions.Utils.Expansions.SelectExpandDefinition
                (
                    null,
                    [
                        new LogicBuilder.Expressions.Utils.Expansions.SelectExpandItem("StatesLivedIn")
                    ]
                )
            )).Single();

            residency.DriversLicenseNumber = "NC54321";
            StateLivedInModel stateLivedIn = residency.StatesLivedIn.First();
            stateLivedIn.State = "OH";
            residency.EntityState = LogicBuilder.Domain.EntityStateType.Modified;
            stateLivedIn.EntityState = LogicBuilder.Domain.EntityStateType.Modified;

            //act
            System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
            ResidencyController controller = new(flowManager);
            var result = (OkObjectResult)controller.Save(new SaveEntityRequest { Entity = residency });
            stopWatch.Stop();
            this.output.WriteLine("Saving valid residency = {0}", stopWatch.Elapsed.TotalMilliseconds);

            //assert
            Assert.True(flowManager.FlowDataCache.Response!.Success);
            Assert.Empty(flowManager.FlowDataCache.Response.ErrorMessages);

            ResidencyModel model = (ResidencyModel)((SaveEntityResponse)result.Value!).Entity!;
            Assert.Equal("NC54321", model.DriversLicenseNumber);
            Assert.Equal("OH", model.StatesLivedIn.First().State);
        }

        [Fact]
        public async Task DeleteValidResidencyRequest()
        {
            //arrange
            IFlowManager flowManager = serviceProvider!.GetRequiredService<IFlowManager>();
            IEnrollmentRepository enrollmentRepository = serviceProvider!.GetRequiredService<IEnrollmentRepository>();
            var residency = (await enrollmentRepository.GetAsync<ResidencyModel, Residency>
            (
                s => s.UserId == 1
            )).Single();

            //act
            System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
            ResidencyController controller = new(flowManager);
            var result = (OkObjectResult)controller.Delete(new DeleteEntityRequest { Entity = residency });
            stopWatch.Stop();
            this.output.WriteLine("Deleting valid residency = {0}", stopWatch.Elapsed.TotalMilliseconds);

            residency = (await enrollmentRepository.GetAsync<ResidencyModel, Residency>
            (
                s => s.UserId == 1
            )).SingleOrDefault();

            //assert
            Assert.Equal(200, result.StatusCode);
            Assert.True(flowManager.FlowDataCache.Response!.Success);
            Assert.Null(residency);
        }

        #region Helpers
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
