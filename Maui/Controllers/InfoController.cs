using Microsoft.AspNetCore.Mvc;

namespace Maui.Controllers
{
    public class InfoController : Controller
    {
        // GET: /<controller>/
        public IActionResult Contatti()
        {
            return View();
        }
        public IActionResult ChiSiamo()
        {
            return View();
        }
        public IActionResult Me()
        {
            return View();
        }
    }
}
