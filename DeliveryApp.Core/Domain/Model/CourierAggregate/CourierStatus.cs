using System.Diagnostics.CodeAnalysis;

using CSharpFunctionalExtensions;

public class CourierStatus : ValueObject
{
    public static CourierStatus Free => new(nameof(Free).ToLowerInvariant());  
    public static CourierStatus Busy => new(nameof(Busy).ToLowerInvariant());  
    
    [ExcludeFromCodeCoverage]
    private CourierStatus()
    {
        
    }

    private CourierStatus(string name)
    {
        Name = name;
    }

    public string Name { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}