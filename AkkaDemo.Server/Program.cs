using Akka.Actor;
using Akka.Configuration;
using static AkkaDemo.Contracts.Messages;

class Program
{
    static void Main(string[] args)
    {
        var config = ConfigurationFactory.ParseString(@"
akka {  
    actor {
        provider = remote
    }
    remote {
        dot-netty.tcp {
            port = 8081
            hostname = 0.0.0.0
            public-hostname = localhost
        }
    }
}
");

        using (var system = ActorSystem.Create("MyServer", config))
        {
            system.ActorOf(Props.Create(() => new ServerActor()), "Server");

            Console.ReadLine();
        }
    }
}

class ServerActor : ReceiveActor, ILogReceive
{
    private readonly HashSet<IActorRef> _clients = new HashSet<IActorRef>();

    public ServerActor()
    {
        //ReceiveAny(message =>
        //{
        //    var x = message;
        //});

        //Become(message =>
        //{
        //    var x = message;
        //});
        Receive<SayRequest>(message =>
        {
            var response = new SayResponse
            {
                Username = message.Username,
                Text = message.Text,
            };
            foreach (var client in _clients) client.Tell(response, Self);
        });

        Receive<ConnectRequest>(message =>
        {
            _clients.Add(Sender);
            Sender.Tell(new ConnectResponse
            {
                Message = "Hello and welcome to Akka.NET chat example",
            }, Self);
        });

        Receive<NickRequest>(message =>
        {
            var response = new NickResponse
            {
                OldUsername = message.OldUsername,
                NewUsername = message.NewUsername,
            };

            foreach (var client in _clients) client.Tell(response, Self);
        });
    }
}