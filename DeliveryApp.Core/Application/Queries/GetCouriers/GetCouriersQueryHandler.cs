using Dapper;
using MediatR;
using Npgsql;

namespace DeliveryApp.Core.Application.Queries.GetCouriers;

public class GetCouriersQueryHandler(string connectionString) : IRequestHandler<GetCouriersQuery, GetCouriersResponse>
{
    public async Task<GetCouriersResponse> Handle(GetCouriersQuery request, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var couriers = (await connection.QueryAsync<GetCouriersResponse.Courier, GetCouriersResponse.Location, GetCouriersResponse.Courier>(
            @"SELECT id, name, location_x as x, location_y as y FROM public.couriers",
            (courier, location) =>
            {
                courier.Location = location;
                return courier;
            },
            new { },
            splitOn: "x"
        )).ToList();
        
        return new GetCouriersResponse(couriers);
    }
}