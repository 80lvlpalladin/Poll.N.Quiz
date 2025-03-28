# Poll.N.Quiz Solution

## Overview

Poll.N.Quiz is a .NET solution built with modern architecture principles. The system uses .NET Aspire for distributed application hosting and integrates several key technologies including MassTransit for messaging, MongoDB for storage, and Redis for caching.

## Architecture

The solution follows a microservices architecture with the following components:

- **API Layer**: RESTful endpoints for client communication
- **Settings Management**: Configuration system with event sourcing
- **Event-driven Architecture**: Using MassTransit and Kafka for message queuing

## Key Components

### Settings Service

- **Settings.API**: RESTful API for managing application settings
- **Settings.EventQueue**: Event-based messaging for settings changes
- **Settings.EventStore**: Event sourcing for settings
- **Settings.Projection**: Read models for settings data
- **Settings.FileStore**: File-based storage for settings
- **Settings.Synchronizer**: Keeps settings systems in sync

### Infrastructure

- **.NET Aspire**: Used for distributed application hosting
- **Scalar**: OpenAPI documentation
- **MassTransit**: Service bus implementation
- **MongoDB**: Document database storage
- **Redis**: Distributed caching
- **Kafka**: Event streaming platform

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
