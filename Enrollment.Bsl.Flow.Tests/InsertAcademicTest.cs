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
    public class InsertAcademicTest
    {
        static InsertAcademicTest()
        {
            InitializeMapperConfiguration();
        }

        public InsertAcademicTest(DatabaseFixture databaseFixture, ITestOutputHelper output)
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
        public void SaveAcademic()
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

            var academic = new AcademicModel
            {
                UserId = user.UserId,
                EntityState = LogicBuilder.Domain.EntityStateType.Added,
                AttendedPriorColleges = true,
                FromDate = new DateTime(2010, 10, 10, 0, 0, 0, DateTimeKind.Unspecified),
                ToDate = new DateTime(2014, 10, 10, 0, 0, 0, DateTimeKind.Unspecified),
                GraduationStatus = "H",
                EarnedCreditAtCmc = true,
                LastHighSchoolLocation = "NC",
                NcHighSchoolName = "NCSCHOOL1",
                Institutions =
                [
                    new InstitutionModel
                    {
                        EntityState = LogicBuilder.Domain.EntityStateType.Added,
                        HighestDegreeEarned = "BD",
                        StartYear = "2015",
                        EndYear = "2018",
                        InstitutionName = "Florida Institution 1",
                        InstitutionState = "FL",
                        MonthYearGraduated = new DateTime(2020, 10, 10, 0, 0, 0, DateTimeKind.Unspecified)
                    }
                ]
            };

            flowManager.FlowDataCache.Request = new SaveEntityRequest { Entity = academic };

            //act
            System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
            flowManager.Start("saveacademic");
            stopWatch.Stop();
            this.output.WriteLine("Saving valid academic = {0}", stopWatch.Elapsed.TotalMilliseconds);

            //assert
            Assert.True(flowManager.FlowDataCache.Response!.Success);
            Assert.Empty(flowManager.FlowDataCache.Response.ErrorMessages);

            AcademicModel model = (AcademicModel)((SaveEntityResponse)flowManager.FlowDataCache.Response).Entity!;
            Assert.Equal("NC", model.LastHighSchoolLocation);
            Assert.Equal("2018", model.Institutions.First().EndYear);
        }

        [Fact]
        public void SaveInvalidAcademic()
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

            var academic = new AcademicModel
            {
                UserId = user.UserId,
                EntityState = LogicBuilder.Domain.EntityStateType.Added,
                AttendedPriorColleges = true,
                FromDate = DateTime.MinValue,
                ToDate = DateTime.MinValue,
                GraduationStatus = null!,
                EarnedCreditAtCmc = true,
                LastHighSchoolLocation = null!,
                NcHighSchoolName = "NCSCHOOL1",
                Institutions =
                [
                    new InstitutionModel
                    {
                        EntityState = LogicBuilder.Domain.EntityStateType.Added,
                        HighestDegreeEarned = "BD",
                        StartYear = "2015",
                        EndYear = null!,
                        InstitutionName = "Florida Institution 1",
                        InstitutionState = "FL",
                        MonthYearGraduated = new DateTime(2020, 10, 10, 0, 0, 0, DateTimeKind.Unspecified)
                    }
                ]
            };

            flowManager.FlowDataCache.Request = new SaveEntityRequest { Entity = academic };

            //act
            System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
            flowManager.Start("saveacademic");
            stopWatch.Stop();
            this.output.WriteLine("Saving valid academic = {0}", stopWatch.Elapsed.TotalMilliseconds);

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
