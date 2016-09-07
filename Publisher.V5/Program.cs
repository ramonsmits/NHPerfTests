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
    const int TotalMessages = 5000;

    static void Main(string[] args)
    {
        double seconds, perSecond;

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
        seconds = swatch.Elapsed.TotalSeconds;
        perSecond = TotalMessages / seconds;
        Console.WriteLine($"for:         {seconds,6:N}s {perSecond,10:N1} msg/s");

        swatch.Restart();

        Parallel.For(0, TotalMessages, i =>
        {
            bus.Publish(new MyEvent());
        });

        seconds = swatch.Elapsed.TotalSeconds;
        perSecond = TotalMessages / seconds;
        Console.WriteLine($"ParallelFor: {seconds,6:N}s {perSecond,10:N1} msg/s");

        swatch.Restart();
        var tasks = new List<Task>(TotalMessages);

        for (var i = 0; i < TotalMessages; i++)
        {
            tasks.Add(Task.Run(() => { bus.Publish(new MyEvent()); }));
        }

        Task.WhenAll(tasks).ConfigureAwait(false).GetAwaiter().GetResult();

        seconds = swatch.Elapsed.TotalSeconds;
        perSecond = TotalMessages / seconds;
        Console.WriteLine($"TaskWhenAll: {seconds,6:N}s {perSecond,10:N1} msg/s");

        Console.ReadLine();
    }
}

class MyEvent : IEvent
{
}
