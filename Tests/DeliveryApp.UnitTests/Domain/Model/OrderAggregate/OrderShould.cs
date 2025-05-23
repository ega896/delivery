using System;
using System.Collections.Generic;

using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;

using FluentAssertions;

using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.OrderAggregate;

public class OrderShould
{
    public static IEnumerable<object[]> GetInvalidOrders()
    {
        yield return [Guid.Empty, Location.Create(1, 1).Value];
        yield return [Guid.NewGuid(), null];
    }
    
    [Fact]
    public void BeCorrectWhenParamsAreCorrectOnCreated()
    {
        // Arrange
        var id = Guid.NewGuid();
        var location = Location.Create(1, 1).Value;

        // Act
        var order = Order.Create(id, location);

        // Assert
        order.IsSuccess.Should().BeTrue();
        order.Value.Id.Should().Be(id);
        order.Value.CourierId.Should().Be(null);
        order.Value.Status.Should().Be(OrderStatus.Created);
        order.Value.Location.Should().Be(location);
    }
    
    [Theory]
    [MemberData(nameof(GetInvalidOrders))]
    public void ReturnErrorWhenParamsAreIncorrectOnCreated(Guid id, Location location)
    {
        // Arrange

        // Act
        var order = Order.Create(id, location);

        // Assert
        order.IsFailure.Should().BeTrue();
        order.Error.Should().NotBeNull();
    }

    [Fact]
    public void BeAssignedToTheCourier()
    {
        // Arrange
        var id = Guid.NewGuid();
        var location = Location.Create(1, 1).Value;
        var order = Order.Create(id, location).Value;
        var courierId = Guid.NewGuid();
        
        // Act
        var result = order.Assign(courierId);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        order.CourierId.Should().Be(courierId);
        order.Status.Should().Be(OrderStatus.Assigned);
    }
    
    [Fact]
    public void NotBeAssignedToTheEmptyCourier()
    {
        // Arrange
        var id = Guid.NewGuid();
        var location = Location.Create(1, 1).Value;
        var order = Order.Create(id, location);
        var courierId = Guid.Empty;
        
        // Act
        var result = order.Value.Assign(courierId);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public void NotBeAssignedToTheCourierWhenStatusIsNotCreated()
    {
        // Arrange
        var id = Guid.NewGuid();
        var location = Location.Create(1, 1).Value;
        var order = Order.Create(id, location);
        var courierId = Guid.NewGuid();
        
        order.Value.Assign(courierId);
        
        // Act
        var result = order.Value.Assign(courierId);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
    
    [Fact]
    public void BeCompleted()
    {
        // Arrange
        var id = Guid.NewGuid();
        var location = Location.Create(1, 1).Value;
        var order = Order.Create(id, location).Value;
        var courierId = Guid.NewGuid();
        
        order.Assign(courierId);
        
        // Act
        var result = order.Complete();
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Completed);
    }
    
    [Fact]
    public void NotBeCompletedWhenStatusIsNotAssigned()
    {
        // Arrange
        var id = Guid.NewGuid();
        var location = Location.Create(1, 1).Value;
        var order = Order.Create(id, location);
        
        // Act
        var result = order.Value.Complete();
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
}