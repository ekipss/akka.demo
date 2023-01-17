using System.IO;
using Akka.Actor;

namespace WinTail
{
    /// <summary>
    /// Actor that validates user input and signals result to others.
    /// </summary>
    public class ValidatorActor : UntypedActor
    {
        private readonly IActorRef _consoleWriterActor;

        public ValidatorActor(IActorRef consoleWriterActor)
        {
            _consoleWriterActor = consoleWriterActor;
        }

        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if (string.IsNullOrEmpty(msg))
            {
                // signal that the user needs to supply an input
                _consoleWriterActor.Tell(new Messages.NullInputError("Input was blank. Please try again.\n"));

                // tell sender to continue doing its thing (whatever that may be, this actor doesn't care)
                Sender.Tell(new Messages.ContinueProcessing());
            }
            else
            {
                if (msg == "123" || msg == "1234")
                {
                    // signal successful input
                    _consoleWriterActor.Tell(new Messages.InputSuccess(string.Format("Starting processing for {0}", msg)));

                    // start coordinator
                    Context.ActorSelection("akka://MyActorSystem/user/tailCoordinatorCoordinatorActor").Tell(new TailCoordinatorCoordinatorActor.StartTailCoordinator(msg, _consoleWriterActor));

                }
                else
                {
                    // signal that input was bad
                    _consoleWriterActor.Tell(new Messages.ValidationError(string.Format("{0} is not 123 or 1234.", msg)));

                    // tell sender to continue doing its thing (whatever that may be, this actor doesn't care)
                    Sender.Tell(new Messages.ContinueProcessing());
                }
            }



        }

        /// <summary>
        /// Checks if file exists at path provided by user.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool IsFileUri(string path)
        {
            return File.Exists(path);
        }
    }
}