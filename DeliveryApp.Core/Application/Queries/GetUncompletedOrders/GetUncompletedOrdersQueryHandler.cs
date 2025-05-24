using Dapper;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using MediatR;
using Npgsql;

namespace DeliveryApp.Core.Application.Queries.GetUncompletedOrders;

public class GetUncompletedOrdersQueryHandler(string connectionString) : IRequestHandler<GetUncompletedOrdersQuery, GetUncomplitedOrdersResponse>
{
    public async Task<GetUncomplitedOrdersResponse> Handle(GetUncompletedOrdersQuery request, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var orders = (await connection.QueryAsync<GetUncomplitedOrdersResponse.Order, GetUncomplitedOrdersResponse.Location, GetUncomplitedOrdersResponse.Order>(
            @"SELECT id, location_x as x, location_y as y FROM public.orders WHERE status != @Status",
            (courier, location) =>
            {
                courier.Location = location;
                return courier;
            },
            new { Status = OrderStatus.Completed.Name },
            splitOn: "x"
        )).ToList();
        
        return new GetUncomplitedOrdersResponse(orders);
    }
}