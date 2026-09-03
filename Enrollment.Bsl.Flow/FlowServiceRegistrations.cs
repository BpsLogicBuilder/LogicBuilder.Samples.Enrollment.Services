using Enrollment.Bsl.Flow;
using Enrollment.Bsl.Flow.Interfaces;
using Enrollment.Repositories;
using Enrollment.Stores;
using LogicBuilder.App.Utils.Rules;
using LogicBuilder.EntityFrameworkCore.Repositories;
using LogicBuilder.RulesDirector;

#pragma warning disable IDE0130 //Microsoft recommended namespace for service registrations
namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static class FlowServiceRegistrations
    {
        public static IServiceCollection AddEnrollmentBslFlowServices(this IServiceCollection services)
        {
            return services
                .AddAppUtilsServices()
                .AddHttpClient()
                .AddFlowFactories()
                .AddBslUtilsServices()
                .AddRulesCacheService
                (
                    new RulesLoaderRequest
                    (
                        "Enrollment.Bsl.Flow.Rulesets",
                        typeof(FlowActivity),
                        [
                            typeof(LogicBuilder.App.Utils.Interfaces.ITypeHelper).Assembly,
                            typeof(LogicBuilder.Forms.Parameters.Expansions.SelectExpandDefinitionParameters).Assembly,
                            typeof(Enrollment.Domain.Entities.UserModel).Assembly,
                            typeof(Enrollment.Data.Entities.User).Assembly,
                            typeof(DirectorBase).Assembly,
                            typeof(string).Assembly
                        ]
                    )
                )
                .AddTransient<IEnrollmentStore, EnrollmentStore>()
                .AddTransient<IContextRepository, EnrollmentRepository>()
                .AddTransient<IEnrollmentRepository, EnrollmentRepository>()
                .AddTransient<IFlowManager, FlowManager>()
                .AddScoped<Progress>();
        }
    }
}
