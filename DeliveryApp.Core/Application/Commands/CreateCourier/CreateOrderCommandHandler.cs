using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.Commands.CreateCourier;

public class CreateCourierCommandHandler(
    ICouriersRepository couriersRepository, 
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCourierCommand, UnitResult<Error>>
{
    public async Task<UnitResult<Error>> Handle(CreateCourierCommand request, CancellationToken cancellationToken)
    {
        var location = Location.CreateRandom();
        
        var transportName = request.Speed switch
        {
            1 => "pedestrian",
            2 => "bicycle",
            3 => "car",
            _ => "unknown"
        };
        
        var createCourierResult = Courier.Create(request.Name, transportName, request.Speed, location);
        if (createCourierResult.IsFailure) return createCourierResult;
        var courier = createCourierResult.Value;
        
        await couriersRepository.AddAsync(courier);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }
}