using CSharpFunctionalExtensions;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.Commands.MoveCouriers;

public class MoveCouriersCommandHandler(
    ICouriersRepository couriersRepository, 
    IOrdersRepository ordersRepository, 
    IUnitOfWork unitOfWork)
    : IRequestHandler<MoveCouriersCommand, UnitResult<Error>>
{
    public async Task<UnitResult<Error>> Handle(MoveCouriersCommand request, CancellationToken cancellationToken)
    {
        var assignedOrders = ordersRepository.GetAllAssigned().ToList();
        if (assignedOrders.Count == 0) return UnitResult.Success<Error>();

        foreach (var assignedOrder in assignedOrders)
        {
            if (!assignedOrder.CourierId.HasValue)
            {
                // log error or warn
                continue;
            }

            var getCourierResult = await couriersRepository.GetAsync(assignedOrder.CourierId.Value);
            if (getCourierResult.HasNoValue)
            {
                // log error
                continue;
            }
            
            var courier = getCourierResult.Value;
            var moveCourierResult = courier.Move(assignedOrder.Location);
            if (moveCourierResult.IsFailure)
            {
                // log error
                continue;
            }

            if (assignedOrder.Location.Equals(courier.Location))
            {
                assignedOrder.Complete();
                courier.SetFree();
            }
            
            couriersRepository.Update(courier);
            ordersRepository.Update(assignedOrder);
        }
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }
}