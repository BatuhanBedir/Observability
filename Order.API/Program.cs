using Common.Shared;
using OpenTelemetry.Shared;
using Order.API.OrderServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<OrderService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetryExt(builder.Configuration);

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
