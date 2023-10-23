using System.Diagnostics;

namespace Observability.ConsoleApp;

internal class ServiceOne
{
    static HttpClient httpClient = new HttpClient();
    internal async Task<int> MakeRequestToGoogle()
    {
        using var activity = ActivitySourceProvider.Source.StartActivity(kind: System.Diagnostics.ActivityKind.Producer, name: "CustomMakeRequestToGoogle");

        try
        {
            var eventTags = new ActivityTagsCollection();

            activity?.AddEvent(new("request to google started", tags: eventTags));

            var result = await httpClient.GetAsync("https://www.google.com");
            //var result = await httpClient.GetAsync("httpss://www.google.com");

            var responseContent = await result.Content.ReadAsStringAsync();

            eventTags.Add("google body length", responseContent.Length);

            activity?.AddEvent(new("request to google completed", tags: eventTags));

            return responseContent.Length;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return -1;
        }


    }
}
