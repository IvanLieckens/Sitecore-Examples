using Examples.Wildcard;

using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Links;
using Sitecore.Text;

namespace Examples.Areas.Wildcard.Models
{
    public class Person : CustomItemBase
    {
        public Person(Item innerItem)
            : base(innerItem)
        {
        }

        public string FirstName => InnerItem[Templates.Person.Fields.PersonFirstName];

        public string LastName => InnerItem[Templates.Person.Fields.PersonLastName];

        public string Phone => InnerItem[Templates.Person.Fields.PersonPhone];

        public string Email => InnerItem[Templates.Person.Fields.PersonEmail];

        public UrlString GetUrl(Item wildcardItem)
        {
            string wildcardUrl = LinkManager.GetItemUrl(wildcardItem);
            return new UrlString(wildcardUrl.Replace(MainUtil.EncodeName("*"), MainUtil.EncodeName(Name)));
        }
    }
}