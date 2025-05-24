using DeliveryApp.Core.Application.Queries.GetBusyCouriers;
using DeliveryApp.Core.Application.Queries.GetUncompletedOrders;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Migrations;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace DeliveryApp.IntegrationTests.Application.Queries;

public class GetUncomplitedOrdersQueryShould : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17.5")
        .WithDatabase("orders")
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
    public async Task ReturnOnlyUncomplitedOrders()
    {
        // Arrange
        var courier = Courier.Create("Ivan", "car", 3, Location.CreateRandom()).Value;
        
        var firstOrder = Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value;
        firstOrder.Assign(courier.Id);
        firstOrder.Complete();
        
        var secondOrder = Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value;

        var orderRepository = new OrdersRepository(_context);
        await orderRepository.AddAsync(firstOrder);
        await orderRepository.AddAsync(secondOrder);
        var unitOfWork = new UnitOfWork(_context);
        await unitOfWork.SaveChangesAsync();

        // Act
        var queryHandler = new GetUncompletedOrdersQueryHandler(_context.Database.GetConnectionString());
        var getUncomplitedOrdersResponse = await queryHandler.Handle(new GetUncompletedOrdersQuery(), CancellationToken.None);

        // Assert
        var orders = getUncomplitedOrdersResponse.Orders.ToList();
        orders.Should().NotBeEmpty();
        orders.Count.Should().Be(1);
        orders.First().Id.Should().Be(secondOrder.Id);
    }
}