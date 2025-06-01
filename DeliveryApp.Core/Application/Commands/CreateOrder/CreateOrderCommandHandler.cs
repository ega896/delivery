using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.Commands.CreateOrder;

public class CreateOrderCommandHandler(
    IOrdersRepository ordersRepository, 
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateOrderCommand, UnitResult<Error>>
{
    public async Task<UnitResult<Error>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var getOrderResult = await ordersRepository.GetAsync(request.BasketId);
        if (getOrderResult.HasValue) return UnitResult.Success<Error>();
        
        var createOrderResult = Order.Create(request.BasketId, Location.CreateRandom());
        if (createOrderResult.IsFailure) return createOrderResult;
        var order = createOrderResult.Value;
        
        await ordersRepository.AddAsync(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }
}