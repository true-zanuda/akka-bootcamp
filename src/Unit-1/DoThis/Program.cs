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

            var writerProps = Props.Create<ConsoleWriterActor>();
            var writer = MyActorSystem.ActorOf(writerProps, "consoleWriterActor");

            var validatorProps = Props.Create(() => new ValidationActor(writer));
            var validator = MyActorSystem.ActorOf(validatorProps, "validator");

            var readerProps = Props.Create<ConsoleReaderActor>(validator);
            var reader = MyActorSystem.ActorOf(readerProps);

            reader.Tell(ConsoleReaderActor.StartCommand);

            MyActorSystem.WhenTerminated.Wait();
        }
    }
}