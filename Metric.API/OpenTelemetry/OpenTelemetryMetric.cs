using System.Diagnostics.Metrics;

namespace Metric.API.OpenTelemetry;

public static class OpenTelemetryMetric
{
    private static readonly Meter meter = new("metric.meter.api");

    //create counter
    public static Counter<int> OrderCreatedEventCounter = meter.CreateCounter<int>("order.created.event.count");

    public static ObservableCounter<int> OrderCancelledCounter = meter.CreateObservableCounter("order.cancelled.count", () => new Measurement<int>(Counter.OrderCancelledCounter));
    //prometheus: order.cancelled.count
}
