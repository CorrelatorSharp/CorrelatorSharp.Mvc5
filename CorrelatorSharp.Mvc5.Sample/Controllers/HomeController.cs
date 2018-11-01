using System.Web.Mvc;

namespace CorrelatorSharp.Mvc5.Sample.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index() 
            => View();

        public ActionResult Test() 
            => Json(ActivityScope.Current);
    }
}