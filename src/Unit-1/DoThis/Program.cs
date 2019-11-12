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
            var writer = MyActorSystem.ActorOf(Props.Create(() => new ConsoleWriterActor()));
            var reader = MyActorSystem.ActorOf(Props.Create(() => new ConsoleReaderActor(writer)));

            reader.Tell(ConsoleReaderActor.StartCommand);

            // blocks the main thread from exiting until the actor system is shut down
            MyActorSystem.WhenTerminated.Wait();
        }
    }
}