using System.Web;

using Sitecore.Diagnostics;
using Sitecore.Rules;
using Sitecore.Rules.Conditions;

namespace Examples.Rules.Request
{
    public class QuerystringCondition<T> : StringOperatorCondition<T> where T : RuleContext
    {
        public string Key { get; set; }

        public string Value { get; set; }

        protected override bool Execute(T ruleContext)
        {
            bool result = false;
            HttpContext httpContext = HttpContext.Current;
            if (httpContext?.Request != null && !string.IsNullOrWhiteSpace(Key) && !string.IsNullOrWhiteSpace(Value))
            {
                string querystringValue = httpContext.Request.QueryString[Key];
                result = Compare(querystringValue, Value);
            }
            else
            {
                Log.Warn("QuerystringCondition rule executed without HttpContext, Key or Value", this);
            }

            return result;
        }
    }
}