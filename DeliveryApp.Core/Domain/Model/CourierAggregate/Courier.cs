using System.Diagnostics.CodeAnalysis;

using CSharpFunctionalExtensions;

using DeliveryApp.Core.Domain.SharedKernel;

using Primitives;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

public class Courier : Aggregate<Guid>
{
    [ExcludeFromCodeCoverage]
    private Courier()
    {
        
    }

    private Courier(string name, Transport transport, Location location)
    {
        Id = Guid.NewGuid();
        Name = name;
        Transport = transport;
        Location = location;
        Status = CourierStatus.Free;
    }
    
    public string Name { get; }
    
    public Transport Transport { get; }
    
    public Location Location { get; private set; }
    
    public CourierStatus Status { get; private set; }
    
    public static Result<Courier, Error> Create(string name, string transportName, int transportSpeed, Location location)
    {
        if (string.IsNullOrWhiteSpace(name)) return GeneralErrors.ValueIsRequired(nameof(name));
        
        var transportCreationResult = Transport.Create(transportName, transportSpeed);
        if (transportCreationResult.IsFailure) return transportCreationResult.Error;
        
        if (location == null) return GeneralErrors.ValueIsRequired(nameof(location));

        return new Courier(name, transportCreationResult.Value, location);
    }
    
    public UnitResult<Error> Move(Location target)
    {
        if (target == null) return GeneralErrors.ValueIsRequired(nameof(target));
        
        if (Status != CourierStatus.Free) return GeneralErrors.ValueIsInvalid(nameof(Status));

        var newLocation = Transport.Move(Location, target).Value;

        Location = newLocation;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> SetFree()
    {
        Status = CourierStatus.Free;
        
        return UnitResult.Success<Error>();
    }
    
    public UnitResult<Error> SetBusy()
    {
        if (Status != CourierStatus.Free) return GeneralErrors.ValueIsInvalid(nameof(Status));
        
        Status = CourierStatus.Busy;
        
        return UnitResult.Success<Error>();
    }

    public Result<float, Error> CalculateTimeToLocation(Location location)
    {
        if (location == null) return GeneralErrors.ValueIsRequired(nameof(location));

        var distance = Location.DistanceTo(location).Value;
        var time = (float)distance / Transport.Speed;

        return time;
    }
}