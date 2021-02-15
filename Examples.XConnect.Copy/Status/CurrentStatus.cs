using System.Collections.Concurrent;
using System.Threading;

using Serilog;

using Sitecore.XConnect;

namespace Examples.XConnect.Copy.Status
{
    public static class CurrentStatus
    {
        private static readonly ConcurrentQueue<Contact> _Queue = new ConcurrentQueue<Contact>();

        private static int _readCounter;

        private static int _writeCounter;

        private static int _addCounter;

        private static int _updateCounter;

        private static int _skippedCounter;

        private static int _exceptionCounter;

        private static bool _reading = true;

        public static int ReadCounter => _readCounter;

        public static int WriteCounter => _writeCounter;

        public static int ContactsQueued => _Queue.Count;

        public static string GetStatus()
        {
            return
                $"Read {_readCounter} batches, {_Queue.Count} contacts are waiting for processing, {_writeCounter} processed of which {_addCounter} added, {_updateCounter} updated and {_skippedCounter} skipped. {_exceptionCounter} exceptions encountered.";
        }

        public static bool TryDequeue(out Contact contact)
        {
            return _Queue.TryDequeue(out contact);
        }

        public static bool IsQueueEmpty()
        {
            return _Queue.IsEmpty;
        }

        public static bool IsReading()
        {
            return _reading;
        }

        public static void QueueContact(Contact contact)
        {
            _Queue.Enqueue(contact);
            Log.Logger.Debug($"Queued contact {contact?.Id}");
        }

        public static void ReadCounterAdd(int number)
        {
            Interlocked.Add(ref _readCounter, number);
        }

        public static void WriteCounterAdd(int number)
        {
            Interlocked.Add(ref _writeCounter, number);
        }

        public static void AddCounterAdd(int number)
        {
            Interlocked.Add(ref _addCounter, number);
        }

        public static void UpdateCounterAdd(int number)
        {
            Interlocked.Add(ref _updateCounter, number);
        }

        public static void SkippedCounterAdd(int number)
        {
            Interlocked.Add(ref _skippedCounter, number);
        }

        public static void ExceptionCounterAdd(int number)
        {
            Interlocked.Add(ref _exceptionCounter, number);
        }

        public static void ReadingIsDone()
        {
            _reading = false;
        }
    }
}
