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
    public class SavePersonalTest
    {
        public SavePersonalTest()
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
        public async Task SavePersonal()
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
                        $"{BaseUrl}api/Personal/Save",
                        JsonSerializer.Serialize
                        (
                            new SaveEntityRequest
                            {
                                Entity = new PersonalModel
                                {
                                    UserId = 1,
                                    FirstName = "Mike",
                                    MiddleName = "Tyson",
                                    LastName = "Smith",
                                    PrimaryEmail = "go.stay@jack.com",
                                    Address1 = "Third Street",
                                    City = "Dallas",
                                    State = "GA",
                                    ZipCode = "30060",
                                    CellPhone = "770-855-0050",
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
