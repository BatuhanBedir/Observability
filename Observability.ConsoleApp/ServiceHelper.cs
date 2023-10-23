﻿namespace Observability.ConsoleApp;

internal class ServiceHelper
{
    internal async Task Work1()
    {
        using var activity = ActivitySourceProvider.Source.StartActivity();

        var serviceOne = new ServiceOne();

        Console.WriteLine($"google response length: {await serviceOne.MakeRequestToGoogle()}");
        Console.WriteLine("Work1 completed");

        var serviceTwo = new ServiceTwo();
        var fileLength = await serviceTwo.WriteToFile("Hello world");
    }
}
