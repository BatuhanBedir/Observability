using Common.Shared.DTOs;
using Common.Shared.Events;
using MassTransit;
using OpenTelemetry.Shared;
using Order.API.Models;
using Order.API.RedisServices;
using Order.API.StockServices;
using System.Diagnostics;
using System.Net;

namespace Order.API.OrderServices;

public class OrderService
{
    private readonly AppDbContext _context;
    private readonly StockService _stockService;
    private readonly RedisService _redisService;
    public OrderService(AppDbContext context, StockService stockService, RedisService redisService)
    {
        _context = context;
        _stockService = stockService;
        _redisService = redisService;
    }

    public async Task<ResponseDto<OrderCreateResponseDto>> CreateAsync(OrderCreateRequestDto request)
    {
        using (var redisActivity = ActivitySourceProvider.Source.StartActivity("RedisStringSetGet"))
        {
            await _redisService.GetDb(0).StringSetAsync("userId", request.UserId);
            redisActivity.SetTag("userId", request.UserId);
            var redisUserId = await _redisService.GetDb(0).StringGetAsync("userId");
        }

        //await _redisService.GetDb(0).StringSetAsync("userId", request.UserId);


        Activity.Current?.SetTag("aspnetcore(instrumentation) tag1", "aspnetcore(instrumentation) value"); //current activity. 

        using var activity = ActivitySourceProvider.Source.StartActivity();//child activity

        activity?.AddEvent(new("order process has started"));

        activity.SetBaggage("userId", request.UserId.ToString());

        var newOrder = new Order()
        {
            Created = DateTime.Now,
            OrderCode = Guid.NewGuid().ToString(),
            Status = OrderStatus.Success,
            UserId = request.UserId,
            Items = request.Items.Select(x => new OrderItem()
            {
                Count = x.Count,
                UnitPrice = x.UnitPrice,
                ProductId = x.ProductId
            }).ToList()
        };

        _context.Orders.Add(newOrder);
        await _context.SaveChangesAsync();

        StockCheckAndPaymentProcessRequestDto stockRequest = new();
        stockRequest.OrderCode = newOrder.OrderCode;
        stockRequest.OrderItems = request.Items;

        var (isSuccess, failMessage) = await _stockService.CheckStockAndPaymentStartAsync(stockRequest);
        if (!isSuccess)
        {
            return ResponseDto<OrderCreateResponseDto>.Fail(HttpStatusCode.InternalServerError.GetHashCode(), failMessage!);
        };

        activity?.SetTag("order user id", request.UserId);
        activity?.AddEvent(new("order process has completed"));


        return ResponseDto<OrderCreateResponseDto>.Success(HttpStatusCode.OK.GetHashCode(), new OrderCreateResponseDto() { Id = newOrder.Id });
    }
}
