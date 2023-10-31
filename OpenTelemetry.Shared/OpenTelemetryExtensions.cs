using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Shared;

public static class OpenTelemetryExtensions
{
    public static void AddOpenTelemetryExt(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenTelemetryConstants>(configuration.GetSection("OpenTelemetry"));

        var openTelemetryConstants = (configuration.GetSection("OpenTelemetry").Get<OpenTelemetryConstants>())!;
        ActivitySourceProvider.Source = new System.Diagnostics.ActivitySource(openTelemetryConstants.ActivitySourceName);

        services.AddOpenTelemetry().WithTracing(options =>
        {
            options.AddSource(openTelemetryConstants.ActivitySourceName)
            .ConfigureResource(resource =>
            {
                resource.AddService(serviceName: openTelemetryConstants.ServiceName, serviceVersion: openTelemetryConstants.ServiceVersion);
            });
            options.AddAspNetCoreInstrumentation(opt =>
            {
                opt.Filter = (context) =>
                {
                    if (!string.IsNullOrEmpty(context.Request.Path.Value))
                    {
                        return context.Request.Path.Value!.Contains("api", StringComparison.InvariantCulture); //url'de api yoksa trace etme | API,api,Api 
                    }
                    return false;
                };
                opt.RecordException = true; //error details
            });
            options.AddConsoleExporter();
            options.AddOtlpExporter(); //jaeger
        });
    }
}
