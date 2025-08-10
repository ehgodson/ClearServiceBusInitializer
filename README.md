# ClearServiceBusInitializer

[![NuGet Version](https://img.shields.io/nuget/v/ClearServiceBusInitializer)](https://www.nuget.org/packages/ClearServiceBusInitializer)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

A lightweight, code-first Azure Service Bus provisioning tool designed for small teams to manage Service Bus resources across different environments. It ensures that your Service Bus queues, topics, subscriptions, and rules are in place before your application starts—just have your Service Bus connection string ready.

## Features

- **?? Code-First Configuration**: Define your Service Bus resources using a fluent, strongly-typed API
- **?? Automatic Provisioning**: Creates or updates Service Bus entities at application startup
- **?? Environment Agnostic**: Works across development, staging, and production environments
- **??? Zero Infrastructure**: No additional infrastructure tooling or manual setup required
- **?? Small Team Friendly**: Perfect for teams without dedicated DevOps resources
- **?? Lightweight**: Minimal dependencies and small footprint
- **?? Flexible Configuration**: Support for queues, topics, subscriptions, and message filters
- **?? Type Safe**: Strongly-typed configuration prevents runtime errors

## Quick Start

### 1. Install the Package

```bash
dotnet add package ClearServiceBusInitializer
```

### 2. Define Your Service Bus Context

Create a class that implements `IServiceBusContext` to define your messaging infrastructure:

```csharp
public class MyAppServiceBusContext : IServiceBusContext
{
    public string Name => "MyApp ServiceBus";

    public void BuildServiceBusResource(ServiceBusResource serviceBusResource)
    {
        // Define queues
        serviceBusResource.AddQueue("order-processing-queue");
        serviceBusResource.AddQueue("payment-queue");

        // Define topics with subscriptions and filters
        serviceBusResource.AddTopic("order-events")
            .AddSubscription("order-created-handler", "OrderCreated")
            .AddSubscription("order-updated-handler", "OrderUpdated")
            .AddSubscription("order-cancelled-handler", "OrderCancelled");

        serviceBusResource.AddTopic("payment-events")
            .AddSubscription("payment-processor", "PaymentStarted", "PaymentRetry")
            .AddSubscription("notification-service", "PaymentCompleted", "PaymentFailed");
    }
}
```

### 3. Register in Dependency Injection

In your `Program.cs`, register the context using the extension method:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register your Service Bus context
builder.Services.AddServiceBusContext<MyAppServiceBusContext>(
    builder.Configuration.GetConnectionString("ServiceBus"));

var app = builder.Build();

// Provision Service Bus resources at startup
await app.CreateServiceBusResource();

app.Run();
```

That's it! Your Service Bus resources will be automatically created or updated when your application starts.

## Advanced Configuration

### Custom Queue Configuration

```csharp
var queueOptions = new Queue.Option(
    DefaultTimeToLive: TimeSpan.FromDays(7),
    DuplicateDetectionWindow: TimeSpan.FromMinutes(10),
    EnableBatchedOperations: true,
    EnablePartitioning: false,
    AutoDeleteOnIdle: TimeSpan.FromHours(24)
);

serviceBusResource.AddQueue("priority-queue", queueOptions);
```

### Custom Topic Configuration

```csharp
var topicOptions = new Topic.Option(
    DefaultTimeToLive: TimeSpan.FromDays(14),
    DuplicateDetectionWindow: TimeSpan.FromMinutes(5),
    EnableBatchedOperations: true,
    EnablePartitioning: true,
    AutoDeleteOnIdle: null
);

serviceBusResource.AddTopic("events-topic", topicOptions);
```

### Custom Subscription Configuration

```csharp
var subscriptionOptions = new Subscription.Option(
    DefaultTimeToLive: TimeSpan.FromDays(7),
    DeadLetteringOnMessageExpiration: true,
    LockDuration: TimeSpan.FromMinutes(5),
    AutoDeleteOnIdle: TimeSpan.FromHours(2),
    RequiresSession: true,
    ForwardDeadLetterTo: "dead-letter-queue"
);

topic.AddSubscription("session-handler", subscriptionOptions, "SessionRequired");
```

### Custom SQL Filters

```csharp
// Using custom SQL expressions for complex filtering
subscription.Filters.Add(new Filter("CustomFilter", "MessageType = 'Order' AND Priority > 5"));
```

### Static Resource Provisioning

For scenarios where you need to provision resources without dependency injection:

```csharp
var serviceBusResource = new ServiceBusResource("MyApplication");
serviceBusResource.AddQueue("static-queue");
serviceBusResource.AddTopic("static-topic")
                 .AddSubscription("static-subscription", "StaticFilter");

await ServiceBusInitializer.CreateServiceBusResource(connectionString, serviceBusResource);
```

## Entity Naming Conventions

The library automatically applies prefixes to entity names for consistency:

- **Service Bus Resource**: `sb-` prefix (e.g., `sb-myapp`)
- **Topics**: `sbt-` prefix (e.g., `sbt-order-events`)
- **Queues**: `sbq-` prefix (e.g., `sbq-payment-queue`)
- **Subscriptions**: `sbs-` prefix (e.g., `sbs-order-handler`)
- **Filters/Rules**: `sbsr-` prefix (e.g., `sbsr-ordercreated`)

Names are automatically:
- Converted to lowercase
- Spaces replaced with hyphens
- Trimmed of whitespace
- Prefixed if not already prefixed

## Error Handling

The library includes specific exceptions for common scenarios:

```csharp
try
{
    await app.CreateServiceBusResource();
}
catch (ServiceBusAdministrationClientNotRegisteredException)
{
    // Handle missing admin client registration
}
catch (ServiceBusContextNotRegisteredException)
{
    // Handle missing context registration
}
```

## Best Practices

### 1. Environment-Specific Contexts

Create different contexts for different environments:

```csharp
public class DevelopmentServiceBusContext : IServiceBusContext
{
    public string Name => "Dev Environment";
    
    public void BuildServiceBusResource(ServiceBusResource resource)
    {
        // Minimal setup for development
        resource.AddQueue("dev-test-queue");
    }
}

public class ProductionServiceBusContext : IServiceBusContext
{
    public string Name => "Production Environment";
    
    public void BuildServiceBusResource(ServiceBusResource resource)
    {
        // Full production setup with custom configurations
        var prodQueueOptions = new Queue.Option(
            DefaultTimeToLive: TimeSpan.FromDays(14),
            EnableBatchedOperations: true,
            EnablePartitioning: true
        );
        
        resource.AddQueue("prod-orders", prodQueueOptions);
        // ... more production resources
    }
}
```

### 2. Modular Resource Definition

Break down large configurations into multiple methods:

```csharp
public class MyServiceBusContext : IServiceBusContext
{
    public string Name => "MyApp";

    public void BuildServiceBusResource(ServiceBusResource resource)
    {
        ConfigureOrderResources(resource);
        ConfigurePaymentResources(resource);
        ConfigureNotificationResources(resource);
    }

    private void ConfigureOrderResources(ServiceBusResource resource)
    {
        resource.AddQueue("order-processing");
        resource.AddTopic("order-events")
                .AddSubscription("order-created-handler", "OrderCreated")
                .AddSubscription("order-updated-handler", "OrderUpdated");
    }

    private void ConfigurePaymentResources(ServiceBusResource resource)
    {
        resource.AddQueue("payment-processing");
        resource.AddTopic("payment-events")
                .AddSubscription("payment-handler", "PaymentCompleted", "PaymentFailed");
    }

    private void ConfigureNotificationResources(ServiceBusResource resource)
    {
        resource.AddQueue("email-notifications");
        resource.AddQueue("sms-notifications");
    }
}
```

### 3. Configuration-Driven Setup

Use configuration to drive resource creation:

```csharp
public class ConfigurableServiceBusContext : IServiceBusContext
{
    private readonly IConfiguration _configuration;

    public ConfigurableServiceBusContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Name => _configuration["ServiceBus:Name"] ?? "DefaultApp";

    public void BuildServiceBusResource(ServiceBusResource resource)
    {
        var queues = _configuration.GetSection("ServiceBus:Queues").Get<string[]>() ?? [];
        foreach (var queue in queues)
        {
            resource.AddQueue(queue);
        }

        var topics = _configuration.GetSection("ServiceBus:Topics").Get<TopicConfig[]>() ?? [];
        foreach (var topicConfig in topics)
        {
            var topic = resource.AddTopic(topicConfig.Name);
            foreach (var sub in topicConfig.Subscriptions)
            {
                topic.AddSubscription(sub.Name, sub.Filters);
            }
        }
    }
}
```

## Performance Considerations

- **Startup Time**: Resource provisioning happens at startup and may add 1-3 seconds depending on the number of entities
- **Idempotent Operations**: The library checks existing resources and only creates/updates what's necessary
- **Batch Operations**: Enable batched operations on queues and topics for better throughput
- **Partitioning**: Consider enabling partitioning for high-throughput scenarios

## Troubleshooting

### Common Issues

1. **Connection String Issues**
   ```csharp
   // Ensure your connection string is valid
   "Endpoint=sb://yournamespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=yourkey"
   ```

2. **Permission Issues**
   - Ensure your connection string has management permissions
   - Verify the shared access key has "Manage" claims

3. **Resource Already Exists**
   - The library handles existing resources gracefully
   - It will update properties if they differ from the configuration

### Debug Logging

Enable debug logging to see what resources are being created:

```csharp
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

## Examples

Check out the [examples](examples/) directory for complete sample applications demonstrating various usage patterns.

## Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- ?? [Documentation](https://github.com/ehgodson/ClearServiceBusInitializer/wiki)
- ?? [Report Issues](https://github.com/ehgodson/ClearServiceBusInitializer/issues)
- ?? [Discussions](https://github.com/ehgodson/ClearServiceBusInitializer/discussions)

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for a list of changes and version history.