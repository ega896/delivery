using CSharpFunctionalExtensions;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.Commands.CreateOrder;

public class CreateOrderCommand(Guid basketId, string street) : IRequest<UnitResult<Error>>
{
    /// <summary>
    /// Идентификатор корзины
    /// </summary>
    /// <remarks>Id корзины берется за основу при создании Id заказа, они совпадают</remarks>
    public Guid BasketId { get; } = basketId;

    public string Street { get; } = street;
}