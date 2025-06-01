using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Services.Dispatch;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.Commands.AssignOrder;

public class AssignOrderCommandHandler(
    ICouriersRepository couriersRepository,
    IOrdersRepository ordersRepository, 
    IDispatchService dispatchService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AssignOrderCommand, UnitResult<Error>>
{
    public async Task<UnitResult<Error>> Handle(AssignOrderCommand request, CancellationToken cancellationToken)
    {
        var getUnassignedOrderResult = await ordersRepository.GetFirstCreatedAsync();
        if (getUnassignedOrderResult.HasNoValue) return UnitResult.Success<Error>();
        var unassignedOrder = getUnassignedOrderResult.Value;

        var freeCouriers = couriersRepository.GetAllFree().ToList();
        if (freeCouriers.Count == 0) return CouriersErrors.NoFreeCouriers();
        
        var dispatchOrderResult = dispatchService.Dispatch(unassignedOrder, freeCouriers);
        if (dispatchOrderResult.IsFailure) return dispatchOrderResult;
        var courier = dispatchOrderResult.Value;
        
        ordersRepository.Update(unassignedOrder);
        couriersRepository.Update(courier);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }
}