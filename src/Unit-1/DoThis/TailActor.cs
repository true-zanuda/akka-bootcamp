using Akka.Actor;
using System;
using System.IO;
using System.Text;

namespace WinTail
{
    /// <summary>
    /// Monitors the file at <see cref="_filePath"/> for changes and sends
    /// file updates to console.
    /// </summary>
    public class TailActor : UntypedActor
    {
        public TailActor(IActorRef reporterActor, String filePath)
        {
            _reporterActor = reporterActor;
            _filePath = filePath;

            // start watching file for changes
            _observer = new FileObserver(Self, Path.GetFullPath(_filePath));
            _observer.Start();

            // open the file stream with shared read/write permissions
            // (so file can be written to while open)
            _fileStream = new FileStream(Path.GetFullPath(_filePath), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _fileStreamReader = new StreamReader(_fileStream, Encoding.UTF8);

            // read the initial contents of the file and send it to console as first msg
            var text = _fileStreamReader.ReadToEnd();
            Self.Tell(new InitialRead(_filePath, text));
        }

        protected override void OnReceive(Object message)
        {
            if (message is FileWrite)
            {
                // move file cursor forward
                // pull results from cursor to end of file and write to output
                // (this is assuming a log file type format that is append-only)
                var text = _fileStreamReader.ReadToEnd();
                if (!String.IsNullOrEmpty(text))
                {
                    _reporterActor.Tell(text);
                }
            }
            else if (message is FileError)
            {
                var fe = message as FileError;
                _reporterActor.Tell(String.Format("Tail error: {0}", fe.Reason));
            }
            else if (message is InitialRead)
            {
                var ir = message as InitialRead;
                _reporterActor.Tell(ir.Text);
            }
        }

        private readonly String _filePath;
        private readonly IActorRef _reporterActor;
        private readonly FileObserver _observer;
        private readonly Stream _fileStream;
        private readonly StreamReader _fileStreamReader;

        #region Message types

        /// <summary>
        /// Signal that the file has changed, and we need to 
        /// read the next line of the file.
        /// </summary>
        public class FileWrite
        {
            public String FileName { get; private set; }

            public FileWrite(String fileName)
            {
                FileName = fileName;
            }
        }

        /// <summary>
        /// Signal that the OS had an error accessing the file.
        /// </summary>
        public class FileError
        {
            public String FileName { get; private set; }
            public String Reason { get; private set; }

            public FileError(String fileName, String reason)
            {
                FileName = fileName;
                Reason = reason;
            }
        }

        /// <summary>
        /// Signal to read the initial contents of the file at actor startup.
        /// </summary>
        public class InitialRead
        {
            public String FileName { get; private set; }
            public String Text { get; private set; }

            public InitialRead(String fileName, String text)
            {
                FileName = fileName;
                Text = text;
            }
        }

        #endregion
    }
}