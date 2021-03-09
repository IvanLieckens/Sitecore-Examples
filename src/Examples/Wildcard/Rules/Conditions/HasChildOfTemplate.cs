using System.Linq;

using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rules;
using Sitecore.Rules.Conditions;

namespace Examples.Wildcard.Rules.Conditions
{
    public class HasChildOfTemplate<T> : WhenCondition<T> where T : RuleContext
    {
        private ID _templateId;

        public HasChildOfTemplate()
        {
            _templateId = ID.Null;
        }

        public ID TemplateId
        {
            get => _templateId;
            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                _templateId = value;
            }
        }

        protected override bool Execute(T ruleContext)
        {
            bool result = false;
            Assert.ArgumentNotNull(ruleContext, nameof(ruleContext));
            Item parent = ruleContext.Item;
            if (parent != null)
            {
                result = parent.Children.Any(c => c.TemplateID == _templateId);
            }

            return result;
        }
    }
}