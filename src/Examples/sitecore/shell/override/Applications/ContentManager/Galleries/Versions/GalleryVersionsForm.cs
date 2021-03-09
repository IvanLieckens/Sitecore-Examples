using System;
using System.Globalization;
using System.Web.UI.HtmlControls;

using Examples.ContentEditorCulture.Utils;

using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell.Applications.ContentManager.Galleries;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XmlControls;

using Version = Sitecore.Data.Version;

// ReSharper disable once CheckNamespace - Can't user override (system keyword) and can't use Sitecore as it messes up namespace hierarchy
namespace Examples.ContentEditorCulture.Shell.Applications.ContentManager.Galleries.Versions
{
    public class GalleryVersionsForm : GalleryForm
    {
        // ReSharper disable once StyleCop.SA1401 - XML control
        // ReSharper disable once StyleCop.SA1306 - Must match
        protected GalleryMenu Options;

        // ReSharper disable once StyleCop.SA1401 - XML control
        // ReSharper disable once StyleCop.SA1306 - Must match
        protected Scrollbox Versions;

        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull(message, nameof(message));
            if (message.Name == "event:click")
            {
                return;
            }

            Invoke(message, true);
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, nameof(e));
            base.OnLoad(e);
            if (Context.ClientPage.IsEvent)
            {
                return;
            }

            Item currentItem = GetCurrentItem();
            if (currentItem != null)
            {
                if (currentItem.IsFallback)
                {
                    HtmlGenericControl htmlGenericControl = new HtmlGenericControl("div");
                    htmlGenericControl.InnerText = Translate.Text("No version exists in the current language. You see a fallback version from '{0}' language.", (object)currentItem.OriginalLanguage);
                    htmlGenericControl.Attributes["class"] = "versionNumSelected";
                    Context.ClientPage.AddControl(Versions, htmlGenericControl);
                }
                else
                {
                    Item[] versions = currentItem.Versions.GetVersions();
                    for (int index = versions.Length - 1; index >= 0; --index)
                    {
                        Item obj = versions[index];
                        XmlControl control = ControlFactory.GetControl("Gallery.Versions.Option") as XmlControl;
                        Assert.IsNotNull(control, typeof(XmlControl), "Xml Control \"{0}\" not found", (object)"Gallery.Versions.Option");
                        Context.ClientPage.AddControl(Versions, control);
                        CultureInfo culture = Context.User.Profile.Culture;
                        string str1 = obj.Statistics.Updated == DateTime.MinValue ? Translate.Text("[Not set]") : DateUtil.FormatShortDateTime(TimeZoneUtil.ToUserTime(obj.Statistics.Updated), culture);
                        string str2 = obj.Statistics.UpdatedBy.Length == 0 ? "-" : obj.Statistics.UpdatedBy;
                        string str3 = obj.Version + ".";
                        string str4 = obj.Version.Number != currentItem.Version.Number ? "<div class=\"versionNum\">" + str3 + "</div>" : "<div class=\"versionNumSelected\">" + str3 + "</div>";
                        control["Number"] = str4;
                        control["Header"] = Translate.Text("Modified <b>{0}</b> by <b>{1}</b>.", str1, str2);
                        control["Click"] =
                            $"item:load(id={currentItem.ID},language={currentItem.Language},version={obj.Version.Number})";
                    }
                }
            }

            Item obj1 = Client.CoreDatabase.GetItem("/sitecore/content/Applications/Content Editor/Menues/Versions");
            if (obj1 == null || !obj1.HasChildren)
            {
                return;
            }

            string queryString = WebUtil.GetQueryString("id");
            Options.AddFromDataSource(obj1, queryString, new CommandContext(currentItem));
        }

        /// <summary>Gets the current item.</summary>
        /// <returns>The current item.</returns>
        private static Item GetCurrentItem()
        {
            string queryString1 = WebUtil.GetQueryString("db");
            string queryString2 = WebUtil.GetQueryString("id");
            Language index1 = Language.Parse(WebUtil.GetQueryString("la"));
            Version index2 = Sitecore.Data.Version.Parse(WebUtil.GetQueryString("vs"));
            Database database = Factory.GetDatabase(queryString1);
            Assert.IsNotNull(database, queryString1);
            return database.Items[queryString2, index1, index2];
        }
    }
}