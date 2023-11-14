using Metric.API.OpenTelemetry;
using Microsoft.AspNetCore.Mvc;

namespace Metric.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class MetricsController : ControllerBase
{
    [HttpGet]
    public IActionResult CounterMetric()
    {
        OpenTelemetryMetric.OrderCreatedEventCounter.Add(1,
            new KeyValuePair<string, object?>("event", "add"),
            new KeyValuePair<string, object?>("queue.name", "event.created.queue"));

        return Ok();
    }
    [HttpGet]
    public IActionResult CounterObservableMetric()
    {
        Counter.OrderCancelledCounter += new Random().Next(1, 100);
        return Ok();
    }
}
