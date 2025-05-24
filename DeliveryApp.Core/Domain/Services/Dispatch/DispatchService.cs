using CSharpFunctionalExtensions;

using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;

using Primitives;

namespace DeliveryApp.Core.Domain.Services.Dispatch;

public class DispatchService : IDispatchService
{
    public Result<Courier, Error> Dispatch(Order order, IReadOnlyCollection<Courier> couriers)
    {
        if (order == null) return GeneralErrors.ValueIsRequired(nameof(order));
        
        if (couriers == null || couriers.Count == 0) return GeneralErrors.ValueIsRequired(nameof(couriers));
        
        var freeCouriers = couriers.Where(c => c.Status == CourierStatus.Free).ToList();
        if (freeCouriers.Count == 0) return CouriersErrors.NoFreeCouriers();
        
        var closestCourier = freeCouriers
            .OrderBy(c => c.CalculateTimeToLocation(order.Location).Value)
            .First();
        
        var orderAssignResult = order.Assign(closestCourier.Id);
        if (orderAssignResult.IsFailure) return orderAssignResult.Error;

        var courierSetBusyResult = closestCourier.SetBusy();
        if (courierSetBusyResult.IsFailure) return courierSetBusyResult.Error;

        return closestCourier;
    }
}