using System.Diagnostics.CodeAnalysis;

using CSharpFunctionalExtensions;

using Primitives;

namespace DeliveryApp.Core.Domain.SharedKernel;

public class Location : ValueObject
{
    public const int MinCoordinate = 1;
    public const int MaxCoordinate = 10;
    
    public int X { get; }
    
    public int Y { get; }

    [ExcludeFromCodeCoverage]
    private Location()
    {
        
    }
    
    private Location(int x, int y) : this()
    {
        X = x;
        Y = y;
    }
    
    public static Result<Location, Error> Create(int x, int y)
    {
        if (x < MinCoordinate || x > MaxCoordinate) return GeneralErrors.ValueIsInvalid(nameof(x));
        
        if (y < MinCoordinate || y > MaxCoordinate) return GeneralErrors.ValueIsInvalid(nameof(y));

        return new Location(x, y);
    }

    public Result<int, Error> DistanceTo(Location targetLocation)
    {
        if (targetLocation == null) return GeneralErrors.ValueIsRequired(nameof(targetLocation));
        
        var xDiff = Math.Abs(X - targetLocation.X);
        var yDiff = Math.Abs(Y - targetLocation.Y);

        return xDiff + yDiff;
    }

    public static Location CreateRandom()
    {
        var random = Random.Shared;
        var x = random.Next(MinCoordinate, MaxCoordinate);
        var y = random.Next(MinCoordinate, MaxCoordinate);
        return new Location(x, y);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
    }
}