using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using Primitives;

namespace DeliveryApp.Core.Ports;

public interface ICouriersRepository : IRepository<Courier>
{
    Task AddAsync(Courier courier);
    
    void Update(Courier courier);
    
    Task<Maybe<Courier>> GetAsync(Guid courierId);
    
    Task<IReadOnlyCollection<Courier>> GetAllFree();
}