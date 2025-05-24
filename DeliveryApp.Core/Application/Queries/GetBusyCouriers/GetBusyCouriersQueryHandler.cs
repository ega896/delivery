using Dapper;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using MediatR;
using Npgsql;

namespace DeliveryApp.Core.Application.Queries.GetBusyCouriers;

public class GetBusyCouriersQueryHandler(string connectionString) : IRequestHandler<GetBusyCouriersQuery, GetBusyCouriersResponse>
{
    public async Task<GetBusyCouriersResponse> Handle(GetBusyCouriersQuery request, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var couriers = (await connection.QueryAsync<GetBusyCouriersResponse.Courier, GetBusyCouriersResponse.Location, GetBusyCouriersResponse.Courier>(
            @"SELECT id, name, location_x as x, location_y as y FROM public.couriers WHERE status = @Status",
            (courier, location) =>
            {
                courier.Location = location;
                return courier;
            },
            new { Status = CourierStatus.Busy.Name },
            splitOn: "x"
        )).ToList();
        
        return new GetBusyCouriersResponse(couriers);
    }
}