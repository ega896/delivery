using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Application.Commands.AssignOrder;
using DeliveryApp.Core.Application.Commands.CreateOrder;
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

public class CreateOrderCommandShould
{
    private readonly IOrdersRepository _ordersRepositoryMock = Substitute.For<IOrdersRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();
    private readonly IGeoService _geoServiceMock = Substitute.For<IGeoService>();
    
    [Fact]
    public async Task ReturnSuccessWhenParamsAreCorrectOnCreated()
    {
        // Arrange
        _ordersRepositoryMock.GetAsync(Arg.Any<Guid>())
            .Returns(Task.FromResult(Maybe<Order>.From((Order)null)));
       
        _geoServiceMock.GetAddress(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<Location, Error>(Location.CreateRandom()));

        // Act
        var commandHandler = new CreateOrderCommandHandler(_ordersRepositoryMock, _unitOfWorkMock, _geoServiceMock);

        var id = Guid.NewGuid();
        const string street = "Test Street";
        var result = await commandHandler.Handle(new CreateOrderCommand(id, street), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public async Task ReturnSuccessWhenOrderWithTheSameIdWasCreatedPreviously()
    {
        // Arrange
        _ordersRepositoryMock.GetAsync(Arg.Any<Guid>())
            .Returns(Task.FromResult(Maybe<Order>.From(Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value)));
        
        _geoServiceMock.GetAddress(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<Location, Error>(Location.CreateRandom()));

        // Act
        var commandHandler = new CreateOrderCommandHandler(_ordersRepositoryMock, _unitOfWorkMock, _geoServiceMock);

        var id = Guid.NewGuid();
        const string street = "Test Street";
        var result = await commandHandler.Handle(new CreateOrderCommand(id, street), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
    
    [Fact]
    public async Task ReturnFailureWhenAddressLocationSearchFailed()
    {
        // Arrange
        _ordersRepositoryMock.GetAsync(Arg.Any<Guid>())
            .Returns(Task.FromResult(Maybe<Order>.From((Order)null)));
        
        _geoServiceMock.GetAddress(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<Location, Error>(new Error("location.undefined", "Location is undefined")));

        // Act
        var commandHandler = new CreateOrderCommandHandler(_ordersRepositoryMock, _unitOfWorkMock, _geoServiceMock);

        var id = Guid.NewGuid();
        const string street = "Test Street";
        var result = await commandHandler.Handle(new CreateOrderCommand(id, street), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}