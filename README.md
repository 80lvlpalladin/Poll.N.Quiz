# Poll.N.Quiz Solution

## Overview

Poll.N.Quiz - a Web API that allows to store and retrieve appsettings.json files, needed for distributed ASP.NET Core applications. Aims to make appsettings.json file management easy and centralized for distributed systems built with ASP.NET Core.
Poll.N.Quiz is a .NET solution built with CQRS and Even Sourcing architecture principles. The system uses .NET Aspire for distributed application hosting and integrates several key technologies including MassTransit for messaging, MongoDB for event store, and Redis for read-only projection.

## Architecture

//TODO

## API Endpoints

The solution exposes REST endpoints through the Settings.API project:

```
POST api/v1/settings - Create or update settings
```

Additional endpoints are mapped through:
- `MapOpenApi()` - OpenAPI documentation
- `MapScalarApiReference()` - API references
- `MapEndpoints()` - Custom application endpoints

## Configuration

The solution uses a multi-layered configuration approach:
- JSON files for base configuration
- Environment variables for deployment-specific settings
- In-memory settings for dynamic configuration

## Getting Started

1. Ensure you have .NET 9.0 SDK installed
2. Configure connection strings in appsettings.json
3. Run the application using the AppHost project

## Dependencies

The solution uses central package management with key dependencies:
- Aspire.Hosting (v9.1.0)
- MassTransit (v8.4.0)
- Microsoft.Extensions.DependencyInjection (v9.0.0)
- ErrorOr (v2.0.1)
- FluentValidation (v11.11.0)

For development and testing:
- Bogus (v35.6.1)
- Testcontainers (v4.0.0)
- Moq (v4.20.72)
- TUnit (v0.6.89)
