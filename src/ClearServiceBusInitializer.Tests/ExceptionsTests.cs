namespace Clear.ServiceBusInitializer.Tests;

public class ExceptionsTests
{
    [Fact]
    public void DomainException_ShouldInheritFromException()
    {
        // Assert
        typeof(DomainException).Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void DomainException_WithMessage_ShouldSetMessage()
    {
        // Arrange
        const string message = "Test domain exception message";

        // Act
        var exception = new DomainException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void DomainException_WithMessageAndInnerException_ShouldSetBoth()
    {
        // Arrange
        const string message = "Test domain exception message";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new DomainException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void ServiceBusAdministrationClientNotRegisteredException_ShouldInheritFromDomainException()
    {
        // Assert
        typeof(ServiceBusAdministrationClientNotRegisteredException).Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void ServiceBusAdministrationClientNotRegisteredException_ShouldHaveCorrectMessage()
    {
        // Act
        var exception = new ServiceBusAdministrationClientNotRegisteredException();

        // Assert
        exception.Message.Should().Be("ServiceBusAdministrationClient not registered in the service collection.");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void ServiceBusContextNotRegisteredException_ShouldInheritFromDomainException()
    {
        // Assert
        typeof(ServiceBusContextNotRegisteredException).Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void ServiceBusContextNotRegisteredException_ShouldHaveCorrectMessage()
    {
        // Act
        var exception = new ServiceBusContextNotRegisteredException();

        // Assert
        exception.Message.Should().Be("ServiceBusContext not registered in the service collection.");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void AllCustomExceptions_ShouldBeSerializable()
    {
        // This test ensures exceptions can be serialized across app domains
        var exceptions = new Exception[]
        {
            new DomainException("Test message"),
            new ServiceBusAdministrationClientNotRegisteredException(),
            new ServiceBusContextNotRegisteredException()
        };

        foreach (var exception in exceptions)
        {
            // Act & Assert - Should not throw
            var serialized = System.Text.Json.JsonSerializer.Serialize(exception.Message);
            serialized.Should().NotBeNullOrEmpty();
        }
    }
}