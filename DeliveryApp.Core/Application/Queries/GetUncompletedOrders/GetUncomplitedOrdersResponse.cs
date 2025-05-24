namespace DeliveryApp.Core.Application.Queries.GetUncompletedOrders;

public class GetUncomplitedOrdersResponse
{
    public GetUncomplitedOrdersResponse(List<Order> orders)
    {
        Orders.AddRange(orders);
    }

    public List<Order> Orders { get; set; } = new();
    
    public class Order
    {
        public Guid Id { get; set; }
    
        public Location Location { get; set; }
    }

    public class Location
    {
        public int X { get; set; }
    
        public int Y { get; set; }
    }
}