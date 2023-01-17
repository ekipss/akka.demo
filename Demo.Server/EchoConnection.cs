using Akka.Actor;
using Akka.IO;
using System.Text;

namespace Demo.Server
{
    public class EchoConnection : UntypedActor
    {
        private readonly IActorRef _connection;

        public EchoConnection(IActorRef connection)
        {
            _connection = connection;
        }

        protected override void OnReceive(object message)
        {
            if (message is Tcp.Received)
            {
                var received = message as Tcp.Received;
                if (received.Data[0] == 'x')
                    Context.Stop(Self);
                else
                {
                    var result = Encoding.Default.GetString(received.Data.ToArray());
                    Console.WriteLine(result + " got successfully");
                    _connection.Tell(Tcp.Write.Create(received.Data));
                }

            }
            else Unhandled(message);
        }
    }
}
