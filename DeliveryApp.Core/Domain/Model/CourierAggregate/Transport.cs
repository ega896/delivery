using System.Diagnostics.CodeAnalysis;

using CSharpFunctionalExtensions;

using DeliveryApp.Core.Domain.SharedKernel;

using Primitives;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

public sealed class Transport : Entity<Guid>
{
    [ExcludeFromCodeCoverage]
    private Transport()
    {
        
    }

    private Transport(string name, int speed)
    {
        Id = Guid.NewGuid();
        Name = name;
        Speed = speed;
    }
    
    public string Name { get; }
    
    public int Speed { get; }
    
    public static Result<Transport, Error> Create(string name, int speed)
    {
        if (string.IsNullOrWhiteSpace(name)) return GeneralErrors.ValueIsRequired(nameof(name));
        
        if (speed is < 1 or > 3) return GeneralErrors.ValueIsInvalid(nameof(speed));

        return new Transport(name, speed);
    }

    public Result<Location, Error> Move(Location from, Location to)
    {
        if (from == null) return GeneralErrors.ValueIsRequired(nameof(from));
        if (to == null) return GeneralErrors.ValueIsRequired(nameof(to));

        if (from == to) return to;

        var xDiff = to.X - from.X;
        var yDiff = to.Y - from.Y;
        
        var stepsLeft = Speed;

        var moveX = Math.Clamp(xDiff, -stepsLeft, stepsLeft);
        stepsLeft -= Math.Abs(moveX);

        var moveY = Math.Clamp(yDiff, -stepsLeft, stepsLeft);

        var newLocation = Location.Create(from.X + moveX, from.Y + moveY).Value;
        return newLocation;
    }
}