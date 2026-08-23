using Enrollment.Domain.Entities;
using LogicBuilder.App.Bsl.Business.Requests;
using LogicBuilder.App.Bsl.Business.Responses;
using LogicBuilder.App.Utils.Web.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;

namespace Enrollment.Api.Tests
{
    public class SaveResidencyTest
    {
        public SaveResidencyTest()
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
        public async Task SaveResidency()
        {
            // arrange
            IHttpClientHelper helper = serviceProvider.GetRequiredService<IHttpClientHelper>();

            //act
            List<Task<SaveEntityResponse>> tasks = [];
            for (int i = 0; i < 30; i++)
            {
                tasks.Add
                (
                    helper.PostAsync<SaveEntityResponse>
                    (
                        $"{BaseUrl}api/Residency/Save",
                        JsonSerializer.Serialize
                        (
                            new SaveEntityRequest
                            {
                                Entity = new ResidencyModel
                                {
                                    UserId = 1,
                                    CitizenshipStatus = "RA",
                                    CountryOfCitizenship = "AA",
                                    DriversLicenseNumber = "GA12345",
                                    DriversLicenseState = "GA",
                                    HasValidDriversLicense = true,
                                    ImmigrationStatus = "BB",
                                    ResidentState = "AR",
                                    StatesLivedIn =
                                    [
                                        new StateLivedInModel { StateLivedInId = 1, UserId = 1, EntityState = LogicBuilder.Domain.EntityStateType.Modified, State = "GA"  },
                                        new StateLivedInModel { StateLivedInId = 2, UserId = 1, EntityState = LogicBuilder.Domain.EntityStateType.Modified, State = "TN" }
                                    ],
                                    EntityState = LogicBuilder.Domain.EntityStateType.Modified
                                }
                            }
                        ),
                        SerializationOptions.Default
                    )
                );

                await Task.WhenAll(tasks);

                //assert
                tasks.ForEach(task => Assert.True(task.Result.Success));
            }
        }

        #region Helpers
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
