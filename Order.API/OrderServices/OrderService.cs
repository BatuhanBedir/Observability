using OpenTelemetry.Shared;
using Order.API.Models;
using System.Diagnostics;

namespace Order.API.OrderServices;

public class OrderService
{
    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OrderCreateResponseDto> CreateAsync(OrderCreateRequestDto requestDto)
    {
        Activity.Current?.SetTag("aspnetcore(instrumentation) tag1", "aspnetcore(instrumentation) value"); //current activity. 

        using var activity = ActivitySourceProvider.Source.StartActivity();//child activity

        activity?.AddEvent(new("order process has started"));

        var newOrder = new Order()
        {
            Created = DateTime.Now,
            OrderCode = Guid.NewGuid().ToString(),
            Status = OrderStatus.Success,
            UserId = requestDto.UserId,
            Items = requestDto.Items.Select(x => new OrderItem()
            {
                Count = x.Count,
                UnitPrice = x.UnitPrice,
                ProductId = x.ProductId
            }).ToList()
        };

        _context.Orders.Add(newOrder);
        await _context.SaveChangesAsync();

        activity?.AddEvent(new("order process has completed"));


        return new OrderCreateResponseDto() { Id = newOrder.Id };
    }
}
