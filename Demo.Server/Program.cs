using Akka.Actor;

namespace Demo.Server
{
    class Program
    {
        static void Main(string[] args)
        {

            var system = ActorSystem.Create("AkkaDemo");
            system.ActorOf(Props.Create(() => new EchoServer(9000)), "server");


            Console.ReadLine();
        }
    }
}
