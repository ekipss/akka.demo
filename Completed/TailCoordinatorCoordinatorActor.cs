using System;
using System.Collections.Generic;
using Akka.Actor;
using Akka.Actor.Internal;
using static WinTail.TailCoordinatorActor;

namespace WinTail
{
    public class TailCoordinatorCoordinatorActor : UntypedActor
    {
        #region Message types
        /// <summary>
        /// Start tailing the file at user-specified path.
        /// </summary>
        public class StartTailCoordinator
        {
            public StartTailCoordinator(string filePath, IActorRef reporterActor)
            {
                FilePath = filePath;
                ReporterActor = reporterActor;
            }

            public string FilePath { get; private set; }

            public IActorRef ReporterActor { get; private set; }
        }

        /// <summary>
        /// Stop tailing the file at user-specified path.
        /// </summary>
        public class StopTailCoordinator
        {
            public StopTailCoordinator(string filePath)
            {
                FilePath = filePath;
            }

            public string FilePath { get; private set; }
        }

        #endregion
        protected override void OnReceive(object message)
        {
            if (message is StartTailCoordinator)
            {
                var msg = message as StartTailCoordinator;
                Context.ActorOf(Props.Create(() => new TailCoordinatorActor(message)));
            }

        }

        // here we are overriding the default SupervisorStrategy
        // which is a One-For-One strategy w/ a Restart directive
        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                5, // maxNumberOfRetries
                TimeSpan.FromSeconds(30), // duration
                x =>
                {
                    //Maybe we consider ArithmeticException to not be application critical
                    //so we just ignore the error and keep going.
                    if (x is ArithmeticException) return Directive.Resume;

                    //Error that we cannot recover from, stop the failing actor
                    else if (x is NotSupportedException) return Directive.Stop;

                    //In all other cases, just restart the failing actor
                    else return Directive.Restart;
                });
        }
    }
}


