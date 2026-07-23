using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Front_TalleresSeta.Controllers
{
    public class PrincipalController : Controller
    {
        //private readonly ILogger<PrincipalController> _logger;


        //public PrincipalController(ILogger<PrincipalController> logger)
        //{
        //    _logger = logger;
        //}

        public IActionResult Index()
        {
            return RedirectToAction("Acceso", "Login");
        }

        //public IActionResult Login()
        //{
        //    return View();
        //}


        public async Task<IActionResult> Logout()
        {
            // Eliminar las cookies
            Response.Cookies.Delete("CookieName1");
            Response.Cookies.Delete("CookieName2");

            await HttpContext.SignOutAsync();
            return RedirectToAction("Acceso", "Login");
            //return Redirect("/Principal/Index");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}
    }
}