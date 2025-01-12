using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Poll.N.Quiz.Settings.Queries;
using Poll.N.Quiz.API.Shared;
using Poll.N.Quiz.API.Shared.ExceptionHandlers;
using Poll.N.Quiz.API.Shared.Extensions;
using Poll.N.Quiz.Settings.API;
using Poll.N.Quiz.Settings.Commands;
using Poll.N.Quiz.Settings.Synchronizer;
using Poll.N.Quiz.Settings.Synchronizer.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions{ Args = args });

builder
    .AddConcurrencyRateLimiter(concurrentRequestsLimit: 10)
    .Configuration
        .AddJsonFile("appsettings.json");

var configuration = builder.Configuration;

builder.Services
    .AddSingleton<CurrentEnvironment>()
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddQueryServices(configuration)
    .AddCommandServices()
    .AddSynchronizerServices(configuration)
    .AddOpenApi();

var app = builder.Build();

await InitializeSettingsProjectionAsync(app.Services);

app.MapOpenApi();

app.MapScalarApiReference();

app.MapEndpoints();

app.UseHttpsRedirection();

app.Run();

return;

async Task InitializeSettingsProjectionAsync
    (IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
{
    var projectionUpdater = serviceProvider.GetRequiredService<ProjectionUpdater>();
    await projectionUpdater.InitializeProjectionAsync(cancellationToken);
}

