namespace DeliveryApp.Core.Application.Queries.GetBusyCouriers;

public class GetBusyCouriersResponse
{
    public GetBusyCouriersResponse(List<Courier> couriers)
    {
        Couriers.AddRange(couriers);
    }

    public List<Courier> Couriers { get; set; } = new();
    
    public class Courier
    {
        public Guid Id { get; set; }
    
        public string Name { get; set; }
    
        public Location Location { get; set; }
    }

    public class Location
    {
        public int X { get; set; }
    
        public int Y { get; set; }
    }
}