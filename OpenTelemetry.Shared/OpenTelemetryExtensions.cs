using MassTransit.Logging;
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
            .AddSource(DiagnosticHeaders.DefaultListenerName)
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
                //serilog üzerinden elasticsearch db'ye hatalar gönderildiği için yoruma alındı
                //opt.RecordException = true; //error details
                //opt.EnrichWithException = (activity, exception) =>
                //{
                //    //activity.SetTag("key1", exception.InnerException);
                //};
            });

            options.AddEntityFrameworkCoreInstrumentation(opt =>
            {
                opt.SetDbStatementForText = true;
                opt.SetDbStatementForStoredProcedure = true;
                //opt.EnrichWithIDbCommand = (activity, dbCommand) =>
                //{
                //    //efcore ile üretilen sql cümleciğini activity yani span olarak her kaydettiğinde burası tetikleniyor
                //};
            });

            //api ' den veya mvc ' den başka external api ' lara yapmış olduğumuz istekleri oto. şekilde trace ediyor
            options.AddHttpClientInstrumentation(opt =>
            {
                opt.FilterHttpRequestMessage = (request) =>
                {

                    return !request.RequestUri.AbsoluteUri.Contains("9200", StringComparison.InvariantCulture);

                };
                opt.EnrichWithHttpRequestMessage = async (activity, request) =>
                {
                    var requestContent = "empty";

                    if (request.Content is not null)
                        requestContent = await request.Content.ReadAsStringAsync();

                    activity.SetTag("http.request.body", requestContent);
                };
                opt.EnrichWithHttpResponseMessage = async (activity, response) =>
                {
                    if (response.Content is not null)
                        activity.SetTag("http.response.body", await response.Content.ReadAsStringAsync());

                };

            });

            options.AddRedisInstrumentation(opt =>
            {
                opt.SetVerboseDatabaseStatements = true; //db ile ilgili statements detaylı kaydet
            });

            options.AddConsoleExporter();

            options.AddOtlpExporter(); //jaeger
        });
    }
}
