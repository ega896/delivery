using System;
using System.Collections.Generic;

using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Services.Dispatch;
using DeliveryApp.Core.Domain.SharedKernel;

using FluentAssertions;

using Xunit;

namespace DeliveryApp.UnitTests.Domain.Services;

public class DispatchShould
{
    public static IEnumerable<object[]> GetInvalidDispatchPreconditions()
    {
        yield return [null, new List<Courier> { Courier.Create("Ivan", "car", 3, Location.Create(1, 1).Value).Value }];
        yield return [Order.Create(Guid.NewGuid(), Location.Create(5, 5).Value).Value, null];
        yield return [Order.Create(Guid.NewGuid(), Location.Create(5, 5).Value).Value, new List<Courier>()];
        
        var courier = Courier.Create("Ivan", "car", 3, Location.Create(1, 1).Value).Value;
        courier.SetBusy();
        yield return [Order.Create(Guid.NewGuid(), Location.Create(5, 5).Value).Value, new List<Courier> { courier }];
    }
    
    [Fact]
    public void BeDoneWithCourierThatWillFasterReachToOrderLocation()
    {
        var courier1 = Courier.Create("Ivan", "car", 3, Location.Create(1, 1).Value);
        var courier2 = Courier.Create("Petr", "bicycle", 2, Location.Create(2, 2).Value);
        var courier3 = Courier.Create("Pavel", "pedestrian", 1, Location.Create(3, 3).Value);
        
        var order = Order.Create(Guid.NewGuid(), Location.Create(5, 5).Value);
        
        var dispatchService = new DispatchService();
        var result = dispatchService.Dispatch(order.Value, [courier1.Value, courier2.Value, courier3.Value]);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(courier1.Value);
    }
    
    [Theory]
    [MemberData(nameof(GetInvalidDispatchPreconditions))]
    public void ReturnErrorWhenParamsAreIncorrect(Order order, IReadOnlyCollection<Courier> couriers)
    {
        // Arrange
        
        // Act
        var dispatchService = new DispatchService();
        var result = dispatchService.Dispatch(order, couriers);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
    }
}