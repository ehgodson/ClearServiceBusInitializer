using Clear.ServiceBusInitializer.Entities;

namespace Clear.ServiceBusInitializer.Tests.Entities;

public class TopicTests
{
    [Fact]
    public void Constructor_WithNameOnly_ShouldCreateTopicWithDefaultOptions()
    {
        // Arrange
        const string topicName = "test-topic";

        // Act
        var topic = new Topic(topicName);

        // Assert
        topic.Name.Should().Be("sbt-test-topic");
        topic.Options.Should().NotBeNull();
        topic.Options.DefaultTimeToLive.Should().Be(TimeSpan.FromDays(14));
        topic.Options.DuplicateDetectionWindow.Should().BeNull();
        topic.Options.EnableBatchedOperations.Should().BeTrue();
        topic.Options.EnablePartitioning.Should().BeFalse();
        topic.Options.AutoDeleteOnIdle.Should().BeNull();
        topic.Subscriptions.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithCustomOptions_ShouldCreateTopicWithSpecifiedOptions()
    {
        // Arrange
        const string topicName = "custom-topic";
        var options = new Topic.Option(
            DefaultTimeToLive: TimeSpan.FromDays(7),
            DuplicateDetectionWindow: TimeSpan.FromMinutes(5),
            EnableBatchedOperations: false,
            EnablePartitioning: true,
            AutoDeleteOnIdle: TimeSpan.FromHours(1)
        );

        // Act
        var topic = new Topic(topicName, options);

        // Assert
        topic.Name.Should().Be("sbt-custom-topic");
        topic.Options.Should().Be(options);
        topic.Options.DefaultTimeToLive.Should().Be(TimeSpan.FromDays(7));
        topic.Options.DuplicateDetectionWindow.Should().Be(TimeSpan.FromMinutes(5));
        topic.Options.EnableBatchedOperations.Should().BeFalse();
        topic.Options.EnablePartitioning.Should().BeTrue();
        topic.Options.AutoDeleteOnIdle.Should().Be(TimeSpan.FromHours(1));
    }

    [Fact]
    public void AddSubscription_WithFilters_ShouldAddSubscriptionToTopic()
    {
        // Arrange
        var topic = new Topic("test-topic");
        const string subscriptionName = "test-subscription";
        var filters = new List<string> { "filter1", "filter2" };

        // Act
        var result = topic.AddSubscription(subscriptionName, filters);

        // Assert
        result.Should().Be(topic);
        topic.Subscriptions.Should().HaveCount(1);
        
        var subscription = topic.Subscriptions.First();
        subscription.Name.Should().Be("sbs-test-subscription");
        subscription.Filters.Should().HaveCount(2);
        subscription.Filters[0].Name.Should().Be("sbsr-filter1");
        subscription.Filters[1].Name.Should().Be("sbsr-filter2");
    }

    [Fact]
    public void AddSubscription_WithOptionsAndFilters_ShouldAddSubscriptionWithCustomOptions()
    {
        // Arrange
        var topic = new Topic("test-topic");
        const string subscriptionName = "test-subscription";
        var subscriptionOptions = new Subscription.Option(
            DefaultTimeToLive: TimeSpan.FromDays(1),
            DeadLetteringOnMessageExpiration: false,
            LockDuration: TimeSpan.FromSeconds(30),
            AutoDeleteOnIdle: TimeSpan.FromHours(2),
            RequiresSession: true,
            ForwardDeadLetterTo: "dead-letter-queue"
        );
        var filters = new List<string> { "custom-filter" };

        // Act
        var result = topic.AddSubscription(subscriptionName, subscriptionOptions, filters);

        // Assert
        result.Should().Be(topic);
        topic.Subscriptions.Should().HaveCount(1);
        
        var subscription = topic.Subscriptions.First();
        subscription.Name.Should().Be("sbs-test-subscription");
        subscription.Options.Should().Be(subscriptionOptions);
        subscription.Filters.Should().HaveCount(1);
    }

    [Fact]
    public void AddSubscription_Multiple_ShouldAddAllSubscriptions()
    {
        // Arrange
        var topic = new Topic("test-topic");

        // Act
        topic.AddSubscription("sub1", new List<string> { "filter1" })
             .AddSubscription("sub2", new List<string> { "filter2" })
             .AddSubscription("sub3", new List<string> { "filter3" });

        // Assert
        topic.Subscriptions.Should().HaveCount(3);
        topic.Subscriptions[0].Name.Should().Be("sbs-sub1");
        topic.Subscriptions[1].Name.Should().Be("sbs-sub2");
        topic.Subscriptions[2].Name.Should().Be("sbs-sub3");
    }

    [Fact]
    public void Topic_ShouldInheritFromEntityWithPrefix()
    {
        // Act
        var topic = new Topic("test");

        // Assert
        topic.Should().BeAssignableTo<EntityWithPrefix>();
    }

    [Theory]
    [InlineData("MyTopic", "sbt-mytopic")]
    [InlineData("Order Processing Topic", "sbt-order-processing-topic")]
    [InlineData("sbt-already-prefixed", "sbt-already-prefixed")]
    public void Constructor_ShouldApplyCorrectPrefix(string topicName, string expectedName)
    {
        // Act
        var topic = new Topic(topicName);

        // Assert
        topic.Name.Should().Be(expectedName);
    }

    [Fact]
    public void OptionCreateDefault_ShouldReturnDefaultConfiguration()
    {
        // Act
        var defaultOptions = Topic.Option.CreateDefault();

        // Assert
        defaultOptions.DefaultTimeToLive.Should().Be(TimeSpan.FromDays(14));
        defaultOptions.DuplicateDetectionWindow.Should().BeNull();
        defaultOptions.EnableBatchedOperations.Should().BeTrue();
        defaultOptions.EnablePartitioning.Should().BeFalse();
        defaultOptions.AutoDeleteOnIdle.Should().BeNull();
    }
}