using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

