using DeliveryApp.Core.Application.Queries.GetBusyCouriers;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Migrations;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace DeliveryApp.IntegrationTests.Application.Queries;

public class GetBusyCouriersQueryShould : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17.5")
        .WithDatabase("couriers")
        .WithUsername("username")
        .WithPassword("secret")
        .WithCleanUp(true)
        .Build();

    private ApplicationDbContext _context;
    
    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(
                _postgreSqlContainer.GetConnectionString(),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(Init).Assembly.GetName().Name); 
                })
            .Options;
        
        _context = new ApplicationDbContext(contextOptions);
        await _context.Database.MigrateAsync();
    }
    
    public Task DisposeAsync()
    {
        return _postgreSqlContainer.DisposeAsync().AsTask();
    }
    
    [Fact]
    public async Task ReturnOnlyBusyCouriers()
    {
        // Arrange
        var firstCourier = Courier.Create("Ivan", "car", 3, Location.CreateRandom()).Value;
        firstCourier.SetBusy();

        var secondCourier = Courier.Create("Petr", "bicycle", 2, Location.CreateRandom()).Value;

        var courierRepository = new CouriersRepository(_context);
        var unitOfWork = new UnitOfWork(_context);
        await courierRepository.AddAsync(firstCourier);
        await courierRepository.AddAsync(secondCourier);
        await unitOfWork.SaveChangesAsync();

        // Act
        var queryHandler = new GetBusyCouriersQueryHandler(_context.Database.GetConnectionString());
        var getBusyCouriersResponse = await queryHandler.Handle(new GetBusyCouriersQuery(), CancellationToken.None);

        // Assert
        var couriersResponse = getBusyCouriersResponse.Couriers.ToList();
        couriersResponse.Should().NotBeEmpty();
        couriersResponse.Count.Should().Be(1);
        couriersResponse.First().Id.Should().Be(firstCourier.Id);
    }
}