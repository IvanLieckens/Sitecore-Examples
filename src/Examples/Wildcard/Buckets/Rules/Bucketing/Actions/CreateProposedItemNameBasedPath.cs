using System.Collections.Generic;
using System.Linq;

using Sitecore.Buckets.Rules.Bucketing;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rules.Actions;

namespace Examples.Wildcard.Buckets.Rules.Bucketing.Actions
{
    public class CreateProposedItemNameBasedPath<T> : RuleAction<T> where T : BucketingRuleContext
    {
        public string Levels { get; set; }

        public override void Apply(T ruleContext)
        {
            Assert.ArgumentNotNull(ruleContext, nameof(ruleContext));
            string newItemName = ruleContext.NewItemName;
            int levels = newItemName.Length;
            if (int.TryParse(Levels, out int parsed))
            {
                levels = parsed;
            }

            char[] chars = newItemName.Length > levels ? newItemName.Substring(0, levels).ToCharArray() : newItemName.ToCharArray();
            List<string> path = new List<string>(chars.Length);
            path.AddRange(chars.Select(c => ItemUtil.ProposeValidItemName(c.ToString(), "_")));
            ruleContext.ResolvedPath = string.Join(Sitecore.Buckets.Util.Constants.ContentPathSeperator, path).ToLowerInvariant();
        }
    }
}