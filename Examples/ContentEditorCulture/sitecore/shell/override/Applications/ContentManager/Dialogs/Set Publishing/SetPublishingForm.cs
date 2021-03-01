using System;
using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Examples.ContentEditorCulture.Utils;

using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;

using Control = Sitecore.Web.UI.HtmlControls.Control;

// ReSharper disable once CheckNamespace - Can't user override (system keyword) and can't use Sitecore as it messes up namespace hierarchy
namespace Examples.ContentEditorCulture.Shell.Applications.ContentManager.Dialogs.Set_Publishing
{
    public class SetPublishingForm : DialogForm
    {
        // ReSharper disable once StyleCop.SA1401 - XML control
        // ReSharper disable once StyleCop.SA1306 - Must match
        protected Border Warning;

        // ReSharper disable once StyleCop.SA1401 - XML control
        // ReSharper disable once StyleCop.SA1306 - Must match
        protected Border Versions;

        // ReSharper disable once StyleCop.SA1401 - XML control
        // ReSharper disable once StyleCop.SA1306 - Must match
        protected Border PublishPanel;

        // ReSharper disable once StyleCop.SA1401 - XML control
        // ReSharper disable once StyleCop.SA1306 - Must match
        protected Checkbox NeverPublish;

        // ReSharper disable once StyleCop.SA1401 - XML control
        // ReSharper disable once StyleCop.SA1306 - Must match
        protected DateTimePicker Publish;

        // ReSharper disable once StyleCop.SA1401 - XML control
        // ReSharper disable once StyleCop.SA1306 - Must match
        // ReSharper disable once IdentifierTypo - Must match
        protected DateTimePicker Unpublish;

        // ReSharper disable once StyleCop.SA1401 - XML control
        // ReSharper disable once StyleCop.SA1306 - Must match
        protected Border PublishingTargets;

        protected bool ReadOnly
        {
            get => MainUtil.GetBool(ServerProperties[nameof(ReadOnly)], false);

            set => ServerProperties[nameof(ReadOnly)] = value;
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, nameof(e));
            base.OnLoad(e);
            if (Context.ClientPage.IsEvent)
            {
                return;
            }

            if (WebUtil.GetQueryString("ro") == "1")
            {
                ReadOnly = true;
            }

            Item itemFromQueryString = UIUtil.GetItemFromQueryString(Context.ContentDatabase);
            Error.AssertItemFound(itemFromQueryString);
            RenderItemTab(itemFromQueryString);
            RenderVersions(itemFromQueryString);
            RenderTargetTab(itemFromQueryString);
            if (!ReadOnly)
            {
                return;
            }

            SetReadonly();
        }

        protected override void OnOK(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(args, nameof(args));
            Item itemFromQueryString = UIUtil.GetItemFromQueryString(Context.ContentDatabase);
            Error.AssertItemFound(itemFromQueryString);
            ListString listString = new ListString();
            using (new StatisticDisabler(StatisticDisablerState.ForItemsWithoutVersionOnly))
            {
                itemFromQueryString.Editing.BeginEdit();
                itemFromQueryString.Publishing.NeverPublish = !NeverPublish.Checked;
                ItemPublishing publishing1 = itemFromQueryString.Publishing;
                string publishDateTimeValue = Publish.Value;
                DateTimeOffset dateTimeOffset = DateTimeOffset.MinValue;
                DateTime utcDateTime1 = dateTimeOffset.UtcDateTime;
                DateTime universalTime1 = TimeZoneUtil.ToUniversalTime(DateUtil.ParseDateTime(publishDateTimeValue, utcDateTime1));
                publishing1.PublishDate = universalTime1;
                ItemPublishing publishing2 = itemFromQueryString.Publishing;
                // ReSharper disable once IdentifierTypo - Original
                string unpublishDateTimeValue = Unpublish.Value;
                dateTimeOffset = DateTimeOffset.MaxValue;
                DateTime utcDateTime2 = dateTimeOffset.UtcDateTime;
                DateTime universalTime2 = TimeZoneUtil.ToUniversalTime(DateUtil.ParseDateTime(unpublishDateTimeValue, utcDateTime2));
                publishing2.UnpublishDate = universalTime2;
                foreach (string key in Context.ClientPage.ClientRequest.Form.Keys)
                {
                    if (key != null && key.StartsWith("pb_", StringComparison.InvariantCulture))
                    {
                        string str3 = ShortID.Decode(StringUtil.Mid(key, 3));
                        listString.Add(str3);
                    }
                }

                itemFromQueryString[FieldIDs.PublishingTargets] = listString.ToString();
                itemFromQueryString.Editing.EndEdit();
            }

            Log.Audit(
                this,
                "Set publishing targets: {0}, targets: {1}",
                AuditFormatter.FormatItem(itemFromQueryString),
                listString.ToString());
            foreach (string key in Context.ClientPage.ClientRequest.Form.Keys)
            {
                if (key != null && key.StartsWith("pb_", StringComparison.InvariantCulture))
                {
                    string str = ShortID.Decode(StringUtil.Mid(key, 3));
                    listString.Add(str);
                }
            }

            foreach (Item version in itemFromQueryString.Versions.GetVersions())
            {
                // ReSharper disable once PossiblyMistakenUseOfParamsMethod - Copy from original
                bool b = StringUtil.GetString(Context.ClientPage.ClientRequest.Form["hide_" + version.Version.Number]).Length <= 0;
                // ReSharper disable once StringLiteralTypo - Must match
                DateTimePicker validFrom = Versions.FindControl("validfrom_" + version.Version.Number) as DateTimePicker;
                // ReSharper disable once StringLiteralTypo - Must match
                DateTimePicker validTo = Versions.FindControl("validto_" + version.Version.Number) as DateTimePicker;
                DateTime validFromUtc = TimeZoneUtil.ToUniversalTime(DateUtil.IsoDateToDateTime(validFrom?.Value, DateTimeOffset.MinValue.UtcDateTime));
                DateTime validToUtc = TimeZoneUtil.ToUniversalTime(DateUtil.IsoDateToDateTime(validTo?.Value, DateTimeOffset.MaxValue.UtcDateTime));
                if (
                    b != version.Publishing.HideVersion
                    || DateUtil.CompareDatesIgnoringSeconds(validFromUtc, version.Publishing.ValidFrom) != 0
                    || DateUtil.CompareDatesIgnoringSeconds(validToUtc, version.Publishing.ValidTo) != 0)
                {
                    version.Editing.BeginEdit();
                    version.Publishing.ValidFrom = validFromUtc;
                    version.Publishing.ValidTo = validToUtc;
                    version.Publishing.HideVersion = b;
                    version.Editing.EndEdit();
                    Log.Audit(
                        this,
                        "Set publishing valid: {0}, from: {1}, to:{2}, hide: {3}",
                        AuditFormatter.FormatItem(version),
                        validFromUtc.ToString(CultureInfo.InvariantCulture),
                        validToUtc.ToString(CultureInfo.InvariantCulture),
                        MainUtil.BoolToString(b));
                }
            }

            SheerResponse.SetDialogValue("yes");
            base.OnOK(sender, args);
        }

        protected virtual void SetNeverPublish()
        {
            bool isDisabled = !NeverPublish.Checked;
            Publish.Disabled = isDisabled;
            Unpublish.Disabled = isDisabled;
            Item itemFromQueryString = UIUtil.GetItemFromQueryString(Context.ContentDatabase);
            Error.AssertItemFound(itemFromQueryString);
            if (Context.ClientPage.IsEvent)
            {
                foreach (Item version in itemFromQueryString.Versions.GetVersions())
                {
                    UpdateVersionState(version.Version.Number, isDisabled);
                }
            }

            Context.ClientPage.ClientResponse.Refresh(Publish);
            Context.ClientPage.ClientResponse.Refresh(Unpublish);
        }

        protected void RenderItemTab(Item item)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            NeverPublish.Checked = !item.Publishing.NeverPublish;
            Publish.Value = item.Publishing.PublishDate == DateTimeOffset.MinValue.UtcDateTime ? string.Empty : DateUtil.ToIsoDate(TimeZoneUtil.ToUserTime(item.Publishing.PublishDate));
            Unpublish.Value = item.Publishing.UnpublishDate == DateTimeOffset.MaxValue.UtcDateTime ? string.Empty : DateUtil.ToIsoDate(TimeZoneUtil.ToUserTime(item.Publishing.UnpublishDate));
            SetNeverPublish();
        }

        protected void RenderVersions(Item item)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            Item[] versions = item.Versions.GetVersions();
            StringBuilder versionsHeaderBuilder = new StringBuilder("<table class='scListControl scVersionsTable'>");
            versionsHeaderBuilder.Append("<tr>");
            versionsHeaderBuilder.Append("<td nowrap=\"nowrap\"><b>" + TranslateText("Version") + "</b></td>");
            versionsHeaderBuilder.Append("<td nowrap=\"nowrap\"><b>" + TranslateText("Publishable") + "</b></td>");
            versionsHeaderBuilder.Append("<td width=\"50%\"><b>" + TranslateText("Publishable from") + "</b></td>");
            versionsHeaderBuilder.Append("<td width=\"50%\"><b>" + TranslateText("Publishable to") + "</b></td>");
            versionsHeaderBuilder.Append("</tr>");
            Versions.Controls.Add(new LiteralControl(versionsHeaderBuilder.ToString()));
            string disabledAttribute = string.Empty;
            bool flag = item.Publishing.NeverPublish || ReadOnly || !item.Access.CanWriteLanguage() || !NeverPublish.Checked;
            if (flag)
            {
                disabledAttribute = " disabled=\"true\"";
            }

            foreach (Item version in versions)
            {
                StringBuilder versionBuilder = new StringBuilder();
                string versionNumber = version.Version.Number.ToString();
                string backgroundAttribute = version.Version == item.Version ? " style=\"background-color:#D0EBF6\"" : string.Empty;
                versionBuilder.Append("<tr" + backgroundAttribute + ">");
                versionBuilder.AppendFormat("<td class='scVersionNumber'><b>{0}.</b></td>", version.Version.Number);
                versionBuilder.AppendFormat("<td class='scPublishable'><input id=\"hide_" + versionNumber + "\" type=\"checkbox\"" + (version.Publishing.HideVersion ? string.Empty : " checked=\"checked\"") + disabledAttribute + "/></td>");
                versionBuilder.Append("<td>");
                Versions.Controls.Add(new LiteralControl(versionBuilder.ToString()));
                DateTimePicker dateTimePicker1 = new DateTimePicker
                {
                    // ReSharper disable once StringLiteralTypo - Must match
                    ID = "validfrom_" + versionNumber,
                    Width = new Unit(100.0, UnitType.Percentage),
                    Value = version.Publishing.ValidFrom == DateTime.MinValue
                                                                     ? string.Empty
                                                                     : DateUtil.ToIsoDate(
                                                                         TimeZoneUtil.ToUserTime(
                                                                             version.Publishing.ValidFrom))
                };
                Versions.Controls.Add(dateTimePicker1);
                Versions.Controls.Add(new LiteralControl("</td><td>"));
                DateTimePicker dateTimePicker2 = new DateTimePicker
                {
                    // ReSharper disable once StringLiteralTypo - Must match
                    ID = "validto_" + versionNumber,
                    Width = new Unit(100.0, UnitType.Percentage),
                    Value = version.Publishing.ValidTo == DateTime.MaxValue
                                                                     ? string.Empty
                                                                     : DateUtil.ToIsoDate(
                                                                         TimeZoneUtil.ToUserTime(version.Publishing.ValidTo))
                };
                Versions.Controls.Add(dateTimePicker2);
                if (flag)
                {
                    dateTimePicker2.Disabled = true;
                    dateTimePicker1.Disabled = true;
                }

                Versions.Controls.Add(new LiteralControl("</td></tr>"));
            }

            Versions.Controls.Add(new LiteralControl("</table>"));
        }

        protected virtual string TranslateText(string key)
        {
            return Translate.Text(key);
        }

        private void UpdateVersionState(int versionNumber, bool isDisabled)
        {
            string str = isDisabled ? "true" : "false";
            SheerResponse.SetAttribute("hide_" + versionNumber, "disabled", str);
            // ReSharper disable once StringLiteralTypo - Must match
            ChangeDateTimePickerState(Versions, "validto_" + versionNumber, isDisabled);
            // ReSharper disable once StringLiteralTypo - Must match
            ChangeDateTimePickerState(Versions, "validfrom_" + versionNumber, isDisabled);
        }

        private void ChangeDateTimePickerState(Control parent, string controlId, bool state)
        {
            DateTimePicker control = parent.FindControl(controlId) as DateTimePicker;
            if (control == null)
            {
                return;
            }

            control.Disabled = state;
            Context.ClientPage.ClientResponse.Refresh(control);
        }

        private void RenderTargetTab(Item item)
        {
            Assert.ArgumentNotNull(item, nameof(item));
            Field field = item.Fields[FieldIDs.PublishingTargets];
            if (field == null)
            {
                return;
            }

            Item obj = Context.ContentDatabase.Items["/sitecore/system/publishing targets"];
            if (obj == null)
            {
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();
            string str1 = field.Value;
            foreach (Item child in obj.Children)
            {
                string str2 = str1.IndexOf(child.ID.ToString(), StringComparison.InvariantCulture) >= 0 ? " checked=\"true\"" : string.Empty;
                string str3 = string.Empty;
                if (ReadOnly)
                {
                    str3 = " disabled=\"true\"";
                }

                stringBuilder.Append("<input id=\"pb_" + ShortID.Encode(child.ID) + "\" name=\"pb_" + ShortID.Encode(child.ID) + "\" class=\"scRibbonCheckbox\" type=\"checkbox\"" + str2 + str3 + " style=\"vertical-align:middle\"/>");
                stringBuilder.Append(child.GetUIDisplayName());
                stringBuilder.Append("<br/>");
            }

            PublishingTargets.InnerHtml = stringBuilder.ToString();
        }

        private void SetReadonly()
        {
            ReadOnly = true;
            NeverPublish.Disabled = true;
            Publish.Disabled = true;
            Unpublish.Disabled = true;
            Warning.Visible = true;
        }
    }
}