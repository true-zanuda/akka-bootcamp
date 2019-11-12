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

            PrintInstructions();

            var writer = MyActorSystem.ActorOf(Props.Create(() => new ConsoleWriterActor()));
            var reader = MyActorSystem.ActorOf(Props.Create(() => new ConsoleReaderActor(writer)));

            reader.Tell("start");

            // blocks the main thread from exiting until the actor system is shut down
            MyActorSystem.WhenTerminated.Wait();
        }

        private static void PrintInstructions()
        {
            Console.WriteLine("Write whatever you want into the console!");
            Console.Write("Some lines will appear as");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(" red ");
            Console.ResetColor();
            Console.Write(" and others will appear as");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" green! ");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Type 'exit' to quit this application at any time.\n");
        }
    }
}