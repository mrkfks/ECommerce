using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult DebugClaims()
        {
            if (User?.Claims == null) return Content("User claims are null");
            
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"IsAuthenticated: {User.Identity?.IsAuthenticated}");
            sb.AppendLine($"Name: {User.Identity?.Name}");
            sb.AppendLine("Claims:");
            foreach (var claim in User.Claims)
            {
                sb.AppendLine($"- {claim.Type}: {claim.Value}");
            }
            return Content(sb.ToString());
        }
    }
}
