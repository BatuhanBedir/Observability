using OpenTelemetry.Shared;
using System.Diagnostics;

namespace Order.API.OrderServices;

public class OrderService
{
    public Task CreateAsync(OrderCreateRequestDto requestDto)
    {
        Activity.Current?.SetTag("aspnetcore(instrumentation) tag1", "aspnetcore(instrumentation) value"); //current activity. 

        using var activity = ActivitySourceProvider.Source.StartActivity();  //child activity

        activity?.AddEvent(new("order process has started"));

        activity?.SetTag("order user id", requestDto.UserId);
        
        activity?.AddEvent(new("order process has completed"));


        return Task.CompletedTask;
    }
}
