using Clear.ServiceBusInitializer.Entities;

namespace Clear.ServiceBusInitializer.Tests;

public class TestServiceBusContext : IServiceBusContext
{
    public string Name { get; }
    public ServiceBusResource? CreatedResource { get; private set; }

    public TestServiceBusContext(string name = "TestContext")
    {
        Name = name;
    }

    public void BuildServiceBusResource(ServiceBusResource serviceBusResource)
    {
        CreatedResource = serviceBusResource;
        
        // Add some test resources
        serviceBusResource.AddQueue("test-queue");
        
        var topic = serviceBusResource.AddTopic("test-topic");
        topic.AddSubscription("test-subscription", new List<string> { "test-filter" });
    }
}

public class IServiceBusContextTests
{
    [Fact]
    public void IServiceBusContext_ShouldHaveNameProperty()
    {
        // Arrange
        var context = new TestServiceBusContext("MyContext");

        // Act & Assert
        context.Name.Should().Be("MyContext");
    }

    [Fact]
    public void BuildServiceBusResource_ShouldReceiveServiceBusResource()
    {
        // Arrange
        var context = new TestServiceBusContext();
        var resource = new ServiceBusResource("test-resource");

        // Act
        context.BuildServiceBusResource(resource);

        // Assert
        context.CreatedResource.Should().Be(resource);
    }

    [Fact]
    public void BuildServiceBusResource_ShouldConfigureResourcesCorrectly()
    {
        // Arrange
        var context = new TestServiceBusContext();
        var resource = new ServiceBusResource("test-resource");

        // Act
        context.BuildServiceBusResource(resource);

        // Assert
        resource.Queues.Should().HaveCount(1);
        resource.Queues[0].Name.Should().Be("sbq-test-queue");
        
        resource.Topics.Should().HaveCount(1);
        resource.Topics[0].Name.Should().Be("sbt-test-topic");
        resource.Topics[0].Subscriptions.Should().HaveCount(1);
        resource.Topics[0].Subscriptions[0].Name.Should().Be("sbs-test-subscription");
        resource.Topics[0].Subscriptions[0].Filters.Should().HaveCount(1);
    }

    [Fact]
    public void IServiceBusContext_ShouldBeImplementableByCustomClasses()
    {
        // Arrange & Act
        var context = new TestServiceBusContext();

        // Assert
        context.Should().BeAssignableTo<IServiceBusContext>();
    }
}

// Additional test context for different scenarios
public class EmptyServiceBusContext : IServiceBusContext
{
    public string Name => "EmptyContext";

    public void BuildServiceBusResource(ServiceBusResource serviceBusResource)
    {
        // Intentionally empty - for testing empty configurations
    }
}

public class ComplexServiceBusContext : IServiceBusContext
{
    public string Name => "ComplexContext";

    public void BuildServiceBusResource(ServiceBusResource serviceBusResource)
    {
        // Multiple queues
        serviceBusResource.AddQueue("orders-queue");
        serviceBusResource.AddQueue("payments-queue");
        serviceBusResource.AddQueue("notifications-queue");

        // Multiple topics with subscriptions
        var orderTopic = serviceBusResource.AddTopic("orders-topic");
        orderTopic.AddSubscription("order-created-sub", new List<string> { "OrderCreated" })
                  .AddSubscription("order-updated-sub", new List<string> { "OrderUpdated" })
                  .AddSubscription("order-cancelled-sub", new List<string> { "OrderCancelled" });

        var paymentTopic = serviceBusResource.AddTopic("payments-topic");
        paymentTopic.AddSubscription("payment-processing-sub", new List<string> { "PaymentStarted", "PaymentPending" })
                    .AddSubscription("payment-completed-sub", new List<string> { "PaymentCompleted", "PaymentFailed" });
    }
}