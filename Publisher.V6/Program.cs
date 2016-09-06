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
    const int TotalMessages = 1000;

    static void Main(string[] args)
    {
        var config = new EndpointConfiguration("Publisher.V6");
        config.UseTransport<MsmqTransport>();
        config.UsePersistence<NHibernatePersistence>()
            .ConnectionString(@"Data Source=.\SQLEXPRESS;Integrated Security=True;Database=PTest");
        config.SendFailedMessagesTo("error");

        Run(config).GetAwaiter().GetResult();
    }

    static async Task Run(EndpointConfiguration config)
    {
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
        var seconds = (double)swatch.ElapsedMilliseconds / 1000;
        var perSecond = TotalMessages / seconds;
        Console.WriteLine($"Elapsed: {swatch.ElapsedMilliseconds}. {perSecond} msg/s");
        Console.ReadLine();

        await endpoint.Stop();
    }
}

class MyEvent : IEvent
{
}
