using System.Web.Mvc;

namespace PrancingTurtle.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Http404(string url)
        {
            Response.StatusCode = 404;
            return View("Error404");
        }
    }
}