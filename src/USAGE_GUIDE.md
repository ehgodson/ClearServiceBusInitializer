# ClearServiceBusInitializer Usage Guide

This guide provides comprehensive examples and patterns for using ClearServiceBusInitializer in various scenarios.

## Table of Contents

1. [Getting Started](#getting-started)
2. [Basic Examples](#basic-examples)
3. [Advanced Configuration](#advanced-configuration)
4. [Real-World Scenarios](#real-world-scenarios)
5. [Best Practices](#best-practices)
6. [Troubleshooting](#troubleshooting)

## Getting Started

### Installation

```bash
dotnet add package ClearServiceBusInitializer
```

### Basic Setup

1. **Create a Service Bus Context**
2. **Register in DI Container**
3. **Provision at Startup**

## Basic Examples

### Example 1: Simple E-commerce Application

```csharp
public class EcommerceServiceBusContext : IServiceBusContext
{
    public string Name => "E-commerce Platform";

    public void BuildServiceBusResource(ServiceBusResource serviceBusResource)
    {
        // Order processing queues
        serviceBusResource.AddQueue("order-processing");
        serviceBusResource.AddQueue("payment-processing");
        serviceBusResource.AddQueue("inventory-updates");

        // Event-driven architecture with topics
        serviceBusResource.AddTopic("order-events")
            .AddSubscription("order-fulfillment", "OrderCreated", "OrderPaid")
            .AddSubscription("inventory-service", "OrderCreated", "OrderCancelled")
            .AddSubscription("analytics", "OrderCreated", "OrderCompleted", "OrderCancelled");

        serviceBusResource.AddTopic("payment-events")
            .AddSubscription("accounting", "PaymentCompleted", "PaymentFailed")
            .AddSubscription("notification-service", "PaymentCompleted")
            .AddSubscription("fraud-detection", "PaymentFailed");
    }
}

// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceBusContext<EcommerceServiceBusContext>(
    builder.Configuration.GetConnectionString("ServiceBus"));

var app = builder.Build();

await app.CreateServiceBusResource();
app.Run();
```

### Example 2: Microservices Communication

```csharp
public class MicroservicesServiceBusContext : IServiceBusContext
{
    public string Name => "Microservices Platform";

    public void BuildServiceBusResource(ServiceBusResource serviceBusResource)
    {
        // Service-to-service communication queues
        serviceBusResource.AddQueue("user-service-commands");
        serviceBusResource.AddQueue("product-service-commands");
        serviceBusResource.AddQueue("order-service-commands");

        // Cross-cutting concerns
        serviceBusResource.AddQueue("audit-events");
        serviceBusResource.AddQueue("error-handling");

        // Domain events
        ConfigureUserEvents(serviceBusResource);
        ConfigureProductEvents(serviceBusResource);
        ConfigureOrderEvents(serviceBusResource);
    }

    private void ConfigureUserEvents(ServiceBusResource resource)
    {
        resource.AddTopic("user-events")
            .AddSubscription("profile-service", "UserRegistered", "UserUpdated")
            .AddSubscription("email-service", "UserRegistered", "UserDeleted")
            .AddSubscription("analytics-service", "UserRegistered", "UserLoggedIn");
    }

    private void ConfigureProductEvents(ServiceBusResource resource)
    {
        resource.AddTopic("product-events")
            .AddSubscription("inventory-service", "ProductCreated", "ProductUpdated")
            .AddSubscription("search-service", "ProductCreated", "ProductUpdated", "ProductDeleted")
            .AddSubscription("recommendation-service", "ProductViewed", "ProductPurchased");
    }

    private void ConfigureOrderEvents(ServiceBusResource resource)
    {
        resource.AddTopic("order-events")
            .AddSubscription("payment-service", "OrderCreated")
            .AddSubscription("shipping-service", "OrderPaid")
            .AddSubscription("notification-service", "OrderShipped", "OrderDelivered");
    }
}
```

### Example 3: IoT Data Processing

```csharp
public class IoTServiceBusContext : IServiceBusContext
{
    public string Name => "IoT Data Platform";

    public void BuildServiceBusResource(ServiceBusResource serviceBusResource)
    {
        // High-throughput queues with partitioning
        var highThroughputOptions = new Queue.Option(
            DefaultTimeToLive: TimeSpan.FromDays(7),
            DuplicateDetectionWindow: null,
            EnableBatchedOperations: true,
            EnablePartitioning: true,
            AutoDeleteOnIdle: null
        );

        serviceBusResource.AddQueue("sensor-data-raw", highThroughputOptions);
        serviceBusResource.AddQueue("telemetry-processing", highThroughputOptions);

        // Alert processing with sessions
        var sessionOptions = new Queue.Option(
            DefaultTimeToLive: TimeSpan.FromDays(1),
            DuplicateDetectionWindow: TimeSpan.FromMinutes(5),
            EnableBatchedOperations: true,
            EnablePartitioning: false,
            AutoDeleteOnIdle: null
        );

        serviceBusResource.AddQueue("critical-alerts", sessionOptions);

        // Device management events
        serviceBusResource.AddTopic("device-events")
            .AddSubscription("device-management", "DeviceConnected", "DeviceDisconnected")
            .AddSubscription("monitoring", "DeviceError", "DeviceMaintenance")
            .AddSubscription("analytics", "DeviceConnected", "DeviceDisconnected", "DataReceived");

        // Data processing pipeline
        serviceBusResource.AddTopic("data-pipeline")
            .AddSubscription("data-validation", "DataReceived")
            .AddSubscription("data-enrichment", "DataValidated")
            .AddSubscription("data-storage", "DataEnriched")
            .AddSubscription("real-time-analytics", "DataReceived", "DataValidated");
    }
}
```

## Advanced Configuration

### Custom Configuration Options

```csharp
public class AdvancedServiceBusContext : IServiceBusContext
{
    public string Name => "Advanced Configuration Example";

    public void BuildServiceBusResource(ServiceBusResource serviceBusResource)
    {
        // Long-running process queue with extended TTL
        var longRunningOptions = new Queue.Option(
            DefaultTimeToLive: TimeSpan.FromDays(30),
            DuplicateDetectionWindow: TimeSpan.FromHours(1),
            EnableBatchedOperations: true,
            EnablePartitioning: false,
            AutoDeleteOnIdle: TimeSpan.FromDays(7)
        );

        serviceBusResource.AddQueue("long-running-processes", longRunningOptions);

        // High-frequency topic with duplicate detection
        var highFrequencyOptions = new Topic.Option(
            DefaultTimeToLive: TimeSpan.FromDays(7),
            DuplicateDetectionWindow: TimeSpan.FromMinutes(1),
            EnableBatchedOperations: true,
            EnablePartitioning: true,
            AutoDeleteOnIdle: null
        );

        var topic = serviceBusResource.AddTopic("high-frequency-events", highFrequencyOptions);

        // Session-enabled subscription with custom lock duration
        var sessionSubscriptionOptions = new Subscription.Option(
            DefaultTimeToLive: TimeSpan.FromDays(14),
            DeadLetteringOnMessageExpiration: true,
            LockDuration: TimeSpan.FromMinutes(5),
            AutoDeleteOnIdle: null,
            RequiresSession: true,
            ForwardDeadLetterTo: "dead-letter-queue"
        );

        topic.AddSubscription("session-processor", sessionSubscriptionOptions, "SessionRequired");

        // Auto-scaling subscription with short lock duration
        var autoScaleOptions = new Subscription.Option(
            DefaultTimeToLive: TimeSpan.FromDays(7),
            DeadLetteringOnMessageExpiration: true,
            LockDuration: TimeSpan.FromSeconds(30),
            AutoDeleteOnIdle: TimeSpan.FromHours(1),
            RequiresSession: false,
            ForwardDeadLetterTo: null
        );

        topic.AddSubscription("auto-scale-processor", autoScaleOptions, "HighVolume");

        // Dead letter queue
        serviceBusResource.AddQueue("dead-letter-queue");
    }
}
```

### Environment-Specific Configuration

```csharp
public abstract class BaseServiceBusContext : IServiceBusContext
{
    public abstract string Name { get; }

    public virtual void BuildServiceBusResource(ServiceBusResource serviceBusResource)
    {
        ConfigureCommonResources(serviceBusResource);
        ConfigureEnvironmentSpecificResources(serviceBusResource);
    }

    protected virtual void ConfigureCommonResources(ServiceBusResource resource)
    {
        // Resources common to all environments
        resource.AddQueue("health-checks");
        resource.AddQueue("error-logs");
        
        resource.AddTopic("system-events")
            .AddSubscription("monitoring", "SystemStart", "SystemStop");
    }

    protected abstract void ConfigureEnvironmentSpecificResources(ServiceBusResource resource);
}

public class DevelopmentServiceBusContext : BaseServiceBusContext
{
    public override string Name => "Development Environment";

    protected override void ConfigureEnvironmentSpecificResources(ServiceBusResource resource)
    {
        // Development-specific resources with shorter TTLs
        var devOptions = new Queue.Option(
            DefaultTimeToLive: TimeSpan.FromDays(1),
            DuplicateDetectionWindow: null,
            EnableBatchedOperations: false,
            EnablePartitioning: false,
            AutoDeleteOnIdle: TimeSpan.FromHours(4)
        );

        resource.AddQueue("dev-testing", devOptions);
        resource.AddQueue("dev-debugging", devOptions);

        resource.AddTopic("dev-events")
            .AddSubscription("dev-logger", "DebugEvent", "TestEvent");
    }
}

public class ProductionServiceBusContext : BaseServiceBusContext
{
    public override string Name => "Production Environment";

    protected override void ConfigureEnvironmentSpecificResources(ServiceBusResource resource)
    {
        // Production resources with high availability settings
        var prodQueueOptions = new Queue.Option(
            DefaultTimeToLive: TimeSpan.FromDays(14),
            DuplicateDetectionWindow: TimeSpan.FromMinutes(10),
            EnableBatchedOperations: true,
            EnablePartitioning: true,
            AutoDeleteOnIdle: null
        );

        var prodTopicOptions = new Topic.Option(
            DefaultTimeToLive: TimeSpan.FromDays(14),
            DuplicateDetectionWindow: TimeSpan.FromMinutes(10),
            EnableBatchedOperations: true,
            EnablePartitioning: true,
            AutoDeleteOnIdle: null
        );

        resource.AddQueue("order-processing", prodQueueOptions);
        resource.AddQueue("payment-processing", prodQueueOptions);

        var orderTopic = resource.AddTopic("order-events", prodTopicOptions);
        
        var criticalSubOptions = new Subscription.Option(
            DefaultTimeToLive: TimeSpan.FromDays(14),
            DeadLetteringOnMessageExpiration: true,
            LockDuration: TimeSpan.FromMinutes(2),
            AutoDeleteOnIdle: null,
            RequiresSession: false,
            ForwardDeadLetterTo: "critical-dlq"
        );

        orderTopic.AddSubscription("order-fulfillment", criticalSubOptions, "OrderCreated", "OrderPaid");
        
        resource.AddQueue("critical-dlq", prodQueueOptions);
    }
}
```

### Configuration-Driven Setup

```csharp
public class ServiceBusConfig
{
    public string Name { get; set; } = "";
    public List<QueueConfig> Queues { get; set; } = new();
    public List<TopicConfig> Topics { get; set; } = new();
}

public class QueueConfig
{
    public string Name { get; set; } = "";
    public int TtlDays { get; set; } = 14;
    public bool EnablePartitioning { get; set; } = false;
    public bool EnableBatchedOperations { get; set; } = true;
    public int? AutoDeleteHours { get; set; }
    public int? DuplicateDetectionMinutes { get; set; }
}

public class TopicConfig
{
    public string Name { get; set; } = "";
    public int TtlDays { get; set; } = 14;
    public bool EnablePartitioning { get; set; } = false;
    public bool EnableBatchedOperations { get; set; } = true;
    public List<SubscriptionConfig> Subscriptions { get; set; } = new();
}

public class SubscriptionConfig
{
    public string Name { get; set; } = "";
    public List<string> Filters { get; set; } = new();
    public int TtlDays { get; set; } = 14;
    public int LockDurationSeconds { get; set; } = 60;
    public bool RequiresSession { get; set; } = false;
    public bool DeadLetteringOnExpiration { get; set; } = true;
    public string? ForwardDeadLetterTo { get; set; }
}

public class ConfigurableServiceBusContext : IServiceBusContext
{
    private readonly ServiceBusConfig _config;

    public ConfigurableServiceBusContext(IOptions<ServiceBusConfig> config)
    {
        _config = config.Value;
    }

    public string Name => _config.Name;

    public void BuildServiceBusResource(ServiceBusResource serviceBusResource)
    {
        // Configure queues from configuration
        foreach (var queueConfig in _config.Queues)
        {
            var options = new Queue.Option(
                DefaultTimeToLive: TimeSpan.FromDays(queueConfig.TtlDays),
                DuplicateDetectionWindow: queueConfig.DuplicateDetectionMinutes.HasValue 
                    ? TimeSpan.FromMinutes(queueConfig.DuplicateDetectionMinutes.Value) 
                    : null,
                EnableBatchedOperations: queueConfig.EnableBatchedOperations,
                EnablePartitioning: queueConfig.EnablePartitioning,
                AutoDeleteOnIdle: queueConfig.AutoDeleteHours.HasValue 
                    ? TimeSpan.FromHours(queueConfig.AutoDeleteHours.Value) 
                    : null
            );

            serviceBusResource.AddQueue(queueConfig.Name, options);
        }

        // Configure topics from configuration
        foreach (var topicConfig in _config.Topics)
        {
            var topicOptions = new Topic.Option(
                DefaultTimeToLive: TimeSpan.FromDays(topicConfig.TtlDays),
                DuplicateDetectionWindow: null,
                EnableBatchedOperations: topicConfig.EnableBatchedOperations,
                EnablePartitioning: topicConfig.EnablePartitioning,
                AutoDeleteOnIdle: null
            );

            var topic = serviceBusResource.AddTopic(topicConfig.Name, topicOptions);

            foreach (var subConfig in topicConfig.Subscriptions)
            {
                var subOptions = new Subscription.Option(
                    DefaultTimeToLive: TimeSpan.FromDays(subConfig.TtlDays),
                    DeadLetteringOnMessageExpiration: subConfig.DeadLetteringOnExpiration,
                    LockDuration: TimeSpan.FromSeconds(subConfig.LockDurationSeconds),
                    AutoDeleteOnIdle: null,
                    RequiresSession: subConfig.RequiresSession,
                    ForwardDeadLetterTo: subConfig.ForwardDeadLetterTo
                );

                topic.AddSubscription(subConfig.Name, subOptions, subConfig.Filters);
            }
        }
    }
}

// appsettings.json
{
  "ServiceBusConfig": {
    "Name": "Configurable Service Bus",
    "Queues": [
      {
        "Name": "orders",
        "TtlDays": 7,
        "EnablePartitioning": true,
        "EnableBatchedOperations": true
      },
      {
        "Name": "payments",
        "TtlDays": 14,
        "DuplicateDetectionMinutes": 10
      }
    ],
    "Topics": [
      {
        "Name": "events",
        "TtlDays": 7,
        "EnablePartitioning": true,
        "Subscriptions": [
          {
            "Name": "processor",
            "Filters": ["OrderCreated", "OrderUpdated"],
            "LockDurationSeconds": 120
          },
          {
            "Name": "audit",
            "Filters": ["OrderCreated", "OrderDeleted"],
            "RequiresSession": true
          }
        ]
      }
    ]
  }
}

// Registration
builder.Services.Configure<ServiceBusConfig>(
    builder.Configuration.GetSection("ServiceBusConfig"));
builder.Services.AddServiceBusContext<ConfigurableServiceBusContext>(connectionString);
```

## Real-World Scenarios

### Scenario 1: E-commerce Order Processing

```csharp
public class OrderProcessingContext : IServiceBusContext
{
    public string Name => "Order Processing System";

    public void BuildServiceBusResource(ServiceBusResource resource)
    {
        // Command queues for different services
        resource.AddQueue("inventory-commands");
        resource.AddQueue("payment-commands");
        resource.AddQueue("shipping-commands");
        resource.AddQueue("notification-commands");

        // Order lifecycle events
        var orderTopic = resource.AddTopic("order-lifecycle");
        
        // Inventory management subscriptions
        orderTopic.AddSubscription("inventory-reservation", "OrderCreated")
                  .AddSubscription("inventory-release", "OrderCancelled", "OrderExpired");

        // Payment processing subscriptions
        orderTopic.AddSubscription("payment-processing", "OrderConfirmed")
                  .AddSubscription("payment-refund", "OrderCancelled");

        // Shipping and fulfillment
        orderTopic.AddSubscription("shipping-preparation", "PaymentCompleted")
                  .AddSubscription("fulfillment-tracking", "OrderShipped");

        // Customer notifications
        orderTopic.AddSubscription("customer-notifications", 
            "OrderCreated", "OrderConfirmed", "OrderShipped", "OrderDelivered");

        // Analytics and reporting
        orderTopic.AddSubscription("analytics", 
            "OrderCreated", "OrderCompleted", "OrderCancelled");

        // Audit trail
        orderTopic.AddSubscription("audit-trail", 
            "OrderCreated", "OrderModified", "OrderCompleted", "OrderCancelled");
    }
}
```

### Scenario 2: Financial Services Transaction Processing

```csharp
public class FinancialServicesContext : IServiceBusContext
{
    public string Name => "Financial Services Platform";

    public void BuildServiceBusResource(ServiceBusResource resource)
    {
        // High-security, session-based queues for transactions
        var secureOptions = new Queue.Option(
            DefaultTimeToLive: TimeSpan.FromDays(30),
            DuplicateDetectionWindow: TimeSpan.FromMinutes(1),
            EnableBatchedOperations: true,
            EnablePartitioning: false,
            AutoDeleteOnIdle: null
        );

        resource.AddQueue("transaction-processing", secureOptions);
        resource.AddQueue("fraud-detection", secureOptions);
        resource.AddQueue("compliance-checking", secureOptions);

        // Real-time transaction monitoring
        var monitoringTopic = resource.AddTopic("transaction-monitoring");

        var sessionSubOptions = new Subscription.Option(
            DefaultTimeToLive: TimeSpan.FromDays(30),
            DeadLetteringOnMessageExpiration: true,
            LockDuration: TimeSpan.FromMinutes(5),
            AutoDeleteOnIdle: null,
            RequiresSession: true,
            ForwardDeadLetterTo: "compliance-dlq"
        );

        // Regulatory compliance - requires sessions for ordered processing
        monitoringTopic.AddSubscription("regulatory-reporting", sessionSubOptions,
            "TransactionCreated", "TransactionCompleted", "TransactionFailed");

        // Risk management
        monitoringTopic.AddSubscription("risk-assessment",
            "TransactionCreated", "LargeTransaction", "SuspiciousActivity");

        // Customer notifications
        monitoringTopic.AddSubscription("customer-alerts",
            "TransactionCompleted", "TransactionFailed", "SuspiciousActivity");

        // Audit and compliance dead letter queue
        resource.AddQueue("compliance-dlq", secureOptions);
    }
}
```

### Scenario 3: Healthcare System Integration

```csharp
public class HealthcareIntegrationContext : IServiceBusContext
{
    public string Name => "Healthcare Integration Platform";

    public void BuildServiceBusResource(ServiceBusResource resource)
    {
        // HIPAA-compliant processing with encryption and audit trails
        var hipaaOptions = new Queue.Option(
            DefaultTimeToLive: TimeSpan.FromDays(2555), // 7 years for medical records
            DuplicateDetectionWindow: TimeSpan.FromMinutes(5),
            EnableBatchedOperations: true,
            EnablePartitioning: false,
            AutoDeleteOnIdle: null
        );

        // Patient data processing
        resource.AddQueue("patient-data-ingestion", hipaaOptions);
        resource.AddQueue("hl7-message-processing", hipaaOptions);
        resource.AddQueue("fhir-resource-processing", hipaaOptions);

        // Clinical workflow topics
        var clinicalTopic = resource.AddTopic("clinical-events");

        var auditSubOptions = new Subscription.Option(
            DefaultTimeToLive: TimeSpan.FromDays(2555),
            DeadLetteringOnMessageExpiration: true,
            LockDuration: TimeSpan.FromMinutes(10),
            AutoDeleteOnIdle: null,
            RequiresSession: false,
            ForwardDeadLetterTo: "audit-dlq"
        );

        // Audit trail for all clinical activities
        clinicalTopic.AddSubscription("clinical-audit", auditSubOptions,
            "PatientAdmitted", "PatientDischarged", "PrescriptionCreated", 
            "LabResultAvailable", "DiagnosisUpdated");

        // Care team notifications
        clinicalTopic.AddSubscription("care-team-notifications",
            "PatientAdmitted", "CriticalLabResult", "EmergencyAlert");

        // Billing and insurance
        clinicalTopic.AddSubscription("billing-integration",
            "PatientDischarged", "ProcedureCompleted", "DiagnosisConfirmed");

        // Quality metrics and reporting
        clinicalTopic.AddSubscription("quality-metrics",
            "PatientAdmitted", "PatientDischarged", "ReadmissionAlert");

        // Audit dead letter queue with long retention
        resource.AddQueue("audit-dlq", hipaaOptions);
    }
}
```

## Best Practices

### 1. Naming Conventions

```csharp
// Good: Descriptive names that indicate purpose
resource.AddQueue("order-payment-processing");
resource.AddTopic("customer-lifecycle-events");
resource.AddSubscription("inventory-update-handler", "InventoryChanged");

// Avoid: Generic names
resource.AddQueue("queue1");
resource.AddTopic("events");
resource.AddSubscription("handler", "Event");
```

### 2. Resource Organization

```csharp
public class WellOrganizedContext : IServiceBusContext
{
    public string Name => "Well Organized Service Bus";

    public void BuildServiceBusResource(ServiceBusResource resource)
    {
        // Group related functionality together
        ConfigureOrderProcessing(resource);
        ConfigurePaymentProcessing(resource);
        ConfigureInventoryManagement(resource);
        ConfigureNotifications(resource);
        ConfigureAuditAndCompliance(resource);
    }

    private void ConfigureOrderProcessing(ServiceBusResource resource)
    {
        // Order-specific resources
        resource.AddQueue("order-validation");
        resource.AddQueue("order-fulfillment");
        
        resource.AddTopic("order-events")
            .AddSubscription("order-processor", "OrderCreated", "OrderModified")
            .AddSubscription("order-analytics", "OrderCompleted", "OrderCancelled");
    }

    // ... other configuration methods
}
```

### 3. Error Handling and Dead Letter Queues

```csharp
public void ConfigureWithErrorHandling(ServiceBusResource resource)
{
    // Main processing queue
    var processingOptions = new Queue.Option(
        DefaultTimeToLive: TimeSpan.FromDays(7),
        DuplicateDetectionWindow: TimeSpan.FromMinutes(5),
        EnableBatchedOperations: true,
        EnablePartitioning: false,
        AutoDeleteOnIdle: null
    );

    resource.AddQueue("main-processing", processingOptions);

    // Dead letter queue for failed messages
    resource.AddQueue("failed-message-handling", processingOptions);

    // Topic with dead letter forwarding
    var topic = resource.AddTopic("critical-events");

    var criticalSubOptions = new Subscription.Option(
        DefaultTimeToLive: TimeSpan.FromDays(7),
        DeadLetteringOnMessageExpiration: true,
        LockDuration: TimeSpan.FromMinutes(2),
        AutoDeleteOnIdle: null,
        RequiresSession: false,
        ForwardDeadLetterTo: "failed-message-handling"
    );

    topic.AddSubscription("critical-processor", criticalSubOptions, "CriticalEvent");
}
```

### 4. Performance Optimization

```csharp
public void ConfigureForPerformance(ServiceBusResource resource)
{
    // High-throughput configuration
    var highThroughputOptions = new Queue.Option(
        DefaultTimeToLive: TimeSpan.FromDays(7),
        DuplicateDetectionWindow: null, // Disable for better performance
        EnableBatchedOperations: true,   // Enable batching
        EnablePartitioning: true,        // Enable partitioning for scale
        AutoDeleteOnIdle: null
    );

    resource.AddQueue("high-volume-processing", highThroughputOptions);

    // Topic with optimized subscriptions
    var topic = resource.AddTopic("high-frequency-events", new Topic.Option(
        DefaultTimeToLive: TimeSpan.FromDays(3), // Shorter TTL for high frequency
        DuplicateDetectionWindow: null,
        EnableBatchedOperations: true,
        EnablePartitioning: true,
        AutoDeleteOnIdle: null
    ));

    // Fast processing subscription
    var fastSubOptions = new Subscription.Option(
        DefaultTimeToLive: TimeSpan.FromDays(3),
        DeadLetteringOnMessageExpiration: true,
        LockDuration: TimeSpan.FromSeconds(30), // Short lock for fast processing
        AutoDeleteOnIdle: null,
        RequiresSession: false,
        ForwardDeadLetterTo: null
    );

    topic.AddSubscription("fast-processor", fastSubOptions, "FastEvent");
}
```

### 5. Testing Strategies

```csharp
// Development context for testing
public class TestingServiceBusContext : IServiceBusContext
{
    public string Name => "Testing Environment";

    public void BuildServiceBusResource(ServiceBusResource resource)
    {
        // Short-lived resources for testing
        var testOptions = new Queue.Option(
            DefaultTimeToLive: TimeSpan.FromHours(1),
            DuplicateDetectionWindow: null,
            EnableBatchedOperations: false,
            EnablePartitioning: false,
            AutoDeleteOnIdle: TimeSpan.FromMinutes(30)
        );

        resource.AddQueue("test-integration", testOptions);
        resource.AddQueue("test-unit", testOptions);

        resource.AddTopic("test-events")
            .AddSubscription("test-handler", "TestEvent");
    }
}

// Integration test example
[Test]
public async Task TestServiceBusProvisioning()
{
    var services = new ServiceCollection();
    services.AddServiceBusContext<TestingServiceBusContext>(testConnectionString);
    
    var provider = services.BuildServiceProvider();
    var host = Mock.Of<IHost>(h => h.Services == provider);

    // This should succeed without throwing
    await host.CreateServiceBusResource();
}
```

## Troubleshooting

### Common Issues and Solutions

#### 1. Connection String Problems

```csharp
// Problem: Invalid connection string format
"sb://namespace.servicebus.windows.net/;SharedAccessKeyName=key;SharedAccessKey=value"

// Solution: Correct format with Endpoint
"Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=key;SharedAccessKey=value"
```

#### 2. Permission Issues

```csharp
// Ensure your connection string has Manage permissions
// Required permissions: Manage, Send, Listen
```

#### 3. Resource Already Exists

```csharp
// The library handles this automatically
// It will update properties if they differ from configuration
// No action needed on your part
```

#### 4. Performance Issues

```csharp
// Enable partitioning for high-throughput scenarios
var options = new Queue.Option(
    EnablePartitioning: true,
    EnableBatchedOperations: true
);

// Use shorter lock durations for fast processing
var subOptions = new Subscription.Option(
    LockDuration: TimeSpan.FromSeconds(30)
);
```

#### 5. Debugging Resource Creation

```csharp
// Enable detailed logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Add try-catch for specific error handling
try
{
    await app.CreateServiceBusResource();
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to create Service Bus resources");
    throw;
}
```

This comprehensive usage guide provides practical examples and patterns for implementing ClearServiceBusInitializer in various real-world scenarios. Use these examples as starting points and adapt them to your specific requirements.