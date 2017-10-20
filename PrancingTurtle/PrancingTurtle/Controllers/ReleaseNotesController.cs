using System.Web.Mvc;

namespace PrancingTurtle.Controllers
{
    public class ReleaseNotesController : Controller
    {
        // GET: ReleaseNotes
        public ActionResult Index()
        {
            // Return the most recent version of the release notes
            return RedirectToAction("V1");
        }

        public ActionResult V1()
        {
            return View();
        }
    }
}