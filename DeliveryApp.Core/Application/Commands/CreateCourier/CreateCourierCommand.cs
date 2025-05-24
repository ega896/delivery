using CSharpFunctionalExtensions;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.Commands.CreateCourier;

public class CreateCourierCommand(string name, int speed) : IRequest<UnitResult<Error>>
{
    public string Name { get; } = name;
    
    public int Speed { get; } = speed;
}