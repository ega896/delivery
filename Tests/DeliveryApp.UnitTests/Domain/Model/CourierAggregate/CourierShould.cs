using System.Collections.Generic;

using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.SharedKernel;

using FluentAssertions;

using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate;

public class CourierShould
{
    public static IEnumerable<object[]> GetInvalidCouriers()
    {
        yield return [null, "car", 3, Location.Create(1, 1).Value];
        yield return [string.Empty, "car", 3, Location.Create(1, 1).Value];
        yield return ["   ", "car", 3, Location.Create(1, 1).Value];
        yield return ["Ivan", null, 3, Location.Create(1, 1).Value];
        yield return ["Ivan", string.Empty, 3, Location.Create(1, 1).Value];
        yield return ["Ivan", "   ", 3, Location.Create(1, 1).Value];
        yield return ["Ivan", "car", 0, Location.Create(1, 1).Value];
        yield return ["Ivan", "car", 4, Location.Create(1, 1).Value];
    }
    
    [Fact]
    public void BeCorrectWhenParamsAreCorrectOnCreated()
    {
        // Arrange
        const string name = "Ivan";
        const string transportName = "car";
        const int transportSpeed = 3;
        var location = Location.Create(1, 1).Value;

        // Act
        var courier = Courier.Create(name, transportName, transportSpeed, location);

        // Assert
        courier.IsSuccess.Should().BeTrue();
        courier.Value.Name.Should().Be(name);
        courier.Value.Transport.Name.Should().Be(transportName);
        courier.Value.Transport.Speed.Should().Be(transportSpeed);
        courier.Value.Location.Should().Be(location);
    }
    
    [Theory]
    [MemberData(nameof(GetInvalidCouriers))]
    public void ReturnErrorWhenParamsAreIncorrectOnCreated(string name, string transportName, int transportSpeed, Location location)
    {
        // Arrange

        // Act
        var courier = Courier.Create(name, transportName, transportSpeed, location);

        // Assert
        courier.IsFailure.Should().BeTrue();
        courier.Error.Should().NotBeNull();
    }

    [Fact]
    public void MoveToCorrectLocation()
    {
        // Arrange
        const string name = "Ivan";
        const string transportName = "car";
        const int transportSpeed = 3;
        var location = Location.Create(1, 1).Value;
        var courier = Courier.Create(name, transportName, transportSpeed, location);
        var target = Location.Create(3, 3).Value;
        
        // Act
        var movedCourier = courier.Value.Move(target);
        
        // Assert
        movedCourier.IsSuccess.Should().BeTrue();
        movedCourier.Value.Location.Should().Be(Location.Create(3, 2).Value);
    }
    
    [Fact]
    public void NotMoveToIncorrectLocation()
    {
        // Arrange
        const string name = "Ivan";
        const string transportName = "car";
        const int transportSpeed = 3;
        var location = Location.Create(1, 1).Value;
        var courier = Courier.Create(name, transportName, transportSpeed, location);
        
        // Act
        var movedCourier = courier.Value.Move(null);
        
        // Assert
        movedCourier.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void BeSetFree()
    {
        // Arrange
        const string name = "Ivan";
        const string transportName = "car";
        const int transportSpeed = 3;
        var location = Location.Create(1, 1).Value;
        var courier = Courier.Create(name, transportName, transportSpeed, location);
        
        // Act
        var result = courier.Value.SetFree();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(CourierStatus.Free);
    }
    
    [Fact]
    public void BeSetBusy()
    {
        // Arrange
        const string name = "Ivan";
        const string transportName = "car";
        const int transportSpeed = 3;
        var location = Location.Create(1, 1).Value;
        var courier = Courier.Create(name, transportName, transportSpeed, location);
        
        // Act
        var result = courier.Value.SetBusy();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(CourierStatus.Busy);
    }
    
    [Fact]
    public void NotBeSetBusyWhenAlreadyBusy()
    {
        // Arrange
        const string name = "Ivan";
        const string transportName = "car";
        const int transportSpeed = 3;
        var location = Location.Create(1, 1).Value;
        var courier = Courier.Create(name, transportName, transportSpeed, location);
        var busyCourier = courier.Value.SetBusy();
        
        // Act
        var result = busyCourier.Value.SetBusy();
        
        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void CalculateTimeToCorrectLocation()
    {
        // Assert
        const string name = "Ivan";
        const string transportName = "car";
        const int transportSpeed = 3;
        var location = Location.Create(1, 1).Value;
        var courier = Courier.Create(name, transportName, transportSpeed, location);
        var target = Location.Create(4, 4).Value;
        
        // Act
        var movedCourier = courier.Value.CalculateTimeToLocation(target);
        
        // Assert
        movedCourier.IsSuccess.Should().BeTrue();
        movedCourier.Value.Should().Be(2.0f);
    } 
    
    [Fact]
    public void NotCalculateTimeToIncorrectLocation()
    {
        // Assert
        const string name = "Ivan";
        const string transportName = "car";
        const int transportSpeed = 3;
        var location = Location.Create(1, 1).Value;
        var courier = Courier.Create(name, transportName, transportSpeed, location);
        
        // Act
        var movedCourier = courier.Value.CalculateTimeToLocation(null);
        
        // Assert
        movedCourier.IsSuccess.Should().BeFalse();
    }
}