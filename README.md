# Azure Service Bus Provisioning Tool

## Introduction

This tool automates the provisioning of Azure Service Bus resources—queues, topics, subscriptions, and rules—at application startup. It's designed for teams without dedicated infrastructure or DevOps support, where managing messaging resources across multiple environments can become repetitive and error-prone.

Ideal for microservices and distributed systems, this lightweight utility ensures that your application's required Service Bus entities are present and correctly configured before the app starts. Simply provide a connection string—no additional infrastructure tooling or manual setup needed.

---

## How It Works

This tool uses a code-first, fluent configuration approach to declare Service Bus resources. The provisioning process runs during application startup, validating and creating any missing entities automatically.

### 1. Define a Service Bus Context

Implement a class that defines your messaging infrastructure using the fluent API:

```csharp
public class ServiceBusContext : IServiceBusContext
{
    public string Name => "MyApp ServiceBus";

    public void BuildServiceBusResource(ServiceBusResource serviceBusResource)
    {
        // Define a queue
        serviceBusResource.AddQueue("Sample Queue 1");

        // Define a topic with multiple subscriptions
        serviceBusResource.AddTopic("Sample Topic 1")
            .AddSubscription("Sample Subscription 1", "Filter-1")
            .AddSubscription("Sample Subscription 2", "Filter-2")
            .AddSubscription("Sample Subscription 3", "Filter-3");

        // Define another topic
        serviceBusResource.AddTopic("Sample Topic 2")
            .AddSubscription("Sample Subscription 1", "Filter-1")
            .AddSubscription("Sample Subscription 2", "Filter-2", "Filter-3");
    }
}
```

Each method corresponds directly to Azure Service Bus entities, and SQL-like filters can be added to subscriptions for fine-grained message routing.

### 2. Register the Context in Your Application

In `Program.cs`, register the context using the provided extension method:

```csharp
builder.Services.AddServiceBusContext<ServiceBusContext>("your-service-bus-connection-string");
```

### 3. Provision Resources at Startup

Provision the declared Service Bus resources at app startup:

```csharp
await app.CreateServiceBusResource();
```

This step ensures that all required queues, topics, subscriptions, and rules are created or verified before the application starts processing messages.


---

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.