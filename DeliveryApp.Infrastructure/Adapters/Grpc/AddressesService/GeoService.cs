using CSharpFunctionalExtensions;
using DeliveryApp.Core.Ports;
using GeoApp.Api;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Options;
using Primitives;

namespace DeliveryApp.Infrastructure.Adapters.Grpc.AddressesService;

public class GeoService(IOptions<Settings> options) : IGeoService
{
    private readonly MethodConfig _methodConfig = new()
    {
        Names = { MethodName.Default },
        RetryPolicy = new RetryPolicy
        {
            MaxAttempts = 5,
            InitialBackoff = TimeSpan.FromSeconds(1),
            MaxBackoff = TimeSpan.FromSeconds(5),
            BackoffMultiplier = 1.5,
            RetryableStatusCodes = { StatusCode.Unavailable }
        }
    };

    private readonly SocketsHttpHandler _socketsHttpHandler = new()
    {
        PooledConnectionLifetime = Timeout.InfiniteTimeSpan,
        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
        EnableMultipleHttp2Connections = true
    };

    public async Task<Result<Core.Domain.SharedKernel.Location, Error>> GetAddress(string street, CancellationToken cancellationToken)
    {
        using var channel = GrpcChannel.ForAddress(options.Value.GeoServiceGrpcHost, new GrpcChannelOptions
        {
            HttpHandler = _socketsHttpHandler,
            ServiceConfig = new ServiceConfig
            {
                MethodConfigs = { _methodConfig }
            }
        });

        var client = new Geo.GeoClient(channel);
        var getGeolocationReply = await client.GetGeolocationAsync(new GetGeolocationRequest
        {
            Street = street
        }, cancellationToken: cancellationToken);

        var locationDto = getGeolocationReply.Location;
        var location = Core.Domain.SharedKernel.Location.Create(locationDto.X, locationDto.Y);
        return location;
    }
}