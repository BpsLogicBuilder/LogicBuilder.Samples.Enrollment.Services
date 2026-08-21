#pragma warning disable IDE0130 //Microsoft recommended namespace for service registrations
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Enrollment.BSL.AutoMapperProfiles;
using Enrollment.Contexts;
using LogicBuilder.EntityFrameworkCore.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130
{
    public static class BslServiceRegistrations
    {
        public static IServiceCollection AddAutoMapperConfiguration(this IServiceCollection services)
        {
            return services
                .AddSingleton<AutoMapper.IConfigurationProvider>
                (
                    ConfigurationHelper.GetMapperConfiguration(cfg =>
                    {
                        cfg.AddExpressionMapping();

                        cfg.AddProfile<ExpressionOperatorsMappingProfile>();
                        cfg.AddProfile<ExpressionParameterToDescriptorMappingProfile>();
                        cfg.AddProfile<ExpansionParameterToDescriptorMappingProfile>();
                        cfg.AddProfile<ExpansionDescriptorToOperatorMappingProfile>();
                        cfg.AddProfile<EnrollmentProfile>();
                    })
                )
                .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<AutoMapper.IConfigurationProvider>(), sp.GetService));
        }

        public static IServiceCollection AddSqlServerDatabaseConfiguration(this IServiceCollection services, string connectionString)
        {
            return services.AddDbContext<EnrollmentContext>
            (
                options => options.UseSqlServer
                (
                    connectionString,
                    options => options.EnableRetryOnFailure()
                ),
                ServiceLifetime.Transient
            );
        }
    }
}
