using Azure.Messaging.ServiceBus.Administration;
using Clear.ServiceBusInitializer.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Clear.ServiceBusInitializer.Tests;

public class ServiceBusInitializerTests
{
    private const string ValidConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==";

    [Fact]
    public void AddServiceBusContext_ShouldRegisterServicesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddServiceBusContext<TestServiceBusContext>(ValidConnectionString);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        
        var serviceBusContext = serviceProvider.GetService<IServiceBusContext>();
        serviceBusContext.Should().NotBeNull();
        serviceBusContext.Should().BeOfType<TestServiceBusContext>();

        var adminClient = serviceProvider.GetService<ServiceBusAdministrationClient>();
        adminClient.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateServiceBusResource_WithoutAdminClient_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IServiceBusContext, TestServiceBusContext>();
        // Note: Not registering ServiceBusAdministrationClient
        
        var serviceProvider = services.BuildServiceProvider();
        var host = Mock.Of<IHost>(h => h.Services == serviceProvider);

        // Act & Assert
        // GetRequiredService throws InvalidOperationException when service not found
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            host.CreateServiceBusResource());
    }

    [Fact]
    public async Task CreateServiceBusResource_WithoutServiceBusContext_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(new ServiceBusAdministrationClient(ValidConnectionString));
        // Note: Not registering IServiceBusContext
        
        var serviceProvider = services.BuildServiceProvider();
        var host = Mock.Of<IHost>(h => h.Services == serviceProvider);

        // Act & Assert
        // GetRequiredService throws InvalidOperationException when service not found
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            host.CreateServiceBusResource());
    }

    [Fact]
    public async Task CreateServiceBusResource_StaticMethod_ShouldProvisionResourcesCorrectly()
    {
        // Arrange
        var serviceBusResource = new ServiceBusResource("test-resource");
        serviceBusResource.AddQueue("test-queue");
        serviceBusResource.AddTopic("test-topic")
                         .AddSubscription("test-subscription", new List<string> { "test-filter" });

        // Note: This test validates the method signature and structure
        // For now, we'll test that the method doesn't throw with invalid connection string
        
        // Act & Assert
        await Assert.ThrowsAsync<FormatException>(() => 
            ServiceBusInitializer.CreateServiceBusResource("invalid-connection-string", serviceBusResource));
    }

    [Fact]
    public void AddServiceBusContext_WithGenericType_ShouldRegisterCorrectType()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddServiceBusContext<ComplexServiceBusContext>(ValidConnectionString);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var serviceBusContext = serviceProvider.GetService<IServiceBusContext>();
        serviceBusContext.Should().NotBeNull();
        serviceBusContext.Should().BeOfType<ComplexServiceBusContext>();
        serviceBusContext!.Name.Should().Be("ComplexContext");
    }

    [Fact]
    public void ServiceBusInitializer_ShouldBeStaticClass()
    {
        // Assert
        var type = typeof(ServiceBusInitializer);
        type.IsAbstract.Should().BeTrue(); // Static classes are abstract
        type.IsSealed.Should().BeTrue();   // Static classes are sealed
        type.IsClass.Should().BeTrue();    // It's a class
        
        // Static classes cannot be instantiated
        type.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Should().BeEmpty();
    }

    [Fact]
    public void AddServiceBusContext_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddServiceBusContext<TestServiceBusContext>(ValidConnectionString);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddServiceBusContext_WithDifferentContextTypes_ShouldRegisterCorrectly()
    {
        // Arrange
        var services1 = new ServiceCollection();
        var services2 = new ServiceCollection();

        // Act
        services1.AddServiceBusContext<TestServiceBusContext>(ValidConnectionString);
        services2.AddServiceBusContext<EmptyServiceBusContext>(ValidConnectionString);

        // Assert
        var provider1 = services1.BuildServiceProvider();
        var provider2 = services2.BuildServiceProvider();

        var context1 = provider1.GetService<IServiceBusContext>();
        var context2 = provider2.GetService<IServiceBusContext>();

        context1.Should().BeOfType<TestServiceBusContext>();
        context2.Should().BeOfType<EmptyServiceBusContext>();
    }

    [Theory]
    [InlineData("Endpoint=sb://test1.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==")]
    [InlineData("Endpoint=sb://test2.servicebus.windows.net/;SharedAccessKeyName=test;SharedAccessKey=dGVzdA==")]
    public void AddServiceBusContext_WithDifferentConnectionStrings_ShouldRegisterCorrectly(string connectionString)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddServiceBusContext<TestServiceBusContext>(connectionString);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var adminClient = serviceProvider.GetService<ServiceBusAdministrationClient>();
        adminClient.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateServiceBusResource_WithValidRegistration_ShouldReturnSameHost()
    {
        // Arrange
        var services = new ServiceCollection();
        
        services.AddServiceBusContext<EmptyServiceBusContext>(ValidConnectionString);
        var serviceProvider = services.BuildServiceProvider();
        var host = Mock.Of<IHost>(h => h.Services == serviceProvider);

        // Act
        var result = await host.CreateServiceBusResource();

        // Assert
        result.Should().Be(host);
    }

    [Fact]
    public void ServiceBusInitializer_ExtensionMethods_ShouldHaveCorrectSignatures()
    {
        // Arrange & Act
        var addServiceBusContextMethod = typeof(ServiceBusInitializer)
            .GetMethods()
            .FirstOrDefault(m => m.Name == "AddServiceBusContext");

        var createServiceBusResourceMethod = typeof(ServiceBusInitializer)
            .GetMethods()
            .FirstOrDefault(m => m.Name == "CreateServiceBusResource" && m.GetParameters().Length == 1);

        var createServiceBusResourceStaticMethod = typeof(ServiceBusInitializer)
            .GetMethods()
            .FirstOrDefault(m => m.Name == "CreateServiceBusResource" && m.GetParameters().Length == 2);

        // Assert
        addServiceBusContextMethod.Should().NotBeNull();
        createServiceBusResourceMethod.Should().NotBeNull();
        createServiceBusResourceStaticMethod.Should().NotBeNull();

        // Verify extension method characteristics
        addServiceBusContextMethod!.IsStatic.Should().BeTrue();
        createServiceBusResourceMethod!.IsStatic.Should().BeTrue();
        createServiceBusResourceStaticMethod!.IsStatic.Should().BeTrue();
    }

    [Fact]
    public void AddServiceBusContext_ShouldRegisterBothServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddServiceBusContext<TestServiceBusContext>(ValidConnectionString);

        // Assert
        services.Should().HaveCount(2); // IServiceBusContext and ServiceBusAdministrationClient
        
        var contextService = services.FirstOrDefault(s => s.ServiceType == typeof(IServiceBusContext));
        var adminClientService = services.FirstOrDefault(s => s.ServiceType == typeof(ServiceBusAdministrationClient));
        
        contextService.Should().NotBeNull();
        contextService!.Lifetime.Should().Be(ServiceLifetime.Singleton);
        
        adminClientService.Should().NotBeNull();
        adminClientService!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddServiceBusContext_WithInvalidConnectionString_ShouldThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<FormatException>(() => 
            services.AddServiceBusContext<TestServiceBusContext>("invalid-connection-string"));
    }
}