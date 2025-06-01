using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.SharedKernel;
using Primitives;

namespace DeliveryApp.Core.Ports;

public interface IGeoService
{
    Task<Result<Location, Error>> GetAddress(string street, CancellationToken cancellationToken);
}