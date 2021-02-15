using System;
using System.Threading;
using System.Threading.Tasks;

using Examples.XConnect.Copy.Status;

using Serilog;

using Sitecore.XConnect;

namespace Examples.XConnect.Copy.Write
{
    public abstract class Writer
    {
        public Task Start()
        {
            Task result = new Task(WriteContactsFromQueue, TaskCreationOptions.LongRunning);
            result.Start();
            return result;
        }

        protected abstract void ProcessContact(Contact contact);

        private static void HandleException(Exception ex, Contact contact)
        {
            if (ex is AggregateException aex)
            {
                foreach (Exception iex in aex.InnerExceptions)
                {
                    Log.Logger.Error(iex, $"For contact {contact?.Id} exception \"{iex.Message}\" occurred");
                }
            }
            else
            {
                Log.Logger.Error(ex, $"For contact {contact?.Id} exception \"{ex.Message}\" occurred");
            }

            CurrentStatus.ExceptionCounterAdd(1);
        }

        private void WriteContactsFromQueue()
        {
            Log.Logger.Debug($"Thread {Thread.CurrentThread.ManagedThreadId} started writing...");
            Contact contact;
            while (CurrentStatus.IsReading() || !CurrentStatus.IsQueueEmpty())
            {
                if (CurrentStatus.TryDequeue(out contact))
                {
                    try
                    {
                        ProcessContact(contact);
                        CurrentStatus.WriteCounterAdd(1);
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex, contact);
                    }
                }
                else
                {
                    // Reader is running behind, give it some time
                    Thread.Sleep(100);
                }
            }

            Log.Logger.Debug($"Thread {Thread.CurrentThread.ManagedThreadId} is done writing...");
        }
    }
}
