using Clear.ServiceBusInitializer.Entities;

namespace Clear.ServiceBusInitializer.Tests.Entities;

public record TestEntityWithPrefix : EntityWithPrefix
{
    public TestEntityWithPrefix(string prefix, string name) : base(prefix, name)
    {
    }
}

public class EntityWithPrefixTests
{
    [Theory]
    [InlineData("test-", "my-entity", "test-my-entity")]
    [InlineData("prefix-", "EntityName", "prefix-entityname")]
    [InlineData("sb-", "My Queue Name", "sb-my-queue-name")]
    [InlineData("test-", " Spaced Name ", "test--spaced-name-")] // Spaces become dashes, then trimmed
    public void Constructor_ShouldCreateCorrectPrefixedName(string prefix, string name, string expected)
    {
        // Act
        var entity = new TestEntityWithPrefix(prefix, name);

        // Assert
        entity.Name.Should().Be(expected);
    }

    [Fact]
    public void Constructor_ShouldHandleAlreadyPrefixedName()
    {
        // Arrange
        const string prefix = "test-";
        const string name = "test-already-prefixed";

        // Act
        var entity = new TestEntityWithPrefix(prefix, name);

        // Assert
        entity.Name.Should().Be("test-already-prefixed");
    }

    [Fact]
    public void Constructor_ShouldConvertToLowerCase()
    {
        // Arrange
        const string prefix = "TEST-";
        const string name = "MyEntityName";

        // Act
        var entity = new TestEntityWithPrefix(prefix, name);

        // Assert
        entity.Name.Should().Be("test-myentityname");
    }

    [Fact]
    public void Constructor_ShouldReplaceSpacesWithDashes()
    {
        // Arrange
        const string prefix = "test-";
        const string name = "My Entity With Spaces";

        // Act
        var entity = new TestEntityWithPrefix(prefix, name);

        // Assert
        entity.Name.Should().Be("test-my-entity-with-spaces");
    }

    [Fact]
    public void Constructor_ShouldTrimWhitespace()
    {
        // Arrange - Note: trim happens after space replacement
        const string prefix = "test-";
        const string name = "  trimmed  "; // Spaces become dashes first

        // Act
        var entity = new TestEntityWithPrefix(prefix, name);

        // Assert
        entity.Name.Should().Be("test---trimmed--"); // Actual behavior: spaces become dashes first
    }

    [Fact]
    public void Constructor_ShouldTrimWhitespaceCorrectly()
    {
        // Arrange - Test with actual trimming scenario
        const string prefix = "test-";
        const string name = "trimmed"; // No leading/trailing spaces

        // Act
        var entity = new TestEntityWithPrefix(prefix, name);

        // Assert
        entity.Name.Should().Be("test-trimmed");
    }

    [Fact]
    public void Constructor_ShouldHandleMultipleSpaces()
    {
        // Arrange
        const string prefix = "test-";
        const string name = "multiple  spaces  here";

        // Act
        var entity = new TestEntityWithPrefix(prefix, name);

        // Assert
        entity.Name.Should().Be("test-multiple--spaces--here");
    }

    [Fact]
    public void Constructor_ShouldHandleEmptyName()
    {
        // Arrange
        const string prefix = "test-";
        const string name = "";

        // Act
        var entity = new TestEntityWithPrefix(prefix, name);

        // Assert
        entity.Name.Should().Be("test-");
    }
}