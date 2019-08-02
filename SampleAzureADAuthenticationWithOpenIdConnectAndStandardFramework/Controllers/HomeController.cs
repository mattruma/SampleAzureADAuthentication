using System.Security.Claims;
using System.Web.Mvc;

namespace SampleAzureADAuthenticationWithOpenIdConnectAndStandardFramework.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View((ClaimsPrincipal)this.User);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}