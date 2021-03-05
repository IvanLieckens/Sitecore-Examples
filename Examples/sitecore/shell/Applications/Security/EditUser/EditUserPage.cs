using System;
using System.Linq;
using System.Web.UI.WebControls;

using Sitecore.Security;
using Sitecore.Security.Accounts;
using Sitecore.Web;

// ReSharper disable once CheckNamespace - Sitecore should not be used in project namespaces
namespace Examples.ContentEditorCulture.Shell.Applications.Security.EditUser
{
    public class EditUserPage : Sitecore.Shell.Applications.Security.EditUser.EditUserPage
    {
        // ReSharper disable once StyleCop.SA1401 - XAML controls need protected to enable override
        // ReSharper disable once StyleCop.SA1306 - Case must match XAML control ID
        protected DropDownList Timezone;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            User user = GetUser();
            UserProfile profile = user.Profile;

            RenderTimezones();
            if (!Page.IsPostBack)
            {
                string selectedTimezoneId = profile.GetCustomProperty(Constants.TimezoneUserProfileFieldKey);
                if (TimeZoneInfo.GetSystemTimeZones().Any(tz => tz.Id == selectedTimezoneId))
                {
                    Timezone.SelectedValue = selectedTimezoneId;
                }
            }
        }

        protected override void OK_Click()
        {
            base.OK_Click();
            User user = GetUser();
            if (user != null)
            {
                UserProfile profile = user.Profile;

                if (HasChanged(profile.GetCustomProperty(Constants.TimezoneUserProfileFieldKey), Timezone.SelectedValue))
                {
                    profile.SetCustomProperty(Constants.TimezoneUserProfileFieldKey, Timezone.SelectedValue);
                    profile.Save();
                }
            }
        }

        private static User GetUser()
        {
            string username = WebUtil.GetQueryString("us", null);
            return string.IsNullOrWhiteSpace(username) ? null : User.FromName(username, true);
        }

        private static bool HasChanged(string profileValue, string controlValue)
        {
            if (string.IsNullOrEmpty(controlValue))
            {
                return !string.IsNullOrEmpty(profileValue);
            }

            return profileValue != controlValue;
        }

        private void RenderTimezones()
        {
            Timezone.Items.Add(new ListItem(Translate.Text("Default"), string.Empty));
            foreach (TimeZoneInfo timezone in TimeZoneInfo.GetSystemTimeZones())
            {
                Timezone.Items.Add(new ListItem(timezone.DisplayName, timezone.Id));
            }
        }
    }
}