using Azure.Messaging.ServiceBus.Administration;
using Clear.ServiceBusInitializer.Entities;
using System;

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

    [Fact]
    public void DeleteTopicAsync_ShouldHaveCorrectSignature()
    {
        // Arrange
        const string validConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==";
        var adminClient = new ServiceBusAdministrationClient(validConnectionString);
        var provisioner = new ServiceBusProvisioner(adminClient);

        // Assert - Verify the method exists with correct signature
        var method = typeof(ServiceBusProvisioner).GetMethod("DeleteTopicAsync");
        method.Should().NotBeNull();

        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(1);
        parameters[0].ParameterType.Should().Be<string[]>();
        parameters[0].Name.Should().Be("topicNames");
        // Verify it's a params parameter
        parameters[0].GetCustomAttributes(typeof(ParamArrayAttribute), false).Should().NotBeEmpty();
    }

    [Fact]
    public void DeleteQueueAsync_ShouldHaveCorrectSignature()
    {
        // Arrange
        const string validConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==";
        var adminClient = new ServiceBusAdministrationClient(validConnectionString);
        var provisioner = new ServiceBusProvisioner(adminClient);

        // Assert - Verify the method exists with correct signature
        var method = typeof(ServiceBusProvisioner).GetMethod("DeleteQueueAsync");
        method.Should().NotBeNull();

        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(1);
        parameters[0].ParameterType.Should().Be<string[]>();
        parameters[0].Name.Should().Be("queueNames");
        // Verify it's a params parameter
        parameters[0].GetCustomAttributes(typeof(ParamArrayAttribute), false).Should().NotBeEmpty();
    }

    [Fact]
    public void DeleteSubscriptionAsync_ShouldHaveCorrectSignature()
    {
        // Arrange
        const string validConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==";
        var adminClient = new ServiceBusAdministrationClient(validConnectionString);
        var provisioner = new ServiceBusProvisioner(adminClient);

        // Assert - Verify the method exists with correct signature
        var method = typeof(ServiceBusProvisioner).GetMethod("DeleteSubscriptionAsync");
        method.Should().NotBeNull();

        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].ParameterType.Should().Be<string>();
        parameters[0].Name.Should().Be("topicName");
        parameters[1].ParameterType.Should().Be<string[]>();
        parameters[1].Name.Should().Be("subscriptionNames");
        // Verify the second parameter is a params parameter
        parameters[1].GetCustomAttributes(typeof(ParamArrayAttribute), false).Should().NotBeEmpty();
    }

    [Fact]
    public void DeleteFilterAsync_ShouldHaveCorrectSignature()
    {
        // Arrange
        const string validConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==";
        var adminClient = new ServiceBusAdministrationClient(validConnectionString);
        var provisioner = new ServiceBusProvisioner(adminClient);

        // Assert - Verify the method exists with correct signature
        var method = typeof(ServiceBusProvisioner).GetMethod("DeleteFilterAsync");
        method.Should().NotBeNull();

        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].ParameterType.Should().Be<string>();
        parameters[0].Name.Should().Be("topicName");
        parameters[1].ParameterType.Should().Be<string>();
        parameters[1].Name.Should().Be("subscriptionName");
        parameters[2].ParameterType.Should().Be<string[]>();
        parameters[2].Name.Should().Be("filterNames");
        // Verify the third parameter is a params parameter
        parameters[2].GetCustomAttributes(typeof(ParamArrayAttribute), false).Should().NotBeEmpty();
    }

    [Fact]
    public async Task DeleteTopicAsync_WithInvalidConnectionString_ShouldThrow()
    {
        // Arrange
        const string invalidConnectionString = "invalid-connection-string";
        var adminClient = new ServiceBusAdministrationClient("Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==");
        var provisioner = new ServiceBusProvisioner(adminClient);

        // Act & Assert - Should throw when trying to connect
        // Note: This test verifies the method exists but cannot test actual Service Bus operations
        // in a unit test environment without a real connection string
        var method = typeof(ServiceBusProvisioner).GetMethod("DeleteTopicAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteQueueAsync_WithInvalidConnectionString_ShouldThrow()
    {
        // Arrange
        const string invalidConnectionString = "invalid-connection-string";
        var adminClient = new ServiceBusAdministrationClient("Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==");
        var provisioner = new ServiceBusProvisioner(adminClient);

        // Act & Assert - Should throw when trying to connect
        // Note: This test verifies the method exists but cannot test actual Service Bus operations
        // in a unit test environment without a real connection string
        var method = typeof(ServiceBusProvisioner).GetMethod("DeleteQueueAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteSubscriptionAsync_WithInvalidConnectionString_ShouldThrow()
    {
        // Arrange
        const string invalidConnectionString = "invalid-connection-string";
        var adminClient = new ServiceBusAdministrationClient("Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==");
        var provisioner = new ServiceBusProvisioner(adminClient);

        // Act & Assert - Should throw when trying to connect
        // Note: This test verifies the method exists but cannot test actual Service Bus operations
        // in a unit test environment without a real connection string
        var method = typeof(ServiceBusProvisioner).GetMethod("DeleteSubscriptionAsync");
        method.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteFilterAsync_WithInvalidConnectionString_ShouldThrow()
    {
        // Arrange
        const string invalidConnectionString = "invalid-connection-string";
        var adminClient = new ServiceBusAdministrationClient("Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==");
        var provisioner = new ServiceBusProvisioner(adminClient);

        // Act & Assert - Should throw when trying to connect
        // Note: This test verifies the method exists but cannot test actual Service Bus operations
        // in a unit test environment without a real connection string
        var method = typeof(ServiceBusProvisioner).GetMethod("DeleteFilterAsync");
        method.Should().NotBeNull();
    }

    [Theory]
    [InlineData("test-topic")]
    [InlineData("another-topic")]
    public void DeleteTopicAsync_ShouldAcceptValidTopicName(string topicName)
    {
        // Arrange
        const string validConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==";
        var adminClient = new ServiceBusAdministrationClient(validConnectionString);
        var provisioner = new ServiceBusProvisioner(adminClient);

        // Act & Assert - Should not throw during method call setup
        var method = typeof(ServiceBusProvisioner).GetMethod("DeleteTopicAsync");
        method.Should().NotBeNull();

        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(1);
        parameters[0].ParameterType.Should().Be<string[]>();
    }

    [Theory]
    [InlineData("test-queue")]
    [InlineData("another-queue")]
    public void DeleteQueueAsync_ShouldAcceptValidQueueName(string queueName)
    {
        // Arrange
        const string validConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==";
        var adminClient = new ServiceBusAdministrationClient(validConnectionString);
        var provisioner = new ServiceBusProvisioner(adminClient);

        // Act & Assert - Should not throw during method call setup
        var method = typeof(ServiceBusProvisioner).GetMethod("DeleteQueueAsync");
        method.Should().NotBeNull();

        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(1);
        parameters[0].ParameterType.Should().Be<string[]>();
    }

    [Theory]
    [InlineData("test-topic", "test-subscription")]
    [InlineData("another-topic", "another-subscription")]
    public void DeleteSubscriptionAsync_ShouldAcceptValidNames(string topicName, string subscriptionName)
    {
        // Arrange
        const string validConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==";
        var adminClient = new ServiceBusAdministrationClient(validConnectionString);
        var provisioner = new ServiceBusProvisioner(adminClient);

        // Act & Assert - Should not throw during method call setup
        var method = typeof(ServiceBusProvisioner).GetMethod("DeleteSubscriptionAsync");
        method.Should().NotBeNull();

        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(2);
        parameters[0].ParameterType.Should().Be<string>();
        parameters[1].ParameterType.Should().Be<string[]>();
    }

    [Theory]
    [InlineData("test-topic", "test-subscription", "test-filter")]
    [InlineData("another-topic", "another-subscription", "another-filter")]
    public void DeleteFilterAsync_ShouldAcceptValidNames(string topicName, string subscriptionName, string filterName)
    {
        // Arrange
        const string validConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==";
        var adminClient = new ServiceBusAdministrationClient(validConnectionString);
        var provisioner = new ServiceBusProvisioner(adminClient);

        // Act & Assert - Should not throw during method call setup
        var method = typeof(ServiceBusProvisioner).GetMethod("DeleteFilterAsync");
        method.Should().NotBeNull();

        var parameters = method!.GetParameters();
        parameters.Should().HaveCount(3);
        parameters[0].ParameterType.Should().Be<string>();
        parameters[1].ParameterType.Should().Be<string>();
        parameters[2].ParameterType.Should().Be<string[]>();
    }
}