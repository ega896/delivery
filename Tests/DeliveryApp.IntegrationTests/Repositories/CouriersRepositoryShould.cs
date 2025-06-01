using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Migrations;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace DeliveryApp.IntegrationTests.Repositories;

public class CouriersRepositoryShould : IAsyncLifetime
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
    public async Task CanAddCourier()
    {
        // Arrange
        var courier = Courier.Create("Ivan", "car", 3, Location.CreateRandom()).Value;

        // Act
        var courierRepository = new CouriersRepository(_context);
        var unitOfWork = new UnitOfWork(_context);
        await courierRepository.AddAsync(courier);
        await unitOfWork.SaveChangesAsync();

        // Assert
        var getCourierFromDbResult = await courierRepository.GetAsync(courier.Id);
        getCourierFromDbResult.HasValue.Should().BeTrue();
        var courierFromDb = getCourierFromDbResult.Value;
        courierFromDb.Should().BeEquivalentTo(courier);
    }

    [Fact]
    public async Task CanUpdateCourier()
    {
        // Arrange
        var courier = Courier.Create("Ivan", "car", 3, Location.CreateRandom()).Value;
        var courierRepository = new CouriersRepository(_context);
        var unitOfWork = new UnitOfWork(_context);
        await courierRepository.AddAsync(courier);
        await unitOfWork.SaveChangesAsync();

        // Act
        courier.SetBusy();
        courierRepository.Update(courier);
        await unitOfWork.SaveChangesAsync();

        // Assert
        var getCourierFromDbResult = await courierRepository.GetAsync(courier.Id);
        getCourierFromDbResult.HasValue.Should().BeTrue();
        var courierFromDb = getCourierFromDbResult.Value;
        courierFromDb.Should().BeEquivalentTo(courier);
        courierFromDb.Status.Should().Be(CourierStatus.Busy);
    }

    [Fact]
    public async Task CanGetCourier()
    {
        // Arrange
        var courier = Courier.Create("Ivan", "car", 3, Location.CreateRandom()).Value;

        // Act
        var courierRepository = new CouriersRepository(_context);
        var unitOfWork = new UnitOfWork(_context);
        await courierRepository.AddAsync(courier);
        await unitOfWork.SaveChangesAsync();

        // Assert
        var getCourierFromDbResult = await courierRepository.GetAsync(courier.Id);
        getCourierFromDbResult.HasValue.Should().BeTrue();
        var courierFromDb = getCourierFromDbResult.Value;
        courierFromDb.Should().BeEquivalentTo(courier);
    }

    [Fact]
    public async Task CanGetAllFreeCouriers()
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
        var activeCouriers = await courierRepository.GetAllFree();

        // Assert
        activeCouriers.Should().NotBeEmpty();
        activeCouriers.Count.Should().Be(1);
        activeCouriers.First().Should().BeEquivalentTo(secondCourier);
    }
}