using System;

using Examples.ContentEditorCulture.Utils;

using Sitecore;
using Sitecore.Xml.Xsl;

namespace Examples.ContentEditorCulture.Xml.Xsl
{
    public class DateRenderer : Sitecore.Xml.Xsl.DateRenderer
    {
        public override RenderFieldResult Render()
        {
            if (string.IsNullOrEmpty(FieldValue))
            {
                return new RenderFieldResult();
            }

            DateTime dateTime = DateUtil.IsoDateToDateTime(FieldValue, DateTime.MinValue);
            if (dateTime == DateTime.MinValue)
            {
                return new RenderFieldResult();
            }

            string parameter = Parameters["format"];
            return new RenderFieldResult(FormatDate(TimeZoneUtil.ToUserTime(dateTime), parameter));
        }
    }
}