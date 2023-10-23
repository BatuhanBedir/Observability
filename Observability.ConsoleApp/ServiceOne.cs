﻿namespace Observability.ConsoleApp;

internal class ServiceOne
{
    static HttpClient httpClient = new HttpClient();
    internal async Task<int> MakeRequestToGoogle()
    {
        using var activity = ActivitySourceProvider.Source.StartActivity();

        var result = await httpClient.GetAsync("https://www.google.com");

        var responseContent = await result.Content.ReadAsStringAsync();

        return responseContent.Length;
    }
}