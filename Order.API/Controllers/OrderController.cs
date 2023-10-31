using Microsoft.AspNetCore.Mvc;
using Order.API.OrderServices;

namespace Order.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(OrderCreateRequestDto request)
    {
        await _orderService.CreateAsync(request);

        #region Exception exp.
        //var a = 10;
        //var b = 0;
        //var c = a / b; 
        #endregion
        return Ok(new OrderCreateResponseDto() { Id = new Random().Next(1, 500) });
    }
}
