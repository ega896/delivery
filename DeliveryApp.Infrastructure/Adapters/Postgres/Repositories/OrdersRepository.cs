using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;

public class OrdersRepository(ApplicationDbContext dbContext) : IOrdersRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task AddAsync(Order order)
    {
        await _dbContext.Orders.AddAsync(order);
    }

    public void Update(Order order)
    {
        _dbContext.Orders.Update(order);
    }

    public async Task<Maybe<Order>> GetAsync(Guid orderId)
    {
        var order = await _dbContext.Orders
            .SingleOrDefaultAsync(o => o.Id == orderId);
        
        return order;
    }

    public async Task<Maybe<Order>> GetFirstCreatedAsync()
    {
        var order = await _dbContext.Orders
            .FirstOrDefaultAsync(o => o.Status.Name == OrderStatus.Created.Name);
        
        return order;
    }

    public async Task<IReadOnlyCollection<Order>> GetAllAssigned()
    {
        var orders = await _dbContext.Orders
            .Where(o => o.Status.Name == OrderStatus.Assigned.Name)
            .ToListAsync();
        
        return orders;
    }
}