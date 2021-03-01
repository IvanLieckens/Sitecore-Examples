using Sitecore;

namespace Examples.ContentEditorCulture.Fields
{
    public class DateTime : Date
    {
        public DateTime()
        {
            ShowTime = true;
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

        protected override string GetCurrentDate()
        {
            return DateUtil.IsoNow;
        }
    }
}