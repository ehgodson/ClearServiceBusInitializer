using Clear.ServiceBusInitializer.Entities;

namespace Clear.ServiceBusInitializer.Tests.Entities;

public class SubscriptionTests
{
    [Fact]
    public void Constructor_WithFiltersOnly_ShouldCreateSubscriptionWithDefaultOptions()
    {
        // Arrange
        const string subscriptionName = "test-subscription";
        var filters = new List<string> { "filter1", "filter2" };

        // Act
        var subscription = new Subscription(subscriptionName, filters);

        // Assert
        subscription.Name.Should().Be("sbs-test-subscription");
        subscription.Options.Should().NotBeNull();
        subscription.Options.DefaultTimeToLive.Should().Be(TimeSpan.FromDays(14));
        subscription.Options.DeadLetteringOnMessageExpiration.Should().BeTrue();
        subscription.Options.LockDuration.Should().Be(TimeSpan.FromMinutes(1));
        subscription.Options.AutoDeleteOnIdle.Should().BeNull();
        subscription.Options.RequiresSession.Should().BeFalse();
        subscription.Options.ForwardDeadLetterTo.Should().BeNull();
        subscription.Filters.Should().HaveCount(2);
    }

    [Fact]
    public void Constructor_WithCustomOptionsAndFilters_ShouldCreateSubscriptionWithSpecifiedOptions()
    {
        // Arrange
        const string subscriptionName = "custom-subscription";
        var options = new Subscription.Option(
            DefaultTimeToLive: TimeSpan.FromDays(7),
            DeadLetteringOnMessageExpiration: false,
            LockDuration: TimeSpan.FromSeconds(30),
            AutoDeleteOnIdle: TimeSpan.FromHours(1),
            RequiresSession: true,
            ForwardDeadLetterTo: "dead-letter-queue"
        );
        var filters = new List<string> { "custom-filter" };

        // Act
        var subscription = new Subscription(subscriptionName, options, filters);

        // Assert
        subscription.Name.Should().Be("sbs-custom-subscription");
        subscription.Options.Should().Be(options);
        subscription.Options.DefaultTimeToLive.Should().Be(TimeSpan.FromDays(7));
        subscription.Options.DeadLetteringOnMessageExpiration.Should().BeFalse();
        subscription.Options.LockDuration.Should().Be(TimeSpan.FromSeconds(30));
        subscription.Options.AutoDeleteOnIdle.Should().Be(TimeSpan.FromHours(1));
        subscription.Options.RequiresSession.Should().BeTrue();
        subscription.Options.ForwardDeadLetterTo.Should().Be("dead-letter-queue");
        subscription.Filters.Should().HaveCount(1);
    }

    [Fact]
    public void Constructor_ShouldCreateLabelFiltersFromFilterList()
    {
        // Arrange
        const string subscriptionName = "test-subscription";
        var filters = new List<string> { "OrderCreated", "OrderUpdated", "OrderDeleted" };

        // Act
        var subscription = new Subscription(subscriptionName, filters);

        // Assert
        subscription.Filters.Should().HaveCount(3);
        subscription.Filters[0].Name.Should().Be("sbsr-ordercreated");
        subscription.Filters[0].SqlExpression.Should().Be("sys.Label='OrderCreated'");
        subscription.Filters[1].Name.Should().Be("sbsr-orderupdated");
        subscription.Filters[1].SqlExpression.Should().Be("sys.Label='OrderUpdated'");
        subscription.Filters[2].Name.Should().Be("sbsr-orderdeleted");
        subscription.Filters[2].SqlExpression.Should().Be("sys.Label='OrderDeleted'");
    }

    [Fact]
    public void AddLabelFilter_ShouldAddFilterToSubscription()
    {
        // Arrange
        var subscription = new Subscription("test-subscription", new List<string>());
        const string filterName = "NewFilter";

        // Act
        var result = subscription.AddLabelFilter(filterName);

        // Assert
        result.Should().Be(subscription);
        subscription.Filters.Should().HaveCount(1);
        subscription.Filters[0].Name.Should().Be("sbsr-newfilter");
        subscription.Filters[0].SqlExpression.Should().Be("sys.Label='NewFilter'");
    }

    [Fact]
    public void AddLabelFilter_Multiple_ShouldAddAllFilters()
    {
        // Arrange
        var subscription = new Subscription("test-subscription", new List<string>());

        // Act
        subscription.AddLabelFilter("Filter1")
                   .AddLabelFilter("Filter2")
                   .AddLabelFilter("Filter3");

        // Assert
        subscription.Filters.Should().HaveCount(3);
        subscription.Filters[0].Name.Should().Be("sbsr-filter1");
        subscription.Filters[1].Name.Should().Be("sbsr-filter2");
        subscription.Filters[2].Name.Should().Be("sbsr-filter3");
    }

    [Fact]
    public void Subscription_ShouldInheritFromEntityWithPrefix()
    {
        // Act
        var subscription = new Subscription("test", new List<string>());

        // Assert
        subscription.Should().BeAssignableTo<EntityWithPrefix>();
    }

    [Theory]
    [InlineData("MySubscription", "sbs-mysubscription")]
    [InlineData("Order Processing Subscription", "sbs-order-processing-subscription")]
    [InlineData("sbs-already-prefixed", "sbs-already-prefixed")]
    public void Constructor_ShouldApplyCorrectPrefix(string subscriptionName, string expectedName)
    {
        // Act
        var subscription = new Subscription(subscriptionName, new List<string>());

        // Assert
        subscription.Name.Should().Be(expectedName);
    }

    [Fact]
    public void OptionCreateDefault_ShouldReturnDefaultConfiguration()
    {
        // Act
        var defaultOptions = Subscription.Option.CreateDefault();

        // Assert
        defaultOptions.DefaultTimeToLive.Should().Be(TimeSpan.FromDays(14));
        defaultOptions.DeadLetteringOnMessageExpiration.Should().BeTrue();
        defaultOptions.LockDuration.Should().Be(TimeSpan.FromMinutes(1));
        defaultOptions.AutoDeleteOnIdle.Should().BeNull();
        defaultOptions.RequiresSession.Should().BeFalse();
        defaultOptions.ForwardDeadLetterTo.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithEmptyFilters_ShouldCreateSubscriptionWithoutFilters()
    {
        // Arrange
        const string subscriptionName = "empty-subscription";
        var filters = new List<string>();

        // Act
        var subscription = new Subscription(subscriptionName, filters);

        // Assert
        subscription.Name.Should().Be("sbs-empty-subscription");
        subscription.Filters.Should().BeEmpty();
    }
}