using System.Collections.Generic;

using Examples.Areas.Wildcard.Models.Shared;

using Sitecore.Data.Items;

namespace Examples.Areas.Wildcard.Models.People
{
    public class PeopleOverviewViewModel
    {
        public IEnumerable<Person> Persons { get; set; }

        public PagingViewModel Paging { get; set; }

        public Item WildcardItem { get; set; }
    }
}