using Common.Shared;
using OpenTelemetry.Shared;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddOpenTelemetryExt(builder.Configuration);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<PaymentService>(options =>
{
    options.BaseAddress = new Uri((builder.Configuration.GetSection("ApiServices")["PaymentApi"])!);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestAndResponseActivityMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();