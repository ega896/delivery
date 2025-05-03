﻿using CSharpFunctionalExtensions;

using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;

using Primitives;

namespace DeliveryApp.Core.Domain.Services.Dispatch;

public interface IDispatchService
{
    Result<Courier, Error> Dispatch(Order order, IReadOnlyCollection<Courier> couriers);
}