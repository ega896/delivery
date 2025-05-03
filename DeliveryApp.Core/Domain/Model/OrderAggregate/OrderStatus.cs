﻿using System.Diagnostics.CodeAnalysis;

using CSharpFunctionalExtensions;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate;

public class OrderStatus : ValueObject
{
    public static OrderStatus Created => new(nameof(Created).ToLowerInvariant());   
    public static OrderStatus Assigned => new(nameof(Assigned).ToLowerInvariant());   
    public static OrderStatus Completed => new(nameof(Completed).ToLowerInvariant());   
    
    [ExcludeFromCodeCoverage]
    private OrderStatus()
    {
        
    }
    
    private OrderStatus(string name)
    {
        Name = name;
    }
    
    public string Name { get; }
    

    protected override IEnumerable<object> GetEqualityComponents() 
    {
        yield return Name;
    }
}