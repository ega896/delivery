using System.Diagnostics.CodeAnalysis;

using CSharpFunctionalExtensions;

using DeliveryApp.Core.Domain.SharedKernel;

using Primitives;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate;

public class Order : Entity<Guid>
{
    [ExcludeFromCodeCoverage]
    private Order()
    {
        
    }
    
    private Order(Guid id, Location location)
    {
        Id = id;
        Status = OrderStatus.Created;
        Location = location;
    }
    
    public Guid CourierId { get; private set; }
    
    public OrderStatus Status { get; private set; }
    
    public Location Location { get; }
    
    public static Result<Order, Error> Create(Guid id, Location location)
    {
        if (id == Guid.Empty) return GeneralErrors.ValueIsRequired(nameof(location));
        
        if (location == null) return GeneralErrors.ValueIsRequired(nameof(location));

        return new Order(id, location);
    }
    
    public UnitResult<Error> Assign(Guid courierId)
    {
        if (courierId == Guid.Empty) return GeneralErrors.ValueIsRequired(nameof(courierId));
        
        if (Status != OrderStatus.Created) return GeneralErrors.ValueIsInvalid(nameof(Status));

        CourierId = courierId;
        Status = OrderStatus.Assigned;

        return UnitResult.Success<Error>();;
    }
    
    public UnitResult<Error> Complete()
    {
        if (Status != OrderStatus.Assigned) return GeneralErrors.ValueIsInvalid(nameof(Status));

        Status = OrderStatus.Completed;

        return UnitResult.Success<Error>();;
    }
}