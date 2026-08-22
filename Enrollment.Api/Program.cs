using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Enrollment.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
const string SecureCorsPolicy = "SecureCorsPolicy";
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(SecureCorsPolicy, policy =>
    {
        if (allowedOrigins != null && allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .WithMethods("GET", "POST", "PUT", "DELETE")
                  .WithHeaders("Content-Type", "Authorization");
        }
    });
});
builder.Services.AddControllers().AddJsonOptions
(
    options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = SerializationOptions.Default.PropertyNameCaseInsensitive;
        foreach (var converter in SerializationOptions.Default.Converters)
            options.JsonSerializerOptions.Converters.Add(converter);
    }
);

var certificateClient = new CertificateClient(new Uri(builder.Configuration["keyVaultUrl"]!), new DefaultAzureCredential());
var certificate = await certificateClient.DownloadCertificateAsync(builder.Configuration["bslCertificateName"]);

builder.Services.AddHttpClient(HttpClientOptions.BslClientName, client =>
{
    client.BaseAddress = new Uri(builder.Configuration["baseBslUrl"] ?? throw new InvalidOperationException("baseBslUrl is required"));
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    if (certificate?.Value != null)
        handler.ClientCertificates.Add(certificate.Value);

    if (builder.Environment.IsDevelopment())
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator!;

    return handler;
});

builder.Services.AddAppUtilsHttpClientHelper();
builder.Services.Configure<UrlOptions>(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseCors(SecureCorsPolicy);

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
