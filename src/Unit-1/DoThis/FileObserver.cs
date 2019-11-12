using Akka.Actor;
using System;
using System.IO;

namespace WinTail
{
    /// <summary>
    /// Turns <see cref="FileSystemWatcher"/> events about a specific file into
    /// messages for <see cref="TailActor"/>.
    /// </summary>
    public class FileObserver : IDisposable
    {
        public FileObserver(IActorRef tailActor, String absoluteFilePath)
        {
            _tailActor = tailActor;
            _absoluteFilePath = absoluteFilePath;
            _fileDir = Path.GetDirectoryName(absoluteFilePath);
            _fileNameOnly = Path.GetFileName(absoluteFilePath);
        }

        /// <summary>
        /// Begin monitoring file.
        /// </summary>
        public void Start()
        {
            // Need this for Mono 3.12.0 workaround
            // uncomment next line if you're running on Mono!
            // Environment.SetEnvironmentVariable("MONO_MANAGED_WATCHER", "enabled");

            // make watcher to observe our specific file
            _watcher = new FileSystemWatcher(_fileDir, _fileNameOnly);

            // watch our file for changes to the file name,
            // or new messages being written to file
            _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

            // assign callbacks for event types
            _watcher.Changed += OnFileChanged;
            _watcher.Error += OnFileError;

            // start watching
            _watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Stop monitoring file.
        /// </summary>
        public void Dispose()
        {
            _watcher.Dispose();
        }

        private readonly IActorRef _tailActor;
        private readonly String _absoluteFilePath;
        private FileSystemWatcher _watcher;
        private readonly String _fileDir;
        private readonly String _fileNameOnly;

        /// <summary>
        /// Callback for <see cref="FileSystemWatcher"/> file error events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnFileError(Object sender, ErrorEventArgs e)
        {
            _tailActor.Tell(new TailActor.FileError(_fileNameOnly, e.GetException().Message), ActorRefs.NoSender);
        }

        /// <summary>
        /// Callback for <see cref="FileSystemWatcher"/> file change events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnFileChanged(Object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                // here we use a special ActorRefs.NoSender
                // since this event can happen many times,
                // this is a little microoptimization
                _tailActor.Tell(new TailActor.FileWrite(e.Name), ActorRefs.NoSender);
            }
        }
    }
}