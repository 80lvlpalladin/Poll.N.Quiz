using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.API.Shared;
using Poll.N.Quiz.Settings.API.Queries;
using Poll.N.Quiz.API.Shared.Extensions;
using Poll.N.Quiz.Infrastructure.ServiceDiscovery;
using Poll.N.Quiz.Settings.API;
using Poll.N.Quiz.Settings.API.Commands;
using Poll.N.Quiz.Settings.EventQueue;
using Poll.N.Quiz.Settings.API.Synchronizer;
using Poll.N.Quiz.Settings.API.Synchronizer.Consumers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

configuration
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables();

builder
    .AddConcurrencyRateLimiter()
    .AddTelemetry();

var settingsEventStoreConnectionString =
    configuration.GetConnectionString(nameof(AspireResource.SettingsEventStore));
var settingsProjectionStoreConnectionString =
    configuration.GetConnectionString(nameof(AspireResource.SettingsProjectionStore));
var settingsEventQueueConnectionString =
    configuration.GetConnectionString(nameof(AspireResource.SettingsEventQueue));

if (settingsEventQueueConnectionString is null ||
    settingsEventStoreConnectionString is null ||
    settingsProjectionStoreConnectionString is null)
{
    throw new InvalidOperationException($"Connection strings for " +
         $"{nameof(AspireResource.SettingsEventStore)}, " +
         $"{nameof(AspireResource.SettingsProjectionStore)} or " +
         $"{nameof(AspireResource.SettingsEventQueue)} are not configured.");
}

builder
    .Services
        .AddExceptionHandler<GlobalExceptionHandler>()
        .AddSettingsEventQueueProducerAndConsumer<SettingsEventQueueConsumer>(settingsEventQueueConnectionString)
        .AddQueryServices(settingsProjectionStoreConnectionString)
        .AddCommandServices(settingsEventStoreConnectionString)
        .AddSynchronizerServices(configuration, settingsProjectionStoreConnectionString, settingsEventStoreConnectionString)
        .AddOpenApi()
        .AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .WithOrigins(
                        ConnectionStringResolver.GetDotNetConnectionStringFromEnvironment(
                            AspireResource.SettingsWeb))
                    //.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

var app = builder.Build();

app.MapOpenApi();

app.MapScalarApiReference();

app.MapEndpoints();

app.UseCors();

app.UseHttpsRedirection();

app.Run();
