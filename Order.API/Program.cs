using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Order.API.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<OpenTelemetryConstants>(builder.Configuration.GetSection("OpenTelemetry"));

var openTelemetryConstants = (builder.Configuration.GetSection("OpenTelemetry").Get<OpenTelemetryConstants>())!;

builder.Services.AddOpenTelemetry().WithTracing(options =>
{

    options.AddSource(openTelemetryConstants.ActivitySourceName)
    .ConfigureResource(resource =>
    {
        resource.AddService(serviceName: openTelemetryConstants.ServiceName, serviceVersion: openTelemetryConstants.ServiceVersion);
    });
    options.AddAspNetCoreInstrumentation();
    options.AddConsoleExporter();
    options.AddOtlpExporter(); //jaeger
});

ActivitySourceProvider.Source = new System.Diagnostics.ActivitySource(openTelemetryConstants.ServiceName);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
