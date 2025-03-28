using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.API.Shared;
using Poll.N.Quiz.Settings.Queries;
using Poll.N.Quiz.API.Shared.Extensions;
using Poll.N.Quiz.Aspire;
using Poll.N.Quiz.Settings.API;
using Poll.N.Quiz.Settings.Commands;
using Poll.N.Quiz.Settings.EventQueue;
using Poll.N.Quiz.Settings.Synchronizer;
using Poll.N.Quiz.Settings.Synchronizer.Consumers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables();

builder
    .AddConcurrencyRateLimiter()
    .AddTelemetry();

var settingsEventQueueConnectionString =
    builder.Configuration.GetSettingsEventQueueConnectionString();
var settingsProjectionConnectionString =
    builder.Configuration.GetSettingsProjectionConnectionString();
var settingsEventStoreConnectionString =
    builder.Configuration.GetSettingsEventStoreConnectionString();

var settingsEventQueueTopicName =
    builder.Configuration.GetSection("SettingsEventQueueTopicName").Get<string>();

if(string.IsNullOrWhiteSpace(settingsEventQueueTopicName))
    throw new ConfigurationException("SettingsEventQueueTopicName");

builder.Services
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddSettingsEventQueueProducerAndConsumer<SettingsEventQueueConsumer>(
        settingsEventQueueConnectionString,
        settingsEventQueueTopicName)
    .AddQueryServices(settingsProjectionConnectionString)
    .AddCommandServices(
        builder.Configuration,
        settingsEventStoreConnectionString,
        settingsEventQueueConnectionString)
    .AddSynchronizerServices(
        builder.Configuration,
        settingsProjectionConnectionString,
        settingsEventStoreConnectionString)
    .AddOpenApi();

var app = builder.Build();

app.MapOpenApi();

app.MapScalarApiReference();

app.MapEndpoints();

app.UseHttpsRedirection();

app.Run();

