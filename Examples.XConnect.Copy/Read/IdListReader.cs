using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Examples.XConnect.Copy.Config;
using Examples.XConnect.Copy.Status;

using Serilog;

using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Operations;

namespace Examples.XConnect.Copy.Read
{
    public class IdListReader : Reader
    {
        public IdListReader(XConnectClient client, Configuration config)
        : base(client, config)
        {
        }

        protected override async void ReadContacts()
        {
            Log.Logger.Debug("Started reading...");
            IEnumerator<Guid> ids = GetIdsFromFile();
            while (CurrentStatus.IsReading())
            {
                if (CurrentStatus.ContactsQueued < Configuration.MaxQueueSize)
                {
                    await ReadBatch(ids);
                }
                else
                {
                    // Writers are running behind, give them some time
                    Thread.Sleep(100);
                }
            }

            Log.Logger.Debug("Done reading...");
        }

        private async Task ReadBatch(IEnumerator<Guid> ids)
        {
            List<Guid> batchIds = new List<Guid>();
            bool isReadingDone = false;
            for (int i = 0; i < Config.BatchSize; i++)
            {
                if (ids.MoveNext())
                {
                    batchIds.Add(ids.Current);
                }
                else
                {
                    isReadingDone = true;
                }
            }

            ReadOnlyCollection<IEntityLookupResult<Contact>> lookupResults = await Client.GetContactsAsync(batchIds, GetExpandOptions());
            foreach (IEntityLookupResult<Contact> lookupResult in lookupResults)
            {
                if (lookupResult.Exists)
                {
                    CurrentStatus.QueueContact(lookupResult.Entity);
                }
                else
                {
                    Log.Logger.Information($"Contact with id {lookupResult.Reference.Id} was not found!");
                }
            }

            CurrentStatus.ReadCounterAdd(1);
            if (isReadingDone)
            {
                CurrentStatus.ReadingIsDone();
            }

            Log.Logger.Debug($"Read {CurrentStatus.ReadCounter} batches");
        }

        private IEnumerator<Guid> GetIdsFromFile()
        {
            string filePath = Path.GetFullPath(Config.IdListFileName);
            Log.Logger.Debug($"Using file: {filePath}");
            if (File.Exists(filePath))
            {
                using (TextReader reader = new StreamReader(File.OpenRead(filePath)))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (Guid.TryParse(line, out Guid id))
                        {
                            yield return id;
                        }
                        else
                        {
                            Log.Logger.Debug($"Invalid entry in file: {line}");
                        }
                    }
                }
            }
        }

        private ContactExpandOptions GetExpandOptions()
        {
            return new ContactExpandOptions(Config.ContactFacets)
            {
                Interactions = new RelatedInteractionsExpandOptions(Config.InteractionFacets)
                {
                    StartDateTime = DateTime.MinValue,
                    EndDateTime = DateTime.MaxValue,
                    Limit = Config.InteractionLimit
                }
            };
        }
    }
}
