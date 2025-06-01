using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DeliveryApp.Api.Adapters.Http;
using DeliveryApp.Core.Application.Commands.CreateCourier;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using OpenApi.Models;
using Xunit;

using Error = Primitives.Error;

namespace DeliveryApp.UnitTests.Ui.Http;

public class DeliveryControllerShould
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    
    [Fact]
    public async Task CreateCourierCorrectly()
    {
        // Arrange
        var successResult = UnitResult.Success<Error>();
        _mediator.Send(Arg.Any<CreateCourierCommand>()).Returns(Task.FromResult(successResult));
        
        // Act
        var deliveryController = new DeliveryController(_mediator);
        var result = await deliveryController.CreateCourier(new NewCourier { Name = "Test Courier", Speed = 1 });

        // Assert
        result.Should().BeOfType<OkResult>();
    }
    
    [Fact]
    public async Task ReturnConflictWhenCourierCreationFails()
    {
        // Arrange
        var errorResult = UnitResult.Failure(new Error("test", "test"));
        _mediator.Send(Arg.Any<CreateCourierCommand>()).Returns(Task.FromResult(errorResult));
        
        // Act
        var deliveryController = new DeliveryController(_mediator);
        var result = await deliveryController.CreateCourier(new NewCourier { Name = "Test Courier", Speed = 1 });

        // Assert
        result.Should().BeOfType<ConflictResult>();
    }
}