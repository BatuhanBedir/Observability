using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Elasticsearch;
using OpenTelemetry.Logs;

namespace Logging.Shared;

public static class Logging
{
    public static void AddOpenTelemetryLog(this WebApplicationBuilder builder)
    {

        
    }



    public static Action<HostBuilderContext, LoggerConfiguration> ConfigureLogging => (builderContext, loggerConfiguration) =>
    {
        var environment = builderContext.HostingEnvironment;

        loggerConfiguration
        .ReadFrom.Configuration(builderContext.Configuration) //appsettings.json
        .Enrich.FromLogContext()   //traceId merkezi yerden. her log oluş o log un içerisine traceId ' yi eklememek için
        .Enrich.WithExceptionDetails() //exp detayları
        .Enrich.WithProperty("Env", environment.EnvironmentName) //her log atarken prop ekle. hangi ortamdan atıldı*
        .Enrich.WithProperty("AppName", environment.ApplicationName);

        var elasticSearchBaseUrl = builderContext.Configuration.GetSection("Elasticsearch")["BaseUrl"];
        var userName = builderContext.Configuration.GetSection("Elasticsearch")["UserName"];
        var password = builderContext.Configuration.GetSection("Elasticsearch")["Password"];
        var indexName = builderContext.Configuration.GetSection("Elasticsearch")["IndexName"];

        loggerConfiguration.WriteTo.Elasticsearch(new(new Uri(elasticSearchBaseUrl))
        {
            AutoRegisterTemplate = true, //temp. oto kaydet
            AutoRegisterTemplateVersion = Serilog.Sinks.Elasticsearch.AutoRegisterTemplateVersion.ESv8,
            IndexFormat = $"{indexName}-{environment.EnvironmentName}-logs-" + "{0:yyy.MM.dd}",//her gün atılan log lar için bucket oluşacak. format belirleniyor.
            ModifyConnectionSettings = x => x.BasicAuthentication(userName, password),
            CustomFormatter = new ElasticsearchJsonFormatter()
        });
    };
}
