using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebAppEnvios.Models;

namespace WebAppEnvios.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> HacermeAdmin()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                if (!await _userManager.IsInRoleAsync(user, "Administrador"))
                {
                    await _userManager.AddToRoleAsync(user, "Administrador");
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
