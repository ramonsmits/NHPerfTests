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
        var busConfig = new BusConfiguration();
        busConfig.UseTransport<MsmqTransport>();
        busConfig.UsePersistence<NHibernatePersistence>()
            .ConnectionString(@"Data Source=.\SQLEXPRESS;Integrated Security=True;Database=PTest");


        var bus = Bus.Create(busConfig).Start();

        for (var i = 0; i < 100; i++)
        {
            bus.Publish(new MyEvent());
        }

        var swatch = Stopwatch.StartNew();

        for (var i = 0; i < TotalMessages; i++)
        {
            bus.Publish(new MyEvent());
        }

        swatch.Stop();
        var seconds = (double) swatch.ElapsedMilliseconds/1000;
        var perSecond = TotalMessages/seconds;
        Console.WriteLine($"Elapsed: {swatch.ElapsedMilliseconds}. {perSecond} msg/s");
        Console.ReadLine();
    }
}

class MyEvent : IEvent
{
}
