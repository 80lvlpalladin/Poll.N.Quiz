using Poll.N.Quiz.API;
using Poll.N.Quiz.API.Shared;
using Poll.N.Quiz.API.Shared.Extensions;
using Poll.N.Quiz.Infrastructure.Clients;
using Poll.N.Quiz.Infrastructure.ServiceDiscovery;
using Refit;
using Scalar.AspNetCore;



var builder = WebApplication.CreateBuilder(args);

builder
    .AddTelemetry();

var serviceName = nameof(AspireResource.Api);
var environmentName = builder.Environment.EnvironmentName;
var settingsApiClient = RestService.For<ISettingsApiClient>(
    ConnectionStringResolver.GetHardcodedConnectionString(AspireResource.SettingsApi));

builder.Configuration.AddConfigurationFromSettingsApi(serviceName, environmentName, settingsApiClient);

builder.AddConcurrencyRateLimiter();


builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddOpenApi();
}

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference();
}

app.MapEndpoints();

app.UseHttpsRedirection();

app.Run();

