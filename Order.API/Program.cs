using Common.Shared;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Shared;
using Order.API.Models;
using Order.API.OrderServices;
using Order.API.RedisServices;
using Order.API.StockServices;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisService = sp.GetService<RedisService>()!;

    return redisService.GetConnectionMultiplexer;
});

builder.Services.AddSingleton(_ =>
{
    return new RedisService(builder.Configuration);
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetryExt(builder.Configuration);

builder.Services.AddHttpClient<StockService>(options =>
{
    options.BaseAddress = new Uri((builder.Configuration.GetSection("ApiServices")["StockApi"])!);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<RequestAndResponseActivityMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();