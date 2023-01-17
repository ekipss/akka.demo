using Akka.Actor;
using Akka.Event;
using Akka.IO;
using Demo.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var system = ActorSystem.Create("AkkaDemo");
            var manager = system.Tcp();

            try
            {
                system.ActorOf(Props.Create(() => new TelnetClient("127.0.0.1", 9000)), "client");

                // Setup an actor that will handle deadletter type messages
                var deadletterWatchMonitorProps = Props.Create(() => new DeadletterMonitor());
                var deadletterWatchActorRef = system.ActorOf(deadletterWatchMonitorProps, "DeadLetterMonitoringActor");

                // subscribe to the event stream for messages of type "DeadLetter"
                system.EventStream.Subscribe(deadletterWatchActorRef, typeof(DeadLetter));

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            while (true) { }
        }
    }
}

   
