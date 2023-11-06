using System.Collections.Specialized;

namespace Common.Shared.Events;

public record OrderCreatedEvent
{
    //Activity.Current.TraceId;
    //public Dictionary<string, string> Headers{ get; set; } manuel traceId-header->manuel
    public string OrderCode { get; set; } = null!;
}

//MassTransit - Native OpenTelemetry Support