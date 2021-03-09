using System;
using System.Collections.Generic;

using Examples.XConnect.Copy.Config;
using Examples.XConnect.Copy.Data;

using Sitecore.XConnect.Client;
using Sitecore.XConnect.Client.WebApi;
using Sitecore.XConnect.Schema;
using Sitecore.Xdb.Common.Web;

namespace Examples.XConnect.Copy
{
    public class XConnectClientFactory
    {
        private readonly string _prefix;

        public XConnectClientFactory(string prefix)
        {
            _prefix = prefix;
            Configuration = new Configuration(_prefix);
        }

        public Configuration Configuration { get; }

        public XConnectClient BuildClient(bool enableExtraction = false)
        {
            XConnectClientConfiguration clientConfig = BuildXConnectClientConfiguration(enableExtraction);
            clientConfig.Initialize();
            return new XConnectClient(clientConfig);
        }

        private static string Prefix(string text, string prefix)
        {
            return string.IsNullOrWhiteSpace(prefix) ? text : string.Concat(prefix, ".", text);
        }

        private XConnectClientConfiguration BuildXConnectClientConfiguration(bool enableExtraction)
        {
            List<IHttpClientModifier> clientModifiers = GetHttpClientModifiers();

            string connectionStringName = GetConnectionStringName(ConnectionType.Collection);
            CollectionWebApiClient collectionClient = new CollectionWebApiClient(
                GetWebApiClientUri(connectionStringName, ConnectionType.Collection),
                clientModifiers,
                new[] { GetCertificateWebRequestHandlerModifier(connectionStringName + ".certificate") });

            connectionStringName = GetConnectionStringName(ConnectionType.Search);
            SearchWebApiClient searchClient = new SearchWebApiClient(
                GetWebApiClientUri(connectionStringName, ConnectionType.Collection),
                clientModifiers,
                new[] { GetCertificateWebRequestHandlerModifier(connectionStringName + ".certificate") });

            connectionStringName = GetConnectionStringName(ConnectionType.Configuration);
            ConfigurationWebApiClient configurationClient = new ConfigurationWebApiClient(
                GetWebApiClientUri(connectionStringName, ConnectionType.Configuration),
                clientModifiers,
                new[] { GetCertificateWebRequestHandlerModifier(connectionStringName + ".certificate") });

            return new XConnectClientConfiguration(
                new XdbRuntimeModel(Configuration.Model),
                collectionClient,
                searchClient,
                configurationClient,
                enableExtraction);
        }

        private List<IHttpClientModifier> GetHttpClientModifiers()
        {
            List<IHttpClientModifier> result = new List<IHttpClientModifier>(1);
            TimeoutHttpClientModifier timeoutClientModifier = new TimeoutHttpClientModifier(TimeSpan.FromSeconds(Configuration.Timeout));
            result.Add(timeoutClientModifier);
            return result;
        }

        private string GetConnectionStringName(ConnectionType type)
        {
            string result = Prefix("xconnect.collection", _prefix);
            string collection = result;
            switch (type)
            {
                case ConnectionType.Search:
                    result = Prefix("xconnect.search", _prefix);
                    string searchConnectionString = Configuration.GetConnectionString(result);
                    if (string.IsNullOrWhiteSpace(searchConnectionString))
                    {
                        result = collection;
                    }

                    break;
                case ConnectionType.Configuration:
                    result = Prefix("xconnect.configuration", _prefix);
                    string configConnectionString = Configuration.GetConnectionString(result);
                    if (string.IsNullOrWhiteSpace(configConnectionString))
                    {
                        result = collection;
                    }

                    break;
            }

            return result;
        }

        private CertificateHttpClientHandlerModifier GetCertificateWebRequestHandlerModifier(
            string connectionStringName)
        {
            string certificateOptionsSettingValue = Configuration.GetConnectionString(connectionStringName);
            CertificateHttpClientHandlerModifierOptions options =
                CertificateHttpClientHandlerModifierOptions.Parse(certificateOptionsSettingValue);

            return new CertificateHttpClientHandlerModifier(options);
        }

        private Uri GetWebApiClientUri(string connectionStringName, ConnectionType type)
        {
            string baseUri = Configuration.GetConnectionString(connectionStringName);
            string path = string.Empty;
            switch (type)
            {
                case ConnectionType.Collection:
                case ConnectionType.Search:
                    path = "/odata";
                    break;
                case ConnectionType.Configuration:
                    path = "/configuration";
                    break;
            }

            return new Uri(new Uri(baseUri), path);
        }
    }
}
