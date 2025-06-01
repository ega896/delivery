using CSharpFunctionalExtensions;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.Commands.CreateOrder;

public class CreateOrderCommand : IRequest<UnitResult<Error>>
{
    /// <summary>
    /// Идентификатор корзины
    /// </summary>
    /// <remarks>Id корзины берется за основу при создании Id заказа, они совпадают</remarks>
    public Guid BasketId { get; }
    
    public string Street { get; }
}