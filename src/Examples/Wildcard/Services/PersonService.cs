using System.Collections.Generic;
using System.Linq;

using Examples.Areas.Wildcard.Models;
using Examples.Wildcard.Services.Interfaces;

using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch.SearchTypes;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace Examples.Wildcard.Services
{
    public class PersonService : IPersonService
    {
        private static readonly ID _PersonFolderId = new ID(Sitecore.Configuration.Settings.GetSetting("Wildcard.PersonFolderId"));

        private readonly ISearchIndex _index;

        public PersonService()
        {
            Item root = Sitecore.Context.Database.GetItem(_PersonFolderId);
            _index = ContentSearchManager.GetIndex(new SitecoreIndexableItem(root));
        }

        public IEnumerable<Person> GetAll(out int totalResults, int page = 0, int pageSize = 20)
        {
            SearchResults<SearchResultItem> results;
            using (IProviderSearchContext searchContext = _index.CreateSearchContext())
            {
                results = GetBaseQueryable<SearchResultItem>(searchContext)
                    .OrderBy(i => i.Name)
                    .Page(page, pageSize)
                    .GetResults();
            }

            totalResults = results.TotalSearchResults;
            return AsPersons(results);
        }

        public IEnumerable<Person> GetByName(string name)
        {
            SearchResults<SearchResultItem> results;
            using (IProviderSearchContext searchContext = _index.CreateSearchContext())
            {
                results = GetBaseQueryable<SearchResultItem>(searchContext)
                    .Where(i => i.Name.Equals(name))
                    .OrderBy(i => i.Name)
                    .GetResults();
            }

            return AsPersons(results);
        }

        private static IQueryable<T> GetBaseQueryable<T>(IProviderSearchContext searchContext) where T : SearchResultItem
        {
            return searchContext.GetQueryable<T>(new CultureExecutionContext(Sitecore.Context.Culture)).Where(
                i => i.Paths.Contains(_PersonFolderId) && i.TemplateId == Templates.Person.TemplateId);
        }

        private static IEnumerable<Person> AsPersons(SearchResults<SearchResultItem> results)
        {
            return results.Select(r => r.Document.GetItem()).Where(i => i != null).Select(i => new Person(i));
        }
    }
}