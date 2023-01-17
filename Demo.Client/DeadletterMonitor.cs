using Akka.Actor;
using Akka.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Client
{
    // A dead letter handling actor specifically for messages of type "DeadLetter"
    public class DeadletterMonitor : ReceiveActor
    {

        public DeadletterMonitor()
        {
            Receive<DeadLetter>(dl => HandleDeadletter(dl));
        }

        private void HandleDeadletter(DeadLetter dl)
        {
            Console.WriteLine($"DeadLetter captured: {dl.Message}, sender: {dl.Sender}, recipient: {dl.Recipient}");
        }
    }

}
