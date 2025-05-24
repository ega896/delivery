using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Application.Commands.AssignOrder;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Services.Dispatch;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Core.Ports;
using FluentAssertions;
using NSubstitute;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Application.Commands;

public class AssignOrderCommandShould
{
    private readonly ICouriersRepository _couriersRepositoryMock = Substitute.For<ICouriersRepository>();
    private readonly IOrdersRepository _ordersRepositoryMock = Substitute.For<IOrdersRepository>();
    private readonly IDispatchService _dispatchServiceMock = Substitute.For<IDispatchService>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    
    [Fact]
    public async Task ReturnSuccessWhenNoOrdersAreUnassigned()
    {
        // Arrange
        _ordersRepositoryMock.GetFirstCreatedAsync().Returns(Task.FromResult(Maybe<Order>.From((Order)null)));

        // Act
        var commandHandler = new AssignOrderCommandHandler(
            _couriersRepositoryMock, 
            _ordersRepositoryMock, 
            _dispatchServiceMock, 
            _unitOfWorkMock);

        var result = await commandHandler.Handle(new AssignOrderCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public async Task ReturnFailureWhenNoFreeCouriersAreAvailable()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value;
        _ordersRepositoryMock.GetFirstCreatedAsync().Returns(Task.FromResult(Maybe<Order>.From(order)));
        
        _couriersRepositoryMock.GetAllFree().Returns([]);

        // Act
        var commandHandler = new AssignOrderCommandHandler(
            _couriersRepositoryMock, 
            _ordersRepositoryMock, 
            _dispatchServiceMock, 
            _unitOfWorkMock);

        var result = await commandHandler.Handle(new AssignOrderCommand(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
    
    [Fact]
    public async Task ReturnFailureWhenDispatchFailed()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value;
        _ordersRepositoryMock.GetFirstCreatedAsync().Returns(Task.FromResult(Maybe<Order>.From(order)));
        
        var courier = Courier.Create("Ivan", "car", 3, Location.CreateRandom()).Value;
        _couriersRepositoryMock.GetAllFree().Returns([courier]);
        
        _dispatchServiceMock.Dispatch(Arg.Any<Order>(), Arg.Any<List<Courier>>())
            .Returns(Result.Failure<Courier, Error>(CouriersErrors.NoFreeCouriers()));

        // Act
        var commandHandler = new AssignOrderCommandHandler(
            _couriersRepositoryMock, 
            _ordersRepositoryMock, 
            _dispatchServiceMock, 
            _unitOfWorkMock);

        var result = await commandHandler.Handle(new AssignOrderCommand(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ReturnSuccessAndAssignOrderToTheFreeCourier()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value;
        _ordersRepositoryMock.GetFirstCreatedAsync().Returns(Task.FromResult(Maybe<Order>.From(order)));
        
        var courier = Courier.Create("Ivan", "car", 3, Location.CreateRandom()).Value;
        _couriersRepositoryMock.GetAllFree().Returns([courier]);
        
        _dispatchServiceMock.Dispatch(order, [courier]).Returns(courier);
        
        _unitOfWorkMock.SaveChangesAsync().Returns(Task.FromResult(true));

        // Act
        var commandHandler = new AssignOrderCommandHandler(
            _couriersRepositoryMock, 
            _ordersRepositoryMock, 
            _dispatchServiceMock, 
            _unitOfWorkMock);

        var result = await commandHandler.Handle(new AssignOrderCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}