using Sitecore.ContentTesting.Model.xConnect.Models;
using Sitecore.EmailCampaign.Model.XConnect;
using Sitecore.XConnect.Collection.Model;
using Sitecore.XConnect.Schema;

namespace Examples.XConnect.Copy.Model
{
    public static class CopyXdbModel
    {
        public const string ModelName = "Examples.XConnect.CopyModel";

        public static XdbModel Model { get; } = CopyXdbModel.BuildModel();

        private static XdbModel BuildModel()
        {
            XdbModelBuilder xdbModelBuilder = new XdbModelBuilder("Examples.XConnect.CopyModel", new XdbModelVersion(1, 0));
            xdbModelBuilder.ReferenceModel(CollectionModel.Model);
            xdbModelBuilder.ReferenceModel(CustomDataModel.Model);
            xdbModelBuilder.ReferenceModel(EmailCollectionModel.Model);
            return xdbModelBuilder.BuildModel();
        }
    }
}
