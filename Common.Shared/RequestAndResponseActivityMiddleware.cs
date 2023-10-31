using Microsoft.AspNetCore.Http;
using Microsoft.IO;
using System.Diagnostics;

namespace Common.Shared;

public class RequestAndResponseActivityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
    public RequestAndResponseActivityMiddleware(RequestDelegate next)
    {
        _next = next;
        _recyclableMemoryStreamManager = new();
    }
    public async Task InvokeAsync(HttpContext context)
    {
        await AddRequestBodyContentToActivityTag(context);
        await AddResponseBodyContentToActivityTag(context);
    }
    private async Task AddRequestBodyContentToActivityTag(HttpContext context)
    {
        context.Request.EnableBuffering();
        var requestBodyStreamReader = new StreamReader(context.Request.Body);
        var requestBodyContent = await requestBodyStreamReader.ReadToEndAsync();
        Activity.Current?.SetTag("http.request.body", requestBodyContent);
        context.Request.Body.Position = 0;
    }
    private async Task AddResponseBodyContentToActivityTag(HttpContext context)
    {
        //request
        var originalResponse = context.Response.Body;

        await using var responseBodyMemoryStream = _recyclableMemoryStreamManager.GetStream();
        context.Response.Body = responseBodyMemoryStream; //memoryde 3ü de aynı yeri işaret ediyor

        await _next(context);

        //response
        responseBodyMemoryStream.Position = 0;
        var responseBodyStreamReader = new StreamReader(responseBodyMemoryStream);
        var responseBodyContent = await responseBodyStreamReader.ReadToEndAsync();

        Activity.Current?.SetTag("http.response.body", responseBodyContent);

        responseBodyMemoryStream.Position = 0;
        await responseBodyMemoryStream.CopyToAsync(originalResponse);


        //dolaylı yoldan. önce body'i memorystream'a alıyoruz, memorystream'de okumayı bitiriyoruz, arkasından originalResponse'ı ilk baştaki body'nin işaret ettiği yere tekrar kopyalıyoruz.
    }
}
