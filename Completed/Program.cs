using Akka.Actor;
using Akka.Actor.Setup;
using Akka.Configuration;
using System;

namespace WinTail
{
    class Program
    {
        public static ActorSystem MyActorSystem;

        static void Main(string[] args)
        {
            // make actor system 
            MyActorSystem = ActorSystem.Create("MyActorSystem");

            // create top-level actors within the actor system
            Props consoleWriterProps = Props.Create<ConsoleWriterActor>();
            IActorRef consoleWriterActor = MyActorSystem.ActorOf(consoleWriterProps, "consoleWriterActor");

            Props tailCoordinatorCoordinatorProps = Props.Create(() => new TailCoordinatorCoordinatorActor());
            IActorRef tailCoordinatorCoordinatorActor = MyActorSystem.ActorOf(tailCoordinatorCoordinatorProps, "tailCoordinatorCoordinatorActor");

            Props validatorActorProps = Props.Create(() => new ValidatorActor(consoleWriterActor));
            IActorRef validatorActor = MyActorSystem.ActorOf(validatorActorProps, "validationActor");
            
            Props consoleReaderProps = Props.Create<ConsoleReaderActor>();
            IActorRef consoleReaderActor = MyActorSystem.ActorOf(consoleReaderProps, "consoleReaderActor");

            // begin processing
            consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

            //MyActorSystem
            //   .Scheduler
            //   .ScheduleTellRepeatedly(TimeSpan.FromSeconds(0),
            //             TimeSpan.FromSeconds(5),
            //            validatorActor, "1234", ActorRefs.NoSender);

            //circuit breaker;

            // The example below shows how to deploy 5 workers using a round robin router:
            //var props = Props.Create<Worker>().WithRouter(new RoundRobinPool(5));
            //var actor = system.ActorOf(props, "worker");

            // blocks the main thread from exiting until the actor system is shut down
            MyActorSystem.WhenTerminated.Wait();
        }

    }
}
