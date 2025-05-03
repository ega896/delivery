using System.Collections.Generic;

using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.SharedKernel;

using FluentAssertions;

using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate;

public class TransportShould
{
    public static IEnumerable<object[]> GetInvalidLocations()
    {
        yield return [Location.CreateRandom(), null];
        yield return [null, Location.CreateRandom()];
    }

    public static IEnumerable<object[]> GetTransportAndCorrectLocations()
    {
        yield return [Transport.Create("Pedestrian", 1).Value, Location.Create(1, 1).Value, Location.Create(1, 1).Value, Location.Create(1, 1).Value];
        yield return [Transport.Create("Pedestrian", 1).Value, Location.Create(2, 1).Value, Location.Create(1, 1).Value, Location.Create(1, 1).Value];
        yield return [Transport.Create("Pedestrian", 1).Value, Location.Create(1, 2).Value, Location.Create(1, 1).Value, Location.Create(1, 1).Value];
        yield return [Transport.Create("Pedestrian", 1).Value, Location.Create(1, 1).Value, Location.Create(2, 1).Value, Location.Create(2, 1).Value];
        yield return [Transport.Create("Pedestrian", 1).Value, Location.Create(1, 1).Value, Location.Create(1, 2).Value, Location.Create(1, 2).Value];
        yield return [Transport.Create("Pedestrian", 1).Value, Location.Create(1, 3).Value, Location.Create(1, 1).Value, Location.Create(1, 2).Value];
        yield return [Transport.Create("Pedestrian", 1).Value, Location.Create(3, 3).Value, Location.Create(1, 1).Value, Location.Create(2, 3).Value];
        yield return [Transport.Create("Car", 3).Value, Location.Create(3, 3).Value, Location.Create(3, 6).Value, Location.Create(3, 6).Value];
        yield return [Transport.Create("Car", 3).Value, Location.Create(3, 3).Value, Location.Create(6, 6).Value, Location.Create(6, 3).Value];
        yield return [Transport.Create("Car", 3).Value, Location.Create(1, 2).Value, Location.Create(3, 3).Value, Location.Create(3, 3).Value];
    }

    [Fact]
    public void BeCorrectWhenParamsAreCorrectOnCreated()
    {
        // Arrange
        const string name = "car";
        const int speed = 3;

        // Act
        var transport = Transport.Create(name, speed);

        // Assert
        transport.IsSuccess.Should().BeTrue();
        transport.Value.Name.Should().Be(name);
        transport.Value.Speed.Should().Be(speed);
    }

    [Theory]
    [InlineData("", 1)]
    [InlineData("   ", 1)]
    [InlineData(null, 1)]
    [InlineData("car", -1)]
    [InlineData("car", 0)]
    [InlineData("car", 4)]
    public void ReturnErrorWhenParamsAreIncorrectOnCreated(string name, int speed)
    {
        // Arrange

        // Act
        var transport = Transport.Create(name, speed);

        // Assert
        transport.IsFailure.Should().BeTrue();
        transport.Error.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(GetTransportAndCorrectLocations))]
    public void MoveToCorrectLocation(Transport transport, Location from, Location to, Location expected)
    {
        // Arrange

        // Act
        var newLocation = transport.Move(from, to);

        // Assert
        newLocation.IsSuccess.Should().BeTrue();
        newLocation.Value.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(GetInvalidLocations))]
    public void ReturnErrorWhenMoveFromOrToNullLocation(Location from, Location to)
    {
        // Arrange
        var transport = Transport.Create("car", 3).Value;

        // Act
        var newLocation = transport.Move(from, to);

        // Assert
        newLocation.IsSuccess.Should().BeFalse();
        newLocation.Error.Should().NotBeNull();
    }
}