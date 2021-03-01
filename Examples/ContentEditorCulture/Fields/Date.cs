using System;

using Examples.ContentEditorCulture.Utils;

using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;

namespace Examples.ContentEditorCulture.Fields
{
    public class Date : Input, IContentField
    {
        // ReSharper disable once StyleCop.SA1401 - XAML control
        // ReSharper disable once InconsistentNaming - Must match
        protected DateTimePicker picker;

        public Date()
        {
            Initialize();
        }

        public override bool ReadOnly
        {
            get => GetViewStateBool(nameof(ReadOnly));

            set
            {
                SetViewStateBool(nameof(ReadOnly), value);
                if (value)
                {
                    Attributes["readonly"] = "readonly";
                    Disabled = true;
                }
                else
                {
                    Attributes.Remove("readonly");
                }
            }
        }

        // ReSharper disable once InconsistentNaming - Must match
        public string ItemID
        {
            get => GetViewStateString(nameof(ItemID));

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                SetViewStateString(nameof(ItemID), value);
            }
        }

        public string RealValue
        {
            get => GetViewStateString(nameof(RealValue));

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                SetViewStateString(nameof(RealValue), value.StartsWith("$", StringComparison.InvariantCulture) ? value : TimeZoneUtil.IsoDateToUserTimeIsoDate(value));
            }
        }

        public bool ShowTime
        {
            get => GetViewStateBool("Showtime", false);

            set => SetViewStateBool("Showtime", value);
        }

        public bool IsModified
        {
            get => System.Convert.ToBoolean(ServerProperties[nameof(IsModified)]);

            protected set => ServerProperties[nameof(IsModified)] = value;
        }

        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull(message, nameof(message));
            base.HandleMessage(message);
            if (message["id"] != ID)
            {
                return;
            }

            string name = message.Name;
            // ReSharper disable once StringLiteralTypo - Must match
            if (name != "contentdate:today")
            {
                // ReSharper disable once StringLiteralTypo - Must match
                if (name != "contentdate:clear")
                {
                    return;
                }

                ClearField();
            }
            else
            {
                Today();
            }
        }

        public string GetValue()
        {
            string isoDate = picker != null ? IsModified ? picker.Value : RealValue : RealValue;
            string result = !isoDate.StartsWith("$", StringComparison.InvariantCulture) ? TimeZoneUtil.IsoDateToUtcIsoDate(isoDate) : isoDate;
            return result;
        }

        public void SetValue(string value)
        {
            Assert.ArgumentNotNull(value, nameof(value));
            Value = value;
            value = value.StartsWith("$", StringComparison.InvariantCulture) ? value : TimeZoneUtil.IsoDateToUserTimeIsoDate(value);
            RealValue = value;
            if (picker != null)
            {
                picker.Value = value;
            }
        }

        protected override Item GetItem()
        {
            return Client.ContentDatabase.GetItem(ItemID);
        }

        protected override bool LoadPostData(string value)
        {
            bool result = true;
            if (!string.IsNullOrEmpty(value))
            {
                value = TimeZoneUtil.IsoDateToUserTimeIsoDate(value);
            }

            if (!base.LoadPostData(value))
            {
                result = false;
            }

            picker.Value = value ?? string.Empty;
            return result;
        }

        protected override void OnInit(EventArgs e)
        {
            picker = new DateTimePicker { ID = ID + "_picker" };
            Controls.Add(picker);
            if (!string.IsNullOrEmpty(RealValue))
            {
                picker.Value = RealValue;
            }

            picker.Changed += (param1, param2) => SetModified();
            picker.ShowTime = ShowTime;
            picker.Disabled = Disabled;
            base.OnInit(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ServerProperties["Value"] = ServerProperties["Value"];
            ServerProperties["RealValue"] = ServerProperties["RealValue"];
        }

        protected override void SetModified()
        {
            base.SetModified();
            IsModified = true;
            if (TrackModified)
            {
                Sitecore.Context.ClientPage.Modified = true;
            }
        }

        protected virtual string GetCurrentDate()
        {
            return DateUtil.ToIsoDate(TimeZoneUtil.ToUserTime(System.DateTime.UtcNow).Date);
        }

        private void Initialize()
        {
            Class = "scContentControl";
            Change = "#";
            Activation = true;
            ShowTime = false;
        }

        // ReSharper disable once IdentifierTypo - Original
        private void SetRealValue(string realvalue)
        {
            realvalue = TimeZoneUtil.IsoDateToUserTimeIsoDate(realvalue);
            if (realvalue != RealValue)
            {
                SetModified();
            }

            RealValue = realvalue;
            picker.Value = realvalue;
        }

        private void Today()
        {
            SetRealValue(GetCurrentDate());
        }

        private void ClearField()
        {
            SetRealValue(string.Empty);
        }
    }
}