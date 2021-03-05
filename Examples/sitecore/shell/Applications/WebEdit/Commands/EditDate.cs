using Examples.ContentEditorCulture.Utils;

using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;

// ReSharper disable once CheckNamespace - Don't use Sitecore in custom namespaces
namespace Examples.ContentEditorCulture.Shell.Applications.WebEdit.Commands
{
    public class EditDate : Sitecore.Shell.Applications.WebEdit.Commands.EditDate
    {
        protected override void Run(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));
            // ReSharper disable once StringLiteralTypo - Must match
            string parameter = args.Parameters["controlid"];
            if (args.IsPostBack)
            {
                if (!args.HasResult)
                {
                    return;
                }

                args.Result = TimeZoneUtil.IsoDateToUtcIsoDate(args.Result);
                string str = RenderDate(args);
                if (str == null)
                {
                    return;
                }

                SheerResponse.SetAttribute("scHtmlValue", "value", str);
                SheerResponse.SetAttribute("scPlainValue", "value", args.Result);
                SheerResponse.Eval("scSetHtmlValue('" + parameter + "')");
            }
            else
            {
                SheerResponse.ShowModalDialog(
                    new UrlString("/sitecore/client/Applications/ExperienceEditor/Dialogs/SelectDateTime")
                    {
                        ["sc_date"] = TimeZoneUtil.IsoDateToUserTimeIsoDate(args.Parameters["date"]),
                        ["Header"] = "Select a date",
                        ["Description"] = "Locate and select the desired date and time."
                    }.ToString(),
                    true);
                args.WaitForPostBack();
            }
        }

        private static string RenderDate(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, nameof(args));
            string result = args.Result;
            string parameter1 = args.Parameters["format"];
            // ReSharper disable once StringLiteralTypo - Must match
            string parameter2 = args.Parameters["itemid"];
            string parameter3 = args.Parameters["language"];
            string parameter4 = args.Parameters["version"];
            // ReSharper disable once StringLiteralTypo - Must match
            string parameter5 = args.Parameters["fieldid"];
            Item obj = Context.ContentDatabase.GetItem(ID.Parse(parameter2), Language.Parse(parameter3), Version.Parse(parameter4));
            if (obj == null)
            {
                SheerResponse.Alert("The item was not found.\n\nIt may have been deleted by another user.");
                return null;
            }

            Field field = obj.Fields[ID.Parse(parameter5)];
            if (field == null)
            {
                SheerResponse.Alert("The field was not found.\n\nIt may have been deleted by another user.");
                return null;
            }

            using (FieldRenderer fieldRenderer = new FieldRenderer())
            {
                fieldRenderer.Item = obj;
                fieldRenderer.FieldName = field.Name;
                fieldRenderer.Parameters = "format=" + parameter1;
                fieldRenderer.OverrideFieldValue(result);
                fieldRenderer.DisableWebEditing = true;
                return fieldRenderer.Render();
            }
        }
    }
}