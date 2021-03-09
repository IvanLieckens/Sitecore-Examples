using System.Linq;
using System.Web.ModelBinding;
using System.Web.Mvc;

using Examples.Areas.Wildcard.Models.People;
using Examples.Areas.Wildcard.Models.Shared;
using Examples.Wildcard.Services.Interfaces;

using Sitecore.Data.Items;
using Sitecore.Mvc.Presentation;

namespace Examples.Areas.Wildcard.Controllers
{
    public class PeopleController : Controller
    {
        private readonly IPersonService _personService;

        public PeopleController(IPersonService personService)
        {
            _personService = personService;
        }

        public ActionResult Overview([QueryString] int page = 1)
        {
            --page;
            PeopleOverviewViewModel model = new PeopleOverviewViewModel();
            model.Persons = _personService.GetAll(out int total, page);
            model.WildcardItem = GetPersonWildcardItem();
            model.Paging = new PagingViewModel(page, total);
            return View(model);
        }

        private Item GetPersonWildcardItem()
        {
            return RenderingContext.Current.PageContext.Item.Children.SingleOrDefault(i => i.Name.Equals("*"));
        }
    }
}