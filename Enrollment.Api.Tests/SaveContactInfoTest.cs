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
    public class SaveContactInfoTest
    {
        public SaveContactInfoTest()
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
        public async Task SaveContactInfo()
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
                        $"{BaseUrl}api/ContactInfo/Save",
                        JsonSerializer.Serialize
                        (
                            new SaveEntityRequest
                            {
                                Entity = new ContactInfoModel
                                {
                                    UserId = 1,
                                    HasFormerName = false,
                                    DateOfBirth = new DateTime(2003, 10, 10, 0, 0, 0, DateTimeKind.Unspecified),
                                    SocialSecurityNumber = "000-11-2222",
                                    Gender = "F",
                                    Race = "BL",
                                    Ethnicity = "NHS",
                                    EnergencyContactFirstName = "Jack",
                                    EnergencyContactLastName = "Spratt",
                                    EnergencyContactRelationship = "Father",
                                    EnergencyContactPhoneNumber = "770-222-3333",
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
