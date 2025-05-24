using DeliveryApp.Core.Application.Commands.CreateCourier;
using DeliveryApp.Core.Application.Commands.CreateOrder;
using DeliveryApp.Core.Application.Queries.GetCouriers;
using DeliveryApp.Core.Application.Queries.GetUncompletedOrders;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenApi.Controllers;
using OpenApi.Models;

namespace DeliveryApp.Api.Adapters.Http;

public class DeliveryController(IMediator mediator) : DefaultApiController
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    
    public override async Task<IActionResult> CreateCourier(NewCourier newCourier)
    {
        var createCourierCommand = new CreateCourierCommand(newCourier.Name, newCourier.Speed);
        
        var response = await _mediator.Send(createCourierCommand);
        return response.IsSuccess ? Ok() : Conflict();
    }

    public override async Task<IActionResult> CreateOrder()
    {
        var orderId = Guid.NewGuid();
        var street = "Несуществующая";
        var createOrderCommand = new CreateOrderCommand(orderId, street);
        
        var response = await _mediator.Send(createOrderCommand);
        return response.IsSuccess ? Ok() : Conflict();
    }

    public override async Task<IActionResult> GetCouriers()
    {
        var getUncompletedOrdersQuery = new GetCouriersQuery();
        var response = await _mediator.Send(getUncompletedOrdersQuery);
        
        var model = response.Couriers.Select(c => new Courier
        {
            Id = c.Id,
            Name = c.Name,
            Location = new Location { X = c.Location.X, Y = c.Location.Y }
        });
        
        return Ok(model);
    }

    public override async Task<IActionResult> GetActiveOrders()
    {
        var getUncompletedOrdersQuery = new GetUncompletedOrdersQuery();
        var response = await _mediator.Send(getUncompletedOrdersQuery);
        
        var model = response.Orders.Select(o => new Order
        {
            Id = o.Id,
            Location = new Location { X = o.Location.X, Y = o.Location.Y }
        });
        
        return Ok(model);
    }
}