using System;
using System.IO;
using System.Text;
using Akka.Actor;

namespace WinTail
{
    public class TailActor : UntypedActor
    {
        private readonly IActorRef _reporterActor;
        public TailActor(IActorRef reporterActor, string text)
        {
            _reporterActor = reporterActor;
            Self.Tell(new InitialRead(text));
        }

        protected override void OnReceive(object message)
        {
            if (message is InitialRead)
            {
                var ir = message as InitialRead;
                if (ir.Text == "1234")
                    throw new Exception();
                _reporterActor.Tell(ir.Text);
            }
        }
    }

    public class InitialRead
    {
        public InitialRead(string text)
        {
            Text = text;
        }
        public string Text { get; private set; }
    }
}