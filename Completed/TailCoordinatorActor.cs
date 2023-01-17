using System;
using System.Collections.Generic;
using System.Threading;
using Akka.Actor;
using Akka.Actor.Internal;
using Akka.Util;
using WinTail.SupervisorStrategies;
using static WinTail.TailCoordinatorCoordinatorActor;

namespace WinTail
{

    public class TailCoordinatorActor : UntypedActor
    {
        public TailCoordinatorActor()
        {

            // message we'll receive anyone we DeathWatch
            // dies, OR if the network terminates

        }


        public TailCoordinatorActor(object message)
        {
            if (message is StartTailCoordinator)
            {
                var msg = message as StartTailCoordinator;
                Self.Tell(new StartTail(msg.FilePath, msg.ReporterActor));
            }
        }
        #region Message types
        /// <summary>
        /// Start tailing the file at user-specified path.
        /// </summary>
        public class StartTail
        {
            public StartTail(string filePath, IActorRef reporterActor)
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
        public class StopTail
        {
            public StopTail(string filePath)
            {
                FilePath = filePath;
            }

            public string FilePath { get; private set; }
        }

        #endregion

        protected override void OnReceive(object message)
        {
            if (message is StartTail)
            {
                var msg = message as StartTail;
                Context.WatchWith(msg.ReporterActor, msg.FilePath);
                Context.ActorOf(Props.Create(() => new TailActor(msg.ReporterActor, msg.FilePath)));

            }

        }

        // here we are overriding the default SupervisorStrategy
        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new CustomStrategy(
            1, // maxNumberOfRetries
            TimeSpan.FromSeconds(30), // duration
            x =>
            {
                //Maybe we consider ArithmeticException to not be application critical
                //so we just ignore the error and keep going.
                if (x is ArithmeticException) return Directive.Resume;

                //Error that we cannot recover from, stop the failing actor
                else if (x is NotSupportedException) return Directive.Stop;

                //In all other cases, just restart the failing actor
                else return Directive.Stop;
            });
        }
        

    
    }
}

