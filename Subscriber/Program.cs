using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;

class Program
{
    static void Main(string[] args)
    {
        var busConfig = new BusConfiguration();
        busConfig.UseTransport<MsmqTransport>();
        busConfig.UsePersistence<InMemoryPersistence>();
        busConfig.PurgeOnStartup(true);
        var bus = Bus.Create(busConfig).Start();
        bus.Subscribe(typeof(MyEvent));
        Console.ReadLine();
    }
}

class EventHandler : IHandleMessages<MyEvent>
{
    public void Handle(MyEvent message)
    {
    }
}

class MyEvent : IEvent
{
}
