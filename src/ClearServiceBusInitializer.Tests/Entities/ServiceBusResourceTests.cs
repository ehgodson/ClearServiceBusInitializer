using Clear.ServiceBusInitializer.Entities;

namespace Clear.ServiceBusInitializer.Tests.Entities;

public class ServiceBusResourceTests
{
    [Fact]
    public void Constructor_ShouldCreateServiceBusResourceWithCorrectName()
    {
        // Arrange
        const string resourceName = "test-resource";

        // Act
        var resource = new ServiceBusResource(resourceName);

        // Assert
        resource.Name.Should().Be("sb-test-resource");
        resource.Topics.Should().BeEmpty();
        resource.Queues.Should().BeEmpty();
    }

    [Fact]
    public void AddTopic_WithNameOnly_ShouldAddTopicWithDefaultOptions()
    {
        // Arrange
        var resource = new ServiceBusResource("test-resource");
        const string topicName = "test-topic";

        // Act
        var topic = resource.AddTopic(topicName);

        // Assert
        topic.Should().NotBeNull();
        topic.Name.Should().Be("sbt-test-topic");
        resource.Topics.Should().HaveCount(1);
        resource.Topics[0].Should().Be(topic);
        topic.Options.Should().Be(Topic.Option.CreateDefault());
    }

    [Fact]
    public void AddTopic_WithCustomOptions_ShouldAddTopicWithSpecifiedOptions()
    {
        // Arrange
        var resource = new ServiceBusResource("test-resource");
        const string topicName = "custom-topic";
        var options = new Topic.Option(
            DefaultTimeToLive: TimeSpan.FromDays(7),
            DuplicateDetectionWindow: TimeSpan.FromMinutes(5),
            EnableBatchedOperations: false,
            EnablePartitioning: true,
            AutoDeleteOnIdle: TimeSpan.FromHours(1)
        );

        // Act
        var topic = resource.AddTopic(topicName, options);

        // Assert
        topic.Should().NotBeNull();
        topic.Name.Should().Be("sbt-custom-topic");
        topic.Options.Should().Be(options);
        resource.Topics.Should().HaveCount(1);
        resource.Topics[0].Should().Be(topic);
    }

    [Fact]
    public void AddQueue_WithNameOnly_ShouldAddQueueWithDefaultOptions()
    {
        // Arrange
        var resource = new ServiceBusResource("test-resource");
        const string queueName = "test-queue";

        // Act
        var queue = resource.AddQueue(queueName);

        // Assert
        queue.Should().NotBeNull();
        queue.Name.Should().Be("sbq-test-queue");
        resource.Queues.Should().HaveCount(1);
        resource.Queues[0].Should().Be(queue);
        queue.Options.Should().Be(Queue.Option.CreateDefault());
    }

    [Fact]
    public void AddQueue_WithCustomOptions_ShouldAddQueueWithSpecifiedOptions()
    {
        // Arrange
        var resource = new ServiceBusResource("test-resource");
        const string queueName = "custom-queue";
        var options = new Queue.Option(
            DefaultTimeToLive: TimeSpan.FromDays(7),
            DuplicateDetectionWindow: TimeSpan.FromMinutes(5),
            EnableBatchedOperations: false,
            EnablePartitioning: true,
            AutoDeleteOnIdle: TimeSpan.FromHours(1)
        );

        // Act
        var queue = resource.AddQueue(queueName, options);

        // Assert
        queue.Should().NotBeNull();
        queue.Name.Should().Be("sbq-custom-queue");
        queue.Options.Should().Be(options);
        resource.Queues.Should().HaveCount(1);
        resource.Queues[0].Should().Be(queue);
    }

    [Fact]
    public void AddMultipleTopicsAndQueues_ShouldAddAllResources()
    {
        // Arrange
        var resource = new ServiceBusResource("test-resource");

        // Act
        resource.AddTopic("topic1");
        resource.AddTopic("topic2");
        resource.AddQueue("queue1");
        resource.AddQueue("queue2");

        // Assert
        resource.Topics.Should().HaveCount(2);
        resource.Queues.Should().HaveCount(2);
        resource.Topics[0].Name.Should().Be("sbt-topic1");
        resource.Topics[1].Name.Should().Be("sbt-topic2");
        resource.Queues[0].Name.Should().Be("sbq-queue1");
        resource.Queues[1].Name.Should().Be("sbq-queue2");
    }

    [Fact]
    public void ServiceBusResource_ShouldInheritFromEntityWithPrefix()
    {
        // Act
        var resource = new ServiceBusResource("test");

        // Assert
        resource.Should().BeAssignableTo<EntityWithPrefix>();
    }

    [Theory]
    [InlineData("MyServiceBus", "sb-myservicebus")]
    [InlineData("Order Processing Service Bus", "sb-order-processing-service-bus")]
    [InlineData("sb-already-prefixed", "sb-already-prefixed")]
    public void Constructor_ShouldApplyCorrectPrefix(string resourceName, string expectedName)
    {
        // Act
        var resource = new ServiceBusResource(resourceName);

        // Assert
        resource.Name.Should().Be(expectedName);
    }

    [Fact]
    public void FluentConfiguration_ShouldAllowChainedCalls()
    {
        // Arrange
        var resource = new ServiceBusResource("test-resource");

        // Act
        var topic1 = resource.AddTopic("topic1");
        var topic2 = resource.AddTopic("topic2");
        var queue1 = resource.AddQueue("queue1");
        var queue2 = resource.AddQueue("queue2");

        // Build a complex topic with subscriptions using fluent API
        topic1.AddSubscription("sub1", new List<string> { "filter1" })
              .AddSubscription("sub2", new List<string> { "filter2" });

        // Assert
        resource.Topics.Should().HaveCount(2);
        resource.Queues.Should().HaveCount(2);
        topic1.Subscriptions.Should().HaveCount(2);
        topic2.Subscriptions.Should().BeEmpty();
    }
}