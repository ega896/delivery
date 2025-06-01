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

namespace DeliveryApp.IntegrationTests.Repositories;

public class OrdersRepositoryShould : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17.5")
        .WithDatabase("order")
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
    public async Task CanAddOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(orderId, Location.CreateRandom()).Value;

        // Act
        var orderRepository = new OrdersRepository(_context);
        var unitOfWork = new UnitOfWork(_context);

        await orderRepository.AddAsync(order);
        await unitOfWork.SaveChangesAsync();

        // Assert
        var getOrderFromDbResult = await orderRepository.GetAsync(order.Id);
        getOrderFromDbResult.HasValue.Should().BeTrue();
        var orderFromDb = getOrderFromDbResult.Value;
        orderFromDb.Should().BeEquivalentTo(order);
    }

    [Fact]
    public async Task CanUpdateOrder()
    {
        // Arrange
        var courier = Courier.Create("Ivan", "car", 3, Location.CreateRandom()).Value;

        var orderId = Guid.NewGuid();
        var order = Order.Create(orderId, Location.CreateRandom()).Value;

        var orderRepository = new OrdersRepository(_context);
        await orderRepository.AddAsync(order);

        var unitOfWork = new UnitOfWork(_context);
        await unitOfWork.SaveChangesAsync();

        // Act
        order.Assign(courier.Id);
        orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync();

        // Assert
        var getOrderFromDbResult = await orderRepository.GetAsync(order.Id);
        getOrderFromDbResult.HasValue.Should().BeTrue();
        var orderFromDb = getOrderFromDbResult.Value;
        orderFromDb.Should().BeEquivalentTo(order);
    }

    [Fact]
    public async Task CanGetOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.Create(orderId, Location.CreateRandom()).Value;

        // Act
        var orderRepository = new OrdersRepository(_context);
        await orderRepository.AddAsync(order);

        var unitOfWork = new UnitOfWork(_context);
        await unitOfWork.SaveChangesAsync();

        // Assert
        var getOrderFromDbResult = await orderRepository.GetAsync(order.Id);
        getOrderFromDbResult.HasValue.Should().BeTrue();
        var orderFromDb = getOrderFromDbResult.Value;
        orderFromDb.Should().BeEquivalentTo(order);
    }

    [Fact]
    public async Task CanGetFirstCreatedOrder()
    {
        // Arrange
        var courier = Courier.Create("Ivan", "car", 3, Location.CreateRandom()).Value;
        
        var firstOrder = Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value;
        firstOrder.Assign(courier.Id);
        
        var secondOrder = Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value;

        var orderRepository = new OrdersRepository(_context);
        await orderRepository.AddAsync(firstOrder);
        await orderRepository.AddAsync(secondOrder);

        var unitOfWork = new UnitOfWork(_context);
        await unitOfWork.SaveChangesAsync();

        // Act
        var createdOrderFromDbResult = await orderRepository.GetFirstCreatedAsync();

        // Assert
        createdOrderFromDbResult.HasValue.Should().BeTrue();
        var orderFromDb = createdOrderFromDbResult.Value;
        orderFromDb.Should().BeEquivalentTo(secondOrder);
    }
    
    [Fact]
    public async Task CanGetAllAssignedOrders()
    {
        // Arrange
        var courier = Courier.Create("Ivan", "car", 3, Location.CreateRandom()).Value;
        
        var firstOrder = Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value;
        firstOrder.Assign(courier.Id);
        
        var secondOrder = Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value;

        var orderRepository = new OrdersRepository(_context);
        await orderRepository.AddAsync(firstOrder);
        await orderRepository.AddAsync(secondOrder);

        var unitOfWork = new UnitOfWork(_context);
        await unitOfWork.SaveChangesAsync();

        // Act
        var assignedOrders = await orderRepository.GetAllAssigned();

        // Assert
        assignedOrders.Should().NotBeEmpty();
        assignedOrders.Count.Should().Be(1);
        assignedOrders.First().Should().BeEquivalentTo(firstOrder);
    }
}