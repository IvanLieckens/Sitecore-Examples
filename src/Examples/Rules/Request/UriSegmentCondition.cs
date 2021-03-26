using System.Web;

using Sitecore.Diagnostics;
using Sitecore.Mvc.Extensions;
using Sitecore.Rules;
using Sitecore.Rules.Conditions;

namespace Examples.Rules.Request
{
    public class UriSegmentCondition<T> : StringOperatorCondition<T> where T : RuleContext
    {
        public int Segment { get; set; }

        public string Value { get; set; }

        protected override bool Execute(T ruleContext)
        {
            bool result = false;
            HttpContext httpContext = HttpContext.Current;
            if (Segment > 0 && httpContext?.Request.Url.Segments.Length >= Segment && !string.IsNullOrWhiteSpace(Value))
            {
                // NOTE [ILs] The first segment of a Uri is always "/", this helps mapping between the 0 based
                // indexing of arrays and people starting counting from 1.
                // We are also stripping the / from the end to lessen cognitive burden on the Content Editors
                // needing to remember that all segments end with "/" with potential exception of the last segment.
                string segmentValue = httpContext.Request.Url.Segments[Segment];
                segmentValue = segmentValue.WithoutPostfix("/");
                result = Compare(segmentValue, Value);
            }
            else
            {
                Log.Warn("UriSegmentCondition rule executed without HttpContext, Value or out of bounds segment number", this);
            }

            return result;
        }
    }
}