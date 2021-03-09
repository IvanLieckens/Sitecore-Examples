using System;
using System.Configuration;
using System.Reflection;

using Examples.XConnect.Copy.Model;

using Sitecore.XConnect.Collection.Model;
using Sitecore.XConnect.Schema;

namespace Examples.XConnect.Copy.Config
{
    public class Configuration
    {
        private static int? _writerThreads;

        private static int? _maxQueueSize;

        private readonly string _prefix;

        private double? _timeout;

        private int? _sqlTimeout;

        private int? _batchSize;

        private string[] _interactionFacets;

        private string[] _contactFacets;

        private int? _interactionLimit;

        private XdbModel _xdbModel;

        private string _identifierSource;

        public Configuration(string prefix)
        {
            _prefix = prefix;
        }

        public static int WriterThreads => _writerThreads ?? (_writerThreads = GetWriterThreads()).Value;

        public static int MaxQueueSize => _maxQueueSize ?? (_maxQueueSize = GetMaxQueueSize()).Value;

        public string IdListFileName { get; set; }

        public double Timeout => _timeout ?? (_timeout = GetTimeout()).Value;

        public int? SqlTimeout => _sqlTimeout ?? (_sqlTimeout = GetSqlTimeout());

        public int BatchSize => _batchSize ?? (_batchSize = GetBatchSize()).Value;

        public string[] InteractionFacets => _interactionFacets ?? (_interactionFacets = GetInteractionFacets());

        public string[] ContactFacets => _contactFacets ?? (_contactFacets = GetContactFacets());

        public int InteractionLimit => _interactionLimit ?? (_interactionLimit = GetInteractionLimit()).Value;

        public XdbModel Model => _xdbModel ?? (_xdbModel = GetXdbModel());

        public string IdentifierSource => _identifierSource ?? (_identifierSource = GetIdentifierSource());

        public string GetConnectionString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name]?.ConnectionString;
        }

        private static int GetWriterThreads()
        {
            string value = ConfigurationManager.AppSettings["WriterThreads"];
            if (!int.TryParse(value, out int result))
            {
                result = 6;
            }

            return result;
        }

        private static int GetMaxQueueSize()
        {
            string value = ConfigurationManager.AppSettings["MaxQueueSize"];
            if (!int.TryParse(value, out int result))
            {
                result = 10000;
            }

            return result;
        }

        private double GetTimeout()
        {
            string value = ConfigurationManager.AppSettings[$"{_prefix}.Timeout"];
            if (!double.TryParse(value, out double result))
            {
                result = 30;
            }

            return result;
        }

        private int? GetSqlTimeout()
        {
            string value = ConfigurationManager.AppSettings[$"{_prefix}.SqlTimeout"];
            int? result = null;
            if (int.TryParse(value, out int parsed))
            {
                result = parsed;
            }

            return result;
        }

        private int GetBatchSize()
        {
            string value = ConfigurationManager.AppSettings[$"{_prefix}.BatchSize"];
            if (!int.TryParse(value, out int result))
            {
                result = 100;
            }

            return result;
        }

        private int GetInteractionLimit()
        {
            string value = ConfigurationManager.AppSettings[$"{_prefix}.InteractionLimit"];
            if (!int.TryParse(value, out int result))
            {
                result = 100000;
            }

            return result;
        }

        private string[] GetInteractionFacets()
        {
            string facetsAppSettingValue = ConfigurationManager.AppSettings[$"{_prefix}.InteractionFacets"];
            return facetsAppSettingValue?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private string[] GetContactFacets()
        {
            string facetsAppSettingValue = ConfigurationManager.AppSettings[$"{_prefix}.ContactFacets"];
            return facetsAppSettingValue?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private XdbModel GetXdbModel()
        {
            XdbModel result = CopyXdbModel.Model;
            string value = ConfigurationManager.AppSettings[$"{_prefix}.XdbModel"];
            if (
                !string.IsNullOrWhiteSpace(value)
                && Type.GetType(value)?.GetProperty("Model", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) is XdbModel model)
            {
                result = model;
            }

            return result;
        }

        private string GetIdentifierSource()
        {
            return ConfigurationManager.AppSettings[$"{_prefix}.IdentifierSource"];
        }
    }
}
