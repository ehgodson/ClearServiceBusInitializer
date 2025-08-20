# ClearServiceBusInitializer API Documentation

## Table of Contents

1. [Core Interfaces](#core-interfaces)
2. [Main Classes](#main-classes)
3. [Entity Classes](#entity-classes)
4. [Extension Methods](#extension-methods)
5. [Configuration Options](#configuration-options)
6. [Exception Classes](#exception-classes)
7. [Usage Patterns](#usage-patterns)

## Core Interfaces

### IServiceBusContext

The main interface for defining Service Bus resources in your application.

```csharp
public interface IServiceBusContext
{
    string Name { get; }
    void BuildServiceBusResource(ServiceBusResource serviceBusResource);
}
```

**Properties:**
- `Name`: A descriptive name for your Service Bus context

**Methods:**
- `BuildServiceBusResource(ServiceBusResource)`: Method where you define your Service Bus entities

**Example Implementation:**
```csharp
public class OrderServiceBusContext : IServiceBusContext
{
    public string Name => "Order Processing Service Bus";

    public void BuildServiceBusResource(ServiceBusResource serviceBusResource)
    {
        // Define your Service Bus resources here
        serviceBusResource.AddQueue("order-processing");
        serviceBusResource.AddTopic("order-events")
                         .AddSubscription("order-handler", "OrderCreated");
    }
}
```

## Main Classes

### ServiceBusInitializer

Static class providing extension methods for Service Bus setup and provisioning.

#### Methods

##### AddServiceBusContext<T>
```csharp
public static IServiceCollection AddServiceBusContext<T>(
    this IServiceCollection services, 
    string connectionString) 
    where T : class, IServiceBusContext
```

Registers a Service Bus context and administration client in the DI container.

**Parameters:**
- `services`: The service collection to register services in
- `connectionString`: Azure Service Bus connection string with management permissions

**Returns:** The same service collection for method chaining

**Example:**
```csharp
services.AddServiceBusContext<MyServiceBusContext>(connectionString);
```

##### CreateServiceBusResource (Extension Method)
```csharp
public static async Task<IHost> CreateServiceBusResource(this IHost builder)
```

Provisions Service Bus resources defined in the registered context.

**Parameters:**
- `builder`: The application host

**Returns:** The same host for method chaining

**Throws:**
- `ServiceBusAdministrationClientNotRegisteredException`: When admin client not registered
- `ServiceBusContextNotRegisteredException`: When context not registered

**Example:**
```csharp
await app.CreateServiceBusResource();
```

##### CreateServiceBusResource (Static Method)
```csharp
public static async Task CreateServiceBusResource(
    string connectionString,
    ServiceBusResource serviceBusResource)
```

Provisions a specific Service Bus resource without DI.

**Parameters:**
- `connectionString`: Azure Service Bus connection string
- `serviceBusResource`: The resource definition to provision

**Example:**
```csharp
var resource = new ServiceBusResource("MyApp");
resource.AddQueue("test-queue");
await ServiceBusInitializer.CreateServiceBusResource(connectionString, resource);
```

### ServiceBusProvisioner

Class responsible for the actual creation and updating of Service Bus entities.

#### Constructor
```csharp
public ServiceBusProvisioner(ServiceBusAdministrationClient adminClient)
```

**Parameters:**
- `adminClient`: Azure Service Bus administration client

#### Methods

##### EnsureTopicAsync
```csharp
public async Task EnsureTopicAsync(Topic topic)
```

Creates or updates a Service Bus topic.

**Parameters:**
- `topic`: The topic definition to ensure exists

**Behavior:**
- Creates topic if it doesn't exist
- Updates topic properties if they differ from configuration
- Handles duplicate detection, partitioning, TTL, and other settings

##### EnsureQueueAsync
```csharp
public async Task EnsureQueueAsync(Queue queue)
```

Creates or updates a Service Bus queue.

**Parameters:**
- `queue`: The queue definition to ensure exists

**Behavior:**
- Creates queue if it doesn't exist
- Updates queue properties if they differ from configuration
- Handles duplicate detection, partitioning, TTL, and other settings

##### EnsureSubscriptionAsync
```csharp
public async Task EnsureSubscriptionAsync(Subscription subscription, Topic topic)
```

Creates or updates a Service Bus subscription and its filters.

**Parameters:**
- `subscription`: The subscription definition to ensure exists
- `topic`: The parent topic for the subscription

**Behavior:**
- Creates subscription if it doesn't exist
- Updates subscription properties if they differ
- Removes default rule and creates custom filters
- Handles sessions, dead lettering, lock duration, and other settings

##### DeleteTopicAsync
```csharp
public async Task DeleteTopicAsync(string topicName)
```

Deletes a Service Bus topic if it exists.

**Parameters:**
- `topicName`: The name of the topic to delete

**Behavior:**
- Checks if topic exists before attempting deletion
- Deletes topic and all associated subscriptions and filters
- Operation is idempotent - no error if topic doesn't exist

##### DeleteQueueAsync
```csharp
public async Task DeleteQueueAsync(string queueName)
```

Deletes a Service Bus queue if it exists.

**Parameters:**
- `queueName`: The name of the queue to delete

**Behavior:**
- Checks if queue exists before attempting deletion
- Operation is idempotent - no error if queue doesn't exist

##### DeleteSubscriptionAsync
```csharp
public async Task DeleteSubscriptionAsync(string topicName, string subscriptionName)
```

Deletes a Service Bus subscription if it exists.

**Parameters:**
- `topicName`: The name of the parent topic
- `subscriptionName`: The name of the subscription to delete

**Behavior:**
- Checks if subscription exists before attempting deletion
- Deletes subscription and all associated filters
- Operation is idempotent - no error if subscription doesn't exist

##### DeleteFilterAsync
```csharp
public async Task DeleteFilterAsync(string topicName, string subscriptionName, string filterName)
```

Deletes a message filter/rule from a subscription if it exists.

**Parameters:**
- `topicName`: The name of the parent topic
- `subscriptionName`: The name of the parent subscription
- `filterName`: The name of the filter/rule to delete

**Behavior:**
- Checks if filter exists before attempting deletion
- Operation is idempotent - no error if filter doesn't exist

## Entity Classes

### ServiceBusResource

Root container for all Service Bus entities.

```csharp
public record ServiceBusResource(string Name) : EntityWithPrefix("sb-", Name)
```

**Properties:**
- `Name`: Auto-prefixed name of the resource
- `Topics`: Collection of topics in this resource
- `Queues`: Collection of queues in this resource

**Methods:**

#### AddTopic
```csharp
public Topic AddTopic(string name)
public Topic AddTopic(string name, Topic.Option options)
```

Adds a topic to the resource.

**Example:**
```csharp
var topic = resource.AddTopic("events");
var customTopic = resource.AddTopic("priority-events", new Topic.Option(
    DefaultTimeToLive: TimeSpan.FromDays(7),
    EnablePartitioning: true
));
```

#### AddQueue
```csharp
public Queue AddQueue(string name)
public Queue AddQueue(string name, Queue.Option options)
```

Adds a queue to the resource.

**Example:**
```csharp
var queue = resource.AddQueue("processing");
var customQueue = resource.AddQueue("priority-processing", new Queue.Option(
    DefaultTimeToLive: TimeSpan.FromDays(7),
    EnablePartitioning: true
));
```

### Topic

Represents a Service Bus topic.

```csharp
public record Topic : EntityWithPrefix
```

**Constructor:**
```csharp
public Topic(string name, Option? options = null) : base("sbt-", name)
```

**Properties:**
- `Name`: Auto-prefixed topic name (sbt-*)
- `Subscriptions`: Collection of subscriptions
- `Options`: Configuration options

**Methods:**

#### AddSubscription
```csharp
public Topic AddSubscription(string name, params List<string> filters)
public Topic AddSubscription(string name, Subscription.Option options, params List<string> filters)
public Topic AddSubscription(string name, params List<Filter> filters)
public Topic AddSubscription(string name, Subscription.Option options, params List<Filter> filters)
```

Adds a subscription with filters to the topic.

**Example:**
```csharp
// Using string-based label filters
topic.AddSubscription("order-handler", "OrderCreated", "OrderUpdated")
     .AddSubscription("audit-handler", auditOptions, "OrderCreated", "OrderDeleted");

// Using Filter objects (v1.2.0+)
var priorityFilter = Filter.Create("HighPriority", "Priority", 5);
var statusFilter = Filter.Create("ActiveStatus", "Status", "Active");
topic.AddSubscription("priority-handler", priorityFilter, statusFilter)
     .AddSubscription("complex-handler", customOptions, priorityFilter, statusFilter);
```

#### Topic.Option
Configuration options for topics.

```csharp
public record Option(
    TimeSpan DefaultTimeToLive,
    TimeSpan? DuplicateDetectionWindow,
    bool EnableBatchedOperations,
    bool EnablePartitioning,
    TimeSpan? AutoDeleteOnIdle)
```

**Default Values:**
```csharp
public static Option CreateDefault() => new(
    DefaultTimeToLive: TimeSpan.FromDays(14),
    DuplicateDetectionWindow: null,
    EnableBatchedOperations: true,
    EnablePartitioning: false,
    AutoDeleteOnIdle: null
);
```

### Queue

Represents a Service Bus queue.

```csharp
public record Queue : EntityWithPrefix
```

**Constructor:**
```csharp
public Queue(string name, Option? options = null) : base("sbq-", name)
```

**Properties:**
- `Name`: Auto-prefixed queue name (sbq-*)
- `Options`: Configuration options

#### Queue.Option
Configuration options for queues.

```csharp
public record Option(
    TimeSpan DefaultTimeToLive,
    TimeSpan? DuplicateDetectionWindow,
    bool EnableBatchedOperations,
    bool EnablePartitioning,
    TimeSpan? AutoDeleteOnIdle)
```

**Default Values:**
```csharp
public static Option CreateDefault() => new(
    DefaultTimeToLive: TimeSpan.FromDays(14),
    DuplicateDetectionWindow: null,
    EnableBatchedOperations: true,
    EnablePartitioning: false,
    AutoDeleteOnIdle: null
);
```

### Subscription

Represents a Service Bus subscription.

```csharp
public record Subscription : EntityWithPrefix
```

**Constructors:**
```csharp
public Subscription(string name, params List<string> filters) : base("sbs-", name)
public Subscription(string name, Option options, params List<string> filters) : base("sbs-", name)
public Subscription(string name, params List<Filter> filters) : base("sbs-", name)
public Subscription(string name, Option options, params List<Filter> filters) : base("sbs-", name)
```

**Properties:**
- `Name`: Auto-prefixed subscription name (sbs-*)
- `Filters`: Collection of message filters
- `Options`: Configuration options

**Methods:**

#### AddLabelFilter
```csharp
public Subscription AddLabelFilter(string name)
```

Adds a label-based filter to the subscription.

**Example:**
```csharp
subscription.AddLabelFilter("HighPriority")
           .AddLabelFilter("UrgentProcessing");
```

#### AddFilter
```csharp
public Subscription AddFilter(Filter filter)
```

Adds a custom filter to the subscription.

**Example:**
```csharp
var priorityFilter = Filter.Create("HighPriority", "Priority", 5);
var statusFilter = Filter.Create("ActiveStatus", "Status", "Active");

subscription.AddFilter(priorityFilter)
           .AddFilter(statusFilter)
           .AddLabelFilter("OrderCreated");
```

#### Subscription.Option
Configuration options for subscriptions.

```csharp
public record Option(
    TimeSpan DefaultTimeToLive,
    bool DeadLetteringOnMessageExpiration,
    TimeSpan? LockDuration,
    TimeSpan? AutoDeleteOnIdle,
    bool RequiresSession,
    string? ForwardDeadLetterTo)
```

**Default Values:**
```csharp
public static Option CreateDefault() => new(
    DefaultTimeToLive: TimeSpan.FromDays(14),
    DeadLetteringOnMessageExpiration: true,
    LockDuration: TimeSpan.FromMinutes(1),
    AutoDeleteOnIdle: null,
    RequiresSession: false,
    ForwardDeadLetterTo: null
);
```

### Filter

Represents a message filter/rule for subscriptions.

```csharp
public record Filter(string Name, string SqlExpression) : EntityWithPrefix("sbsr-", Name)
```

**Properties:**
- `Name`: Auto-prefixed filter name (sbsr-*)
- `SqlExpression`: SQL filter expression

**Static Methods:**

#### CreateLabel
```csharp
public static Filter CreateLabel(string label)
```

Creates a label-based filter.

**Example:**
```csharp
var filter = Filter.CreateLabel("OrderCreated");
// Creates: Name = "sbsr-ordercreated", SqlExpression = "sys.Label='OrderCreated'"
```

#### Create (String Value)
```csharp
public static Filter Create(string name, string key, string value)
```

Creates a custom SQL filter with a string value.

**Example:**
```csharp
var filter = Filter.Create("ActiveStatus", "Status", "Active");
// Creates: Name = "sbsr-activestatus", SqlExpression = "Status='Active'"
```

#### Create (Integer Value)
```csharp
public static Filter Create(string name, string key, int value)
```

Creates a custom SQL filter with an integer value.

**Example:**
```csharp
var filter = Filter.Create("HighPriority", "Priority", 5);
// Creates: Name = "sbsr-highpriority", SqlExpression = "Priority=5"
```

### EntityWithPrefix

Abstract base record for all Service Bus entities that handles naming conventions.

```csharp
public abstract record EntityWithPrefix
```

**Constructor:**
```csharp
protected EntityWithPrefix(string prefix, string name)
```

**Properties:**
- `Name`: The final prefixed and normalized name

**Naming Rules:**
1. Converts to lowercase
2. Replaces spaces with hyphens
3. Trims whitespace
4. Adds prefix if not already present

**Examples:**
```csharp
new Queue("My Order Queue") ? Name: "sbq-my-order-queue"
new Topic("Event Processing") ? Name: "sbt-event-processing"
new Subscription(" User Events ") ? Name: "sbs-user-events"
```

## Extension Methods

### IServiceCollection Extensions

#### AddServiceBusContext<T>
```csharp
public static IServiceCollection AddServiceBusContext<T>(
    this IServiceCollection services, 
    string connectionString) 
    where T : class, IServiceBusContext
```

Registers the Service Bus context and administration client as singletons.

### IHost Extensions

#### CreateServiceBusResource
```csharp
public static async Task<IHost> CreateServiceBusResource(this IHost builder)
```

Provisions all Service Bus resources defined in the registered context.

## Configuration Options

### Connection String Format
```
Endpoint=sb://<namespace>.servicebus.windows.net/;SharedAccessKeyName=<keyname>;SharedAccessKey=<key>
```

**Required Permissions:**
- Manage (for creating/updating resources)
- Send/Listen permissions are inherited

### Environment Variables
```bash
# Connection string
SERVICEBUS_CONNECTION_STRING="Endpoint=sb://..."

# Or individual components
SERVICEBUS_NAMESPACE="your-namespace"
SERVICEBUS_KEY_NAME="RootManageSharedAccessKey"
SERVICEBUS_KEY_VALUE="your-access-key"
```

### Configuration Examples

#### appsettings.json
```json
{
  "ConnectionStrings": {
    "ServiceBus": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key"
  },
  "ServiceBus": {
    "Name": "MyApplication",
    "DefaultTtl": "14.00:00:00",
    "EnablePartitioning": false
  }
}
```

## Exception Classes

### DomainException
Base exception class for all library-specific exceptions.

```csharp
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}
```

### ServiceBusAdministrationClientNotRegisteredException
Thrown when the administration client is not registered in DI.

```csharp
public class ServiceBusAdministrationClientNotRegisteredException : DomainException
```

**Message:** "ServiceBusAdministrationClient not registered in the service collection."

### ServiceBusContextNotRegisteredException
Thrown when the Service Bus context is not registered in DI.

```csharp
public class ServiceBusContextNotRegisteredException : DomainException
```

**Message:** "ServiceBusContext not registered in the service collection."

## Usage Patterns

### Pattern 1: Basic Web Application
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceBusContext<MyServiceBusContext>(
    builder.Configuration.GetConnectionString("ServiceBus"));

var app = builder.Build();

await app.CreateServiceBusResource();
app.Run();
```

### Pattern 2: Worker Service
```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddServiceBusContext<WorkerServiceBusContext>(
    builder.Configuration.GetConnectionString("ServiceBus"));

var host = builder.Build();

await host.CreateServiceBusResource();
await host.RunAsync();
```

### Pattern 3: Console Application
```csharp
var services = new ServiceCollection();
services.AddServiceBusContext<ConsoleServiceBusContext>(connectionString);

var provider = services.BuildServiceProvider();
var host = new HostBuilder()
    .ConfigureServices(s => s.AddSingleton(provider))
    .Build();

await host.CreateServiceBusResource();
```

### Pattern 4: Static Usage
```csharp
var resource = new ServiceBusResource("MyConsoleApp");
resource.AddQueue("console-queue");
resource.AddTopic("console-events")
        .AddSubscription("console-handler", "ConsoleEvent");

await ServiceBusInitializer.CreateServiceBusResource(connectionString, resource);
```

### Pattern 5: Environment-Specific Configuration
```csharp
public class ServiceBusContextFactory
{
    public static IServiceBusContext Create(IWebHostEnvironment env)
    {
        return env.EnvironmentName switch
        {
            "Development" => new DevelopmentServiceBusContext(),
            "Staging" => new StagingServiceBusContext(),
            "Production" => new ProductionServiceBusContext(),
            _ => throw new InvalidOperationException($"Unknown environment: {env.EnvironmentName}")
        };
    }
}

// In Program.cs
var contextType = ServiceBusContextFactory.Create(builder.Environment).GetType();
builder.Services.AddServiceBusContext(contextType, connectionString);
```

### Pattern 6: Configuration-Driven Setup
```csharp
public class ConfigurableServiceBusContext : IServiceBusContext
{
    private readonly ServiceBusConfig _config;

    public ConfigurableServiceBusContext(IOptions<ServiceBusConfig> config)
    {
        _config = config.Value;
    }

    public string Name => _config.Name;

    public void BuildServiceBusResource(ServiceBusResource resource)
    {
        foreach (var queueConfig in _config.Queues)
        {
            resource.AddQueue(queueConfig.Name, CreateQueueOptions(queueConfig));
        }

        foreach (var topicConfig in _config.Topics)
        {
            var topic = resource.AddTopic(topicConfig.Name, CreateTopicOptions(topicConfig));
            foreach (var subConfig in topicConfig.Subscriptions)
            {
                topic.AddSubscription(subConfig.Name, CreateSubscriptionOptions(subConfig), subConfig.Filters.ToArray());
            }
        }
    }
}
```

This comprehensive API documentation covers all the public interfaces, classes, and usage patterns available in the ClearServiceBusInitializer library. Use this as a reference for implementing Service Bus resource provisioning in your applications.