using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Enrollment.Bsl.Flow.Interfaces;
using Enrollment.BSL.AutoMapperProfiles;
using Enrollment.Contexts;
using Enrollment.Data.Entities;
using Enrollment.Domain.Entities;
using Enrollment.Repositories;
using LogicBuilder.App.Bsl.Business.Requests;
using LogicBuilder.EntityFrameworkCore.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Enrollment.Bsl.Flow.Tests
{
    [Collection("DatabaseCollection")]
    public class DeletePersonalTest
    {
        static DeletePersonalTest()
        {
            InitializeMapperConfiguration();
        }

        public DeletePersonalTest(DatabaseFixture databaseFixture, ITestOutputHelper output)
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
        public async Task DeleteValidPersonalRequest()
        {
            //arrange
            IFlowManager flowManager = serviceProvider!.GetRequiredService<IFlowManager>();
            IEnrollmentRepository enrollmentRepository = serviceProvider!.GetRequiredService<IEnrollmentRepository>();
            var personal = (await enrollmentRepository.GetAsync<PersonalModel, Personal>
            (
                s => s.UserId == 1
            )).Single();
            flowManager.FlowDataCache.Request = new DeleteEntityRequest { Entity = personal };

            //act
            System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
            flowManager.Start("deletepersonal");
            stopWatch.Stop();
            this.output.WriteLine("Deleting valid personal = {0}", stopWatch.Elapsed.TotalMilliseconds);

            personal = (await enrollmentRepository.GetAsync<PersonalModel, Personal>
            (
                s => s.UserId == 1
            )).SingleOrDefault();

            //assert
            Assert.True(flowManager.FlowDataCache.Response!.Success);
            Assert.Null(personal);
        }

        [Fact]
        public async Task DeletePersonalNotFoundRequest()
        {
            //arrange
            IFlowManager flowManager = serviceProvider!.GetRequiredService<IFlowManager>();
            IEnrollmentRepository enrollmentRepository = serviceProvider!.GetRequiredService<IEnrollmentRepository>();
            var personal = (await enrollmentRepository.GetAsync<PersonalModel, Personal>
            (
                s => s.UserId == 1
            )).Single();
            personal.UserId = Int32.MaxValue;
            flowManager.FlowDataCache.Request = new DeleteEntityRequest { Entity = personal };

            //act
            System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
            flowManager.Start("deletepersonal");
            stopWatch.Stop();
            this.output.WriteLine("Deleting personal not found = {0}", stopWatch.Elapsed.TotalMilliseconds);

            personal = (await enrollmentRepository.GetAsync<PersonalModel, Personal>
            (
                s => s.UserId == 1
            )).SingleOrDefault();

            //assert
            Assert.False(flowManager.FlowDataCache.Response!.Success);
            Assert.Single(flowManager.FlowDataCache.Response.ErrorMessages);
            Assert.NotNull(personal);
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
