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
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Enrollment.Bsl.Flow.Tests
{
    [Collection("DatabaseCollection")]
    public class InsertCertificationTest
    {
        static InsertCertificationTest()
        {
            InitializeMapperConfiguration();
        }

        public InsertCertificationTest(DatabaseFixture databaseFixture, ITestOutputHelper output)
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
        public void SaveCertification()
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

            var certification = new CertificationModel
            {
                UserId = user.UserId,
                EntityState = LogicBuilder.Domain.EntityStateType.Added,
                CertificateStatementChecked = true,
                DeclarationStatementChecked = true,
                PolicyStatementsChecked = true
            };
            flowManager.FlowDataCache.Request = new SaveEntityRequest { Entity = certification };

            //act
            System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
            flowManager.Start("savecertification");
            stopWatch.Stop();
            this.output.WriteLine("Saving valid certification  = {0}", stopWatch.Elapsed.TotalMilliseconds);

            //assert
            Assert.True(flowManager.FlowDataCache.Response!.Success);
            Assert.Empty(flowManager.FlowDataCache.Response.ErrorMessages);

            CertificationModel model = (CertificationModel)((SaveEntityResponse)flowManager.FlowDataCache.Response).Entity!;
            Assert.True(model.CertificateStatementChecked);
        }

        [Fact]
        public void SaveInvalidCertification()
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

            var certification = new CertificationModel
            {
                UserId = user.UserId,
                EntityState = LogicBuilder.Domain.EntityStateType.Added,
                CertificateStatementChecked = false,
                DeclarationStatementChecked = false,
                PolicyStatementsChecked = false
            };
            certification.CertificateStatementChecked = false;
            certification.DeclarationStatementChecked = false;
            certification.PolicyStatementsChecked = false;
            flowManager.FlowDataCache.Request = new SaveEntityRequest { Entity = certification };

            //act
            System.Diagnostics.Stopwatch stopWatch = System.Diagnostics.Stopwatch.StartNew();
            flowManager.Start("savecertification");
            stopWatch.Stop();
            this.output.WriteLine("Saving valid certification = {0}", stopWatch.Elapsed.TotalMilliseconds);

            //assert
            Assert.False(flowManager.FlowDataCache.Response!.Success);
            Assert.Equal(3, flowManager.FlowDataCache.Response.ErrorMessages.Count);
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
