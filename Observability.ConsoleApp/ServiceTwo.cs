using System.Diagnostics;

namespace Observability.ConsoleApp;

internal class ServiceTwo
{
    internal async Task<int> WriteToFile(string text)
    {
        //Activity.Current->o anki güncel activity
        Activity.Current?.SetTag("current activity", "1"); //work1'e yerleşti
        //Activity.Current.SetTag("current activity", "1"); //"?" nullrefexp //exp fırlatılırsa activity durur

        //using (var activity = ActivitySourceProvider.Source.StartActivity("a"))
        //{
        //    await File.WriteAllTextAsync("myFile.txt", text);

        //    var length = (await File.ReadAllTextAsync("myFile.txt")).Length;
        //}

        using var activity = ActivitySourceProvider.Source.StartActivity();

        await File.WriteAllTextAsync("myFile.txt", text);

        return (await File.ReadAllTextAsync("myFile.txt")).Length;

    }
}
