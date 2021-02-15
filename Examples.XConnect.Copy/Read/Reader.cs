using System.Threading.Tasks;

using Examples.XConnect.Copy.Config;

using Sitecore.XConnect.Client;

namespace Examples.XConnect.Copy.Read
{
    public abstract class Reader
    {
        protected Reader(XConnectClient client, Configuration config)
        {
            Client = client;
            Config = config;
        }

        protected XConnectClient Client { get; }

        protected Configuration Config { get; }

        public Task Start()
        {
            Task result = new Task(ReadContacts, TaskCreationOptions.LongRunning);
            result.Start();
            return result;
        }

        protected abstract void ReadContacts();
    }
}
