using LogicBuilder.App.Utils.Json;
using LogicBuilder.Domain.Json;
using LogicBuilder.Expressions.Utils.Json;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

#pragma warning disable IDE0130 //Microsoft recommended namespace for service registrations
namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static class CertificateAuthorizationServiceRegistrations
    {
        public static IServiceCollection AddCertificateAuthorization(this IServiceCollection services, WebApplicationBuilder builder)
        {
            const string HttpsOnlyCertificateValidationPolicy = "HttpsOnlyCertificateValidationPolicy";
            services.AddHttpContextAccessor();
            services.AddControllers
            (
                options =>
                {//needed to prevent successful response with failed certificate authentication
                    options.Filters.Add(new AuthorizeFilter(HttpsOnlyCertificateValidationPolicy));
                }
            )
            .AddJsonOptions
            (
                options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new DescriptorConverter());
                    options.JsonSerializerOptions.Converters.Add(new ModelConverter());
                    options.JsonSerializerOptions.Converters.Add(new ObjectConverter());
                }
            );

            services.AddAuthorization(options =>
            {
                options.AddPolicy(HttpsOnlyCertificateValidationPolicy, policy =>
                {
                    // Require the Certificate Authentication Scheme for this policy
                    policy.AddAuthenticationSchemes(HttpsOnlyCertificateValidationPolicy);
                    policy.RequireAssertion
                    (
                        context =>
                        {
                            return CheckUserValidation(context);
                        }
                    );
                });
            })
            .AddAuthentication(HttpsOnlyCertificateValidationPolicy)
            .AddCertificate(HttpsOnlyCertificateValidationPolicy, options =>
            {
                // Allow self-signed certificates (typical for development/testing)
                options.AllowedCertificateTypes = CertificateTypes.All;

                // Validate the certificate
                options.ValidateCertificateUse = true;
                options.ValidateValidityPeriod = true;

                options.Events = new CertificateAuthenticationEvents
                {
                    OnCertificateValidated = context =>
                    {
                        var certificate = context.ClientCertificate;
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        string expectedThumbprint = builder.Configuration["ExpectedCerificateThumbprint"] ?? "";

                        if (certificate.Thumbprint.Equals(expectedThumbprint, StringComparison.OrdinalIgnoreCase)
                            || ConvertFromHexToBase64(certificate.Thumbprint).Equals(expectedThumbprint, StringComparison.Ordinal))
                        {
                            if (logger.IsEnabled(LogLevel.Information))
                            {
                                logger.LogInformation
                                (
                                    "Certificate validated: Subject={Subject}, Thumbprint={Thumbprint}",
                                    certificate.Subject,
                                    certificate.Thumbprint
                                );
                            }

                            context.Success();
                        }
                        else
                        {
                            if (logger.IsEnabled(LogLevel.Information))
                            {
                                logger.LogInformation
                                (
                                    "Certificate validation failed: Subject={Subject}, Thumbprint={Thumbprint}",
                                    certificate.Subject,
                                    certificate.Thumbprint
                                );
                            }

                            context.Fail("Invalid client certificate thumbprint.");
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogError(context.Exception, "Certificate authentication failed");
                        context.Fail("Certificate authentication failed");
                        return Task.CompletedTask;
                    }
                };
            });

            // Configure Kestrel to allow client certificates
            if (builder.Environment.IsDevelopment())
            {
                builder.WebHost.ConfigureKestrel(serverOptions =>
                {
                    serverOptions.ConfigureHttpsDefaults(httpsOptions =>
                    {//default is NoCertificate which causes failures when a certificate is sent.
                        httpsOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
                        httpsOptions.ClientCertificateValidation = (cert, chain, errors) =>
                        {// FORCE Kestrel to accept the certificate at the TLS level, even if untrusted
                            // Return true to allow the TLS handshake to succeed regardless of trust errors
                            return true;
                        };
                    });
                });
            }

            return services;
        }

        private static bool CheckUserValidation(AspNetCore.Authorization.AuthorizationHandlerContext context)
        {
            HttpContext? httpContext = context.Resource switch
            {
                HttpContext httpCtx => httpCtx,
                Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext mvcCtx => mvcCtx.HttpContext,
                _ => null
            };

            if (httpContext?.Request.Scheme == "http")
                return true;//certificate will not be received by the server
                            //so the policy should return true for http - useful when using http in development.

            return httpContext?.Request.Scheme == "https" && context.User?.Identity?.IsAuthenticated == true;
        }

        private static string ConvertFromHexToBase64(string hexString)
        {
            return Convert.ToBase64String(Convert.FromHexString(hexString));
        }
    }
}
