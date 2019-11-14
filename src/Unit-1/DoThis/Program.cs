using Akka.Actor;
using System;

namespace WinTail
{
    class Program
    {
        public static ActorSystem MyActorSystem;

        static void Main(String[] args)
        {
            MyActorSystem = ActorSystem.Create("ActorSystem");

            var coordinatorProps = Props.Create(() => new TailCoordinatorActor());
            var coordinator = MyActorSystem.ActorOf(coordinatorProps, "coordinator");

            var writerProps = Props.Create<ConsoleWriterActor>();
            var writer = MyActorSystem.ActorOf(writerProps, "consoleWriter");

            var validatorProps = Props.Create(() => new FileValidatorActor(writer));
            MyActorSystem.ActorOf(validatorProps, "validator");

            var readerProps = Props.Create<ConsoleReaderActor>();
            var reader = MyActorSystem.ActorOf(readerProps);

            reader.Tell(ConsoleReaderActor.StartCommand);

            MyActorSystem.WhenTerminated.Wait();
        }
    }
}