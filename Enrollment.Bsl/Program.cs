using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.CompilerServices;
//[assembly: InternalsVisibleTo("Enrollment.Bsl.Flow.Tests")]
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCertificateAuthorization(builder);

builder.Services
    .AddSqlServerDatabaseConfiguration(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection is required"))
    .AddLogging()
    .AddEnrollmentBslFlowServices()
    .AddAutoMapperConfiguration();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class Program 
{
    protected Program() { }
}
