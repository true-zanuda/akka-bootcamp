using Akka.Actor;
using System;
using System.IO;

namespace WinTail
{
    /// <summary>
    /// Actor that validates user input and signals result to others.
    /// </summary>
    public class FileValidatorActor : UntypedActor
    {
        public FileValidatorActor(IActorRef consoleWriterActor)
        {
            _consoleWriterActor = consoleWriterActor;
        }

        protected override void OnReceive(Object message)
        {
            var msg = message as String;
            if (String.IsNullOrEmpty(msg))
            {
                // signal that the user needs to supply an input
                _consoleWriterActor.Tell(new Messages.NullInputError("Input was blank. Please try again.\n"));

                // tell sender to continue doing its thing (whatever that may be, this actor doesn't care)
                Sender.Tell(new Messages.ContinueProcessing());
            }
            else
            {
                var valid = IsFileUri(msg);
                if (valid)
                {
                    // signal successful input
                    _consoleWriterActor.Tell(new Messages.InputSuccess(String.Format("Starting processing for {0}", msg)));

                    // start coordinator
                    var coordinator = Context.ActorSelection("akka://ActorSystem/user/coordinator");
                    coordinator.Tell(new TailCoordinatorActor.StartTail(msg, _consoleWriterActor));
                }
                else
                {
                    // signal that input was bad
                    _consoleWriterActor.Tell(new Messages.ValidationError(String.Format("{0} is not an existing URI on disk.", msg)));

                    // tell sender to continue doing its thing (whatever that may be, this actor doesn't care)
                    Sender.Tell(new Messages.ContinueProcessing());
                }
            }
        }

        private readonly IActorRef _consoleWriterActor;

        /// <summary>
        /// Checks if file exists at path provided by user.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static Boolean IsFileUri(String path)
        {
            return File.Exists(path);
        }
    }
}