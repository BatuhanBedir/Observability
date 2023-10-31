﻿using Common.Shared.DTOs;
using OpenTelemetry.Shared;
using Order.API.Models;
using Order.API.StockServices;
using System.Diagnostics;
using System.Net;

namespace Order.API.OrderServices;

public class OrderService
{
    private readonly AppDbContext _context;
    private readonly StockService _stockService;

    public OrderService(AppDbContext context, StockService stockService)
    {
        _context = context;
        _stockService = stockService;
    }

    public async Task<ResponseDto<OrderCreateResponseDto>> CreateAsync(OrderCreateRequestDto request)
    {
        Activity.Current?.SetTag("aspnetcore(instrumentation) tag1", "aspnetcore(instrumentation) value"); //current activity. 

        using var activity = ActivitySourceProvider.Source.StartActivity();//child activity

        activity?.AddEvent(new("order process has started"));

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
