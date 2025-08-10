using Clear.ServiceBusInitializer.Entities;

namespace Clear.ServiceBusInitializer.Tests.Entities;

public class FilterTests
{
    [Fact]
    public void Constructor_ShouldCreateFilterWithCorrectNameAndExpression()
    {
        // Arrange
        const string name = "test-filter";
        const string sqlExpression = "sys.Label='TestLabel'";

        // Act
        var filter = new Filter(name, sqlExpression);

        // Assert
        filter.Name.Should().Be("sbsr-test-filter");
        filter.SqlExpression.Should().Be(sqlExpression);
    }

    [Fact]
    public void CreateLabel_ShouldCreateCorrectLabelFilter()
    {
        // Arrange
        const string label = "MyLabel";

        // Act
        var filter = Filter.CreateLabel(label);

        // Assert
        filter.Name.Should().Be("sbsr-mylabel");
        filter.SqlExpression.Should().Be("sys.Label='MyLabel'");
    }

    [Theory]
    [InlineData("OrderCreated", "sbsr-ordercreated", "sys.Label='OrderCreated'")]
    [InlineData("User Created", "sbsr-user-created", "sys.Label='User Created'")]
    [InlineData("payment-processed", "sbsr-payment-processed", "sys.Label='payment-processed'")]
    public void CreateLabel_ShouldHandleDifferentLabelFormats(string label, string expectedName, string expectedExpression)
    {
        // Act
        var filter = Filter.CreateLabel(label);

        // Assert
        filter.Name.Should().Be(expectedName);
        filter.SqlExpression.Should().Be(expectedExpression);
    }

    [Fact]
    public void Filter_ShouldInheritFromEntityWithPrefix()
    {
        // Act
        var filter = new Filter("test", "expression");

        // Assert
        filter.Should().BeAssignableTo<EntityWithPrefix>();
    }
}