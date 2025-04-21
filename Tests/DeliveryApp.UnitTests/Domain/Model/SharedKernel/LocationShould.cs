using DeliveryApp.Core.Domain.SharedKernel;

using FluentAssertions;

using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.SharedKernel;

public class LocationShould
{
    [Fact]
    public void BeCorrectWhenParamsAreCorrectOnCreated()
    {
        // Arrange
        const int x = 1;
        const int y = 2;
        
        // Act
        var location = Location.Create(x, y);
        
        // Assert
        location.IsSuccess.Should().BeTrue();
        location.Value.X.Should().Be(x);
        location.Value.Y.Should().Be(y);
    }
    
    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(-1, -1)]
    [InlineData(11, 11)]
    public void ReturnErrorWhenParamsAreIncorrectOnCreated(int x, int y)
    {
        // Arrange
        
        // Act
        var location = Location.Create(x, y);
        
        // Assert
        location.IsFailure.Should().BeTrue();
        location.Error.Should().NotBeNull();
    }
    
    [Fact]
    public void BeEqualWhenParamsAreEqual()
    {
        // Arrange
        const int x = 1;
        const int y = 2;
        
        var location1 = Location.Create(x, y).Value;
        var location2 = Location.Create(x, y).Value;
        
        // Act
        var comparisonResult = location1.Equals(location2);
        
        // Assert
        comparisonResult.Should().BeTrue();
    }
    
    [Fact]
    public void BeNotEqualWhenParamsAreNotEqual()
    {
        // Arrange
        var location1 = Location.Create(1, 2).Value;
        var location2 = Location.Create(3, 4).Value;
        
        // Act
        var comparisonResult = location1.Equals(location2);
        
        // Assert
        comparisonResult.Should().BeFalse();
    }
    
    [Fact]
    public void CalculateDistanceCorrectly()
    {
        // Arrange
        var location1 = Location.Create(1, 2).Value;
        var location2 = Location.Create(3, 4).Value;
        
        // Act
        var distance = location1.DistanceTo(location2);
        
        // Assert
        distance.Should().Be(4);
    }
    
    [Fact]
    public void CreatedRandomlyAccordingToRules()
    {
        // Arrange
        
        // Act
        var randomLocation = Location.CreateRandom();
        
        // Assert
        randomLocation.Should().NotBeNull();
        randomLocation.X.Should().BeInRange(Location.MinCoordinate, Location.MaxCoordinate);
        randomLocation.Y.Should().BeInRange(Location.MinCoordinate, Location.MaxCoordinate);
    }
}