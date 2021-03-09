using System.Linq;
using System.Web;
using System.Web.Mvc;

using Examples.Areas.Wildcard.Models;
using Examples.Wildcard;
using Examples.Wildcard.Services.Interfaces;

using Sitecore;
using Sitecore.Mvc.Presentation;

namespace Examples.Areas.Wildcard.Controllers
{
    public class PersonController : Controller
    {
        private readonly IPersonService _personService;

        public PersonController(IPersonService personService)
        {
            _personService = personService;
        }

        public ActionResult Data()
        {
            Person model = GetModel();
            return View(model);
        }

        public ActionResult Contact()
        {
            Person model = GetModel();
            return View(model);
        }

        private Person GetModel()
        {
            Person result = null;
            if (RenderingContext.Current.PageContext.Item.Name.Equals("*"))
            {
                string name = HttpUtility.UrlDecode(
                    RenderingContext.Current.PageContext.RequestContext.HttpContext.Request.Url?.Segments
                        .LastOrDefault());
                result = _personService.GetByName(name).FirstOrDefault();
            }
            else if (RenderingContext.Current.Rendering.Item.TemplateID == Templates.Person.TemplateId)
            {
                result = new Person(RenderingContext.Current.Rendering.Item);
            }

            return result;
        }
    }
}