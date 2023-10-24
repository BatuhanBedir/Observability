using Observability.ConsoleApp;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

Console.WriteLine("Hello, World!");

ActivitySource.AddActivityListener(new ActivityListener()
{
    ShouldListenTo = source => source.Name == OpenTelemetryConstants.ActivitySourceFileName,
    ActivityStarted = activity =>
    {
        Console.WriteLine("Activity started");
    },
    ActivityStopped = activity =>
    {
        Console.WriteLine("Activity finished");
        activity.Tags.ToList().ForEach(t => Console.WriteLine(t));
    }
    
});


using var traceProviderFile = Sdk.CreateTracerProviderBuilder()
    .AddSource(OpenTelemetryConstants.ActivitySourceFileName)
    .Build();


using var traceProvider = Sdk.CreateTracerProviderBuilder()
   .AddSource(OpenTelemetryConstants.ActivitySourceName)
   .ConfigureResource(configure =>
   {
       configure
       .AddService(serviceName: OpenTelemetryConstants.ServiceName, serviceVersion: OpenTelemetryConstants.ServiceVersion)
       .AddAttributes(new List<KeyValuePair<string, object>>()
               {
                    new KeyValuePair<string, object>("host.machineName", Environment.MachineName),
                    new KeyValuePair<string, object>("host.environment", "dev"),
               });
   })
   .AddConsoleExporter()
   .AddOtlpExporter()
   .AddZipkinExporter(zipkinOptions =>
   {
       zipkinOptions.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
   })
   .Build();

var serviceHelper = new ServiceHelper();
//await serviceHelper.Work1();
await serviceHelper.Work2();