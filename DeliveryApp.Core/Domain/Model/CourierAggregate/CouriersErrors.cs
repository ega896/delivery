using Primitives;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

public static class CouriersErrors
{
    public static Error NoFreeCouriers() => new("no.free.couriers", $"No free couriers available.");
}