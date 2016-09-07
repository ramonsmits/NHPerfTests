using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Persistence;

class Program
{
    const int TotalMessages = 50000;

    static void Main(string[] args)
    {
        var config = new EndpointConfiguration("Publisher.V5");
        config.UseTransport<MsmqTransport>();
        config.UsePersistence<NHibernatePersistence>()
            .ConnectionString(@"Data Source=.\SQLEXPRESS;Integrated Security=True;Database=PTest");
        config.SendFailedMessagesTo("error");

        Run(config).GetAwaiter().GetResult();
    }

    static async Task Run(EndpointConfiguration config)
    {
        double seconds, perSecond;
        var endpoint = await Endpoint.Start(config);

        for (var i = 0; i < 100; i++)
        {
            await endpoint.Publish(new MyEvent());
        }

        var swatch = Stopwatch.StartNew();

        for (var i = 0; i < TotalMessages; i++)
        {
            await endpoint.Publish(new MyEvent());
        }

        swatch.Stop();
        seconds = swatch.Elapsed.TotalSeconds;
        perSecond = TotalMessages / seconds;
        Console.WriteLine($"for:         {seconds,6:N}s {perSecond,10:N1} msg/s");

        swatch.Restart();

        Parallel.For(0, TotalMessages, i =>
        {
            endpoint.Publish(new MyEvent()).ConfigureAwait(false).GetAwaiter().GetResult();
        });

        seconds = swatch.Elapsed.TotalSeconds;
        perSecond = TotalMessages / seconds;
        Console.WriteLine($"ParallelFor: {seconds,6:N}s {perSecond,10:N1} msg/s");

        swatch.Restart();
        var tasks = new List<Task>(TotalMessages);

        for (var i = 0; i < TotalMessages; i++)
        {
            tasks.Add(endpoint.Publish(new MyEvent()));
        }

        await Task.WhenAll(tasks);

        seconds = swatch.Elapsed.TotalSeconds;
        perSecond = TotalMessages / seconds;
        Console.WriteLine($"TaskWhenAll: {seconds,6:N}s {perSecond,10:N1} msg/s");


        Console.ReadLine();

        await endpoint.Stop();

    }
}

class MyEvent : IEvent
{
}
