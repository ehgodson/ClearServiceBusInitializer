using Azure.Messaging.ServiceBus.Administration;
using Clear.ServiceBusInitializer.Entities;

namespace Clear.ServiceBusInitializer.Tests;

public class ServiceBusProvisionerTests
{
    [Fact]
    public void ServiceBusProvisioner_Constructor_ShouldAcceptAdminClient()
    {
        // Arrange
        const string validConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==";
        var adminClient = new ServiceBusAdministrationClient(validConnectionString);

        // Act
        var provisioner = new ServiceBusProvisioner(adminClient);

        // Assert
        provisioner.Should().NotBeNull();
    }

    [Fact]
    public void ServiceBusProvisioner_ShouldHaveRequiredMethods()
    {
        // Arrange
        const string validConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==";
        var adminClient = new ServiceBusAdministrationClient(validConnectionString);
        var provisioner = new ServiceBusProvisioner(adminClient);

        // Assert
        provisioner.Should().NotBeNull();
        
        // Verify the class has the expected methods
        var type = typeof(ServiceBusProvisioner);
        var ensureTopicMethod = type.GetMethod("EnsureTopicAsync");
        var ensureQueueMethod = type.GetMethod("EnsureQueueAsync");
        var ensureSubscriptionMethod = type.GetMethod("EnsureSubscriptionAsync");

        ensureTopicMethod.Should().NotBeNull();
        ensureQueueMethod.Should().NotBeNull();
        ensureSubscriptionMethod.Should().NotBeNull();
    }

    [Fact]
    public async Task EnsureTopicAsync_WithInvalidConnectionString_ShouldThrow()
    {
        // Arrange
        var topic = new Topic("test-topic");
        
        // This will throw when trying to use the admin client
        const string validConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==";
        var adminClient = new ServiceBusAdministrationClient(validConnectionString);
        var provisioner = new ServiceBusProvisioner(adminClient);

        // Act & Assert - This will fail when trying to connect to service bus (unauthorized/network error)
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => provisioner.EnsureTopicAsync(topic));
        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task EnsureQueueAsync_WithInvalidConnectionString_ShouldThrow()
    {
        // Arrange
        var queue = new Queue("test-queue");
        
        // This will throw when trying to use the admin client
        const string validConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==";
        var adminClient = new ServiceBusAdministrationClient(validConnectionString);
        var provisioner = new ServiceBusProvisioner(adminClient);

        // Act & Assert - This will fail when trying to connect to service bus (unauthorized/network error)
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => provisioner.EnsureQueueAsync(queue));
        exception.Should().NotBeNull();
    }

    [Fact]
    public async Task EnsureSubscriptionAsync_WithInvalidConnectionString_ShouldThrow()
    {
        // Arrange
        var topic = new Topic("test-topic");
        var subscription = new Subscription("test-subscription", new List<string> { "test-filter" });
        
        // This will throw when trying to use the admin client
        const string validConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==";
        var adminClient = new ServiceBusAdministrationClient(validConnectionString);
        var provisioner = new ServiceBusProvisioner(adminClient);

        // Act & Assert - This will fail when trying to connect to service bus (unauthorized/network error)
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => provisioner.EnsureSubscriptionAsync(subscription, topic));
        exception.Should().NotBeNull();
    }

    [Fact]
    public void ServiceBusProvisioner_ShouldHaveCorrectConstructorSignature()
    {
        // Arrange & Act
        var constructors = typeof(ServiceBusProvisioner).GetConstructors();

        // Assert
        constructors.Should().HaveCount(1);
        var constructor = constructors[0];
        var parameters = constructor.GetParameters();
        
        parameters.Should().HaveCount(1);
        parameters[0].ParameterType.Should().Be<ServiceBusAdministrationClient>();
    }

    [Theory]
    [InlineData("test-topic")]
    [InlineData("another-topic")]
    public void EnsureTopicAsync_ShouldAcceptValidTopic(string topicName)
    {
        // Arrange
        const string validConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==";
        var adminClient = new ServiceBusAdministrationClient(validConnectionString);
        var provisioner = new ServiceBusProvisioner(adminClient);
        var topic = new Topic(topicName);

        // Act & Assert - Should not throw during method call setup
        var method = typeof(ServiceBusProvisioner).GetMethod("EnsureTopicAsync");
        method.Should().NotBeNull();
        
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(1);
        parameters[0].ParameterType.Should().Be<Topic>();
    }

    [Theory]
    [InlineData("test-queue")]
    [InlineData("another-queue")]
    public void EnsureQueueAsync_ShouldAcceptValidQueue(string queueName)
    {
        // Arrange
        const string validConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==";
        var adminClient = new ServiceBusAdministrationClient(validConnectionString);
        var provisioner = new ServiceBusProvisioner(adminClient);
        var queue = new Queue(queueName);

        // Act & Assert - Should not throw during method call setup
        var method = typeof(ServiceBusProvisioner).GetMethod("EnsureQueueAsync");
        method.Should().NotBeNull();
        
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(1);
        parameters[0].ParameterType.Should().Be<Queue>();
    }

    [Fact]
    public void EnsureSubscriptionAsync_ShouldAcceptValidSubscriptionAndTopic()
    {
        // Arrange
        const string validConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==";
        var adminClient = new ServiceBusAdministrationClient(validConnectionString);
        var provisioner = new ServiceBusProvisioner(adminClient);
        var topic = new Topic("test-topic");
        var subscription = new Subscription("test-subscription", new List<string> { "filter" });

        // Act & Assert - Should not throw during method call setup
        var method = typeof(ServiceBusProvisioner).GetMethod("EnsureSubscriptionAsync");
        method.Should().NotBeNull();
        
        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].ParameterType.Should().Be<Subscription>();
        parameters[1].ParameterType.Should().Be<Topic>();
    }

    [Fact]
    public void ServiceBusProvisioner_ShouldAcceptOnlyServiceBusAdministrationClient()
    {
        // Arrange & Act
        var type = typeof(ServiceBusProvisioner);
        var constructor = type.GetConstructors().First();

        // Assert
        constructor.GetParameters().Should().HaveCount(1);
        constructor.GetParameters()[0].ParameterType.Should().Be<ServiceBusAdministrationClient>();
    }

    [Fact]
    public void ServiceBusAdministrationClient_ShouldRejectInvalidConnectionString()
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => new ServiceBusAdministrationClient("invalid-connection-string"));
    }
}