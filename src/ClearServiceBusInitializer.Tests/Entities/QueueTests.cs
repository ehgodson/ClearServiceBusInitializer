using Clear.ServiceBusInitializer.Entities;

namespace Clear.ServiceBusInitializer.Tests.Entities;

public class QueueTests
{
    [Fact]
    public void Constructor_WithNameOnly_ShouldCreateQueueWithDefaultOptions()
    {
        // Arrange
        const string queueName = "test-queue";

        // Act
        var queue = new Queue(queueName);

        // Assert
        queue.Name.Should().Be("sbq-test-queue");
        queue.Options.Should().NotBeNull();
        queue.Options.DefaultTimeToLive.Should().Be(TimeSpan.FromDays(14));
        queue.Options.DuplicateDetectionWindow.Should().BeNull();
        queue.Options.EnableBatchedOperations.Should().BeTrue();
        queue.Options.EnablePartitioning.Should().BeFalse();
        queue.Options.AutoDeleteOnIdle.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithCustomOptions_ShouldCreateQueueWithSpecifiedOptions()
    {
        // Arrange
        const string queueName = "custom-queue";
        var options = new Queue.Option(
            DefaultTimeToLive: TimeSpan.FromDays(7),
            DuplicateDetectionWindow: TimeSpan.FromMinutes(5),
            EnableBatchedOperations: false,
            EnablePartitioning: true,
            AutoDeleteOnIdle: TimeSpan.FromHours(1)
        );

        // Act
        var queue = new Queue(queueName, options);

        // Assert
        queue.Name.Should().Be("sbq-custom-queue");
        queue.Options.Should().Be(options);
        queue.Options.DefaultTimeToLive.Should().Be(TimeSpan.FromDays(7));
        queue.Options.DuplicateDetectionWindow.Should().Be(TimeSpan.FromMinutes(5));
        queue.Options.EnableBatchedOperations.Should().BeFalse();
        queue.Options.EnablePartitioning.Should().BeTrue();
        queue.Options.AutoDeleteOnIdle.Should().Be(TimeSpan.FromHours(1));
    }

    [Fact]
    public void Queue_ShouldInheritFromEntityWithPrefix()
    {
        // Act
        var queue = new Queue("test");

        // Assert
        queue.Should().BeAssignableTo<EntityWithPrefix>();
    }

    [Theory]
    [InlineData("MyQueue", "sbq-myqueue")]
    [InlineData("Order Processing Queue", "sbq-order-processing-queue")]
    [InlineData("sbq-already-prefixed", "sbq-already-prefixed")]
    public void Constructor_ShouldApplyCorrectPrefix(string queueName, string expectedName)
    {
        // Act
        var queue = new Queue(queueName);

        // Assert
        queue.Name.Should().Be(expectedName);
    }

    [Fact]
    public void OptionCreateDefault_ShouldReturnDefaultConfiguration()
    {
        // Act
        var defaultOptions = Queue.Option.CreateDefault();

        // Assert
        defaultOptions.DefaultTimeToLive.Should().Be(TimeSpan.FromDays(14));
        defaultOptions.DuplicateDetectionWindow.Should().BeNull();
        defaultOptions.EnableBatchedOperations.Should().BeTrue();
        defaultOptions.EnablePartitioning.Should().BeFalse();
        defaultOptions.AutoDeleteOnIdle.Should().BeNull();
    }
}