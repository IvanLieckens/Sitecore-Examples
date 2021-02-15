using System;
using System.Collections.Generic;
using System.Threading;

using Examples.XConnect.Copy.Config;
using Examples.XConnect.Copy.Status;

using Serilog;

using Sitecore.XConnect;
using Sitecore.XConnect.Client;

namespace Examples.XConnect.Copy.Read
{
    public class DataExtractionReader : Reader
    {
        public DataExtractionReader(XConnectClient client, Configuration config)
            : base(client, config)
        {
        }

        protected override async void ReadContacts()
        {
            Log.Logger.Debug("Started reading...");
            IAsyncEntityBatchEnumerator<Contact> cursor =
                await Client.CreateContactEnumerator(GetExpandOptions(), Config.BatchSize);
            while (CurrentStatus.IsReading())
            {
                if (CurrentStatus.ContactsQueued < Configuration.MaxQueueSize)
                {
                    if (await cursor.MoveNext())
                    {
                        foreach (Contact contact in cursor.Current)
                        {
                            CurrentStatus.QueueContact(contact);
                        }

                        CurrentStatus.ReadCounterAdd(1);
                        Log.Logger.Debug($"Read {CurrentStatus.ReadCounter} batches");
                    }
                    else
                    {
                        CurrentStatus.ReadingIsDone();
                    }
                }
                else
                {
                    // Writers are running behind, give them some time
                    Thread.Sleep(100);
                }
            }

            Log.Logger.Debug("Done reading...");
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
