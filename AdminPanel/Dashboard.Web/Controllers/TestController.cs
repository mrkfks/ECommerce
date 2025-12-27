using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dashboard.Web.Controllers
{
    public class TestController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            var authToken = Request.Cookies["AuthToken"];
            var jwtToken = Request.Cookies["JwtToken"];
            var refreshToken = Request.Cookies["RefreshToken"];
            
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var userName = User.Identity?.Name ?? "N/A";
            var claims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
            
            ViewBag.AuthToken = authToken ?? "NULL";
            ViewBag.JwtToken = jwtToken ?? "NULL";
            ViewBag.RefreshToken = refreshToken ?? "NULL";
            ViewBag.IsAuthenticated = isAuthenticated;
            ViewBag.UserName = userName;
            ViewBag.Claims = claims;
            
            return View();
        }
    }
}
