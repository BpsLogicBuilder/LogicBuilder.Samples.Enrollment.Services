using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Enrollment.Bsl.Flow.Interfaces;
using Enrollment.BSL.AutoMapperProfiles;
using Enrollment.Contexts;
using Enrollment.Domain.Entities;
using Enrollment.Repositories;
using LogicBuilder.App.Bsl.Business.Requests;
using LogicBuilder.App.Bsl.Business.Responses;
using LogicBuilder.EntityFrameworkCore.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Enrollment.Bsl.Flow.Tests
{
    [Collection("DatabaseCollection")]
    public class InsertResidencyTest
    {
        static InsertResidencyTest()
        {
            InitializeMapperConfiguration();
        }

        public InsertResidencyTest(DatabaseFixture databaseFixture, ITestOutputHelper output)
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
        public void SaveResidency()
        {
            //arrange
            IFlowManager flowManager = serviceProvider!.GetRequiredService<IFlowManager>();
            var user = new UserModel
            {
                UserName = "NewName",
                EntityState = LogicBuilder.Domain.EntityStateType.Added
            };
            flowManager.FlowDataCache.Request = new SaveEntityRequest { Entity = user };
            flowManager.Start("saveuser");
            Assert.True(user.UserId > 1);

            var residency = new ResidencyModel
            {
                UserId = user.UserId,
                CitizenshipStatus = "US",
                DriversLicenseNumber = "NC12345",
                DriversLicenseState = "NC",
                EntityState = LogicBuilder.Domain.EntityStateType.Added,
                HasValidDriversLicense = true,
                ImmigrationStatus = "AA",
                ResidentState = "AR",
                StatesLivedIn =
                [
                    new StateLivedInModel { EntityState = LogicBuilder.Domain.EntityStateType.Added, State = "OH"  }
                ]
            };

            flowManager.FlowDataCache.Request = new SaveEntityRequest { Entity = residency };

            //act
            System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
            flowManager.Start("saveresidency");
            stopWatch.Stop();
            this.output.WriteLine("Saving valid residency = {0}", stopWatch.Elapsed.TotalMilliseconds);

            //assert
            Assert.True(flowManager.FlowDataCache.Response!.Success);
            Assert.Empty(flowManager.FlowDataCache.Response.ErrorMessages);

            ResidencyModel model = (ResidencyModel)((SaveEntityResponse)flowManager.FlowDataCache.Response).Entity!;
            Assert.Equal("NC12345", model.DriversLicenseNumber);
            Assert.Equal("OH", model.StatesLivedIn.First().State);
        }

        [Fact]
        public void SaveInvalidResidency()
        {
            //arrange
            IFlowManager flowManager = serviceProvider!.GetRequiredService<IFlowManager>();
            var user = new UserModel
            {
                UserName = "NewName",
                EntityState = LogicBuilder.Domain.EntityStateType.Added
            };
            flowManager.FlowDataCache.Request = new SaveEntityRequest { Entity = user };
            flowManager.Start("saveuser");
            Assert.True(user.UserId > 1);

            var residency = new ResidencyModel
            {
                UserId = user.UserId,
                CitizenshipStatus = null!,
                CountryOfCitizenship = "AA",
                ResidentState = null!,
                EntityState = LogicBuilder.Domain.EntityStateType.Added,
                HasValidDriversLicense = true,
                DriversLicenseNumber = null!,
                DriversLicenseState = null!,
                StatesLivedIn = []
            };
            flowManager.FlowDataCache.Request = new SaveEntityRequest { Entity = residency };

            //act
            System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
            flowManager.Start("saveresidency");
            stopWatch.Stop();
            this.output.WriteLine("Saving valid residency = {0}", stopWatch.Elapsed.TotalMilliseconds);

            //assert
            Assert.False(flowManager.FlowDataCache.Response!.Success);
            Assert.Equal(5, flowManager.FlowDataCache.Response.ErrorMessages.Count);
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
