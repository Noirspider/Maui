using Maui.Data;
using Maui.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Maui.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

        public LoginController(
            ApplicationDbContext db,
            IAuthenticationSchemeProvider authenticationSchemeProvider
        )
        {
            _db = db;
            _authenticationSchemeProvider = authenticationSchemeProvider;
        }

        // Get /Login/Index
        public IActionResult Index()
        {
            return View();
        }

        // Post /Login/Index
        [HttpPost]
        public async Task<IActionResult> Index([Bind("Username,Password")] Utente model)
        {
            ModelState.Remove("Ordini");
            ModelState.Remove("Nome");
            ModelState.Remove("Cognome");
            ModelState.Remove("Email");
            ModelState.Remove("Telefono");


            if (!ModelState.IsValid)
            {
                TempData["error"] = "Errore nei dati inseriti";
                return View();
            }

            var user = await _db.Utente.FirstOrDefaultAsync(u =>
                u.Username == model.Username && u.Password == model.Password
            );

            if (user == null)
            {
                TempData["error"] = "Account non esistente";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.NameIdentifier, user.IdUtente.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var authProperties = new AuthenticationProperties();

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );
            TempData["success"] = "Login effettuato con successo";

            return RedirectToAction("Index", "Home");
        }

        // Get /Login/Logout
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["success"] = "Sei stato disconnesso";

            return RedirectToAction("Index", "Home");
        }

        // Get /Login/SignUp
        public async Task<IActionResult> SignUp()
        {
            return View();
        }

        // Post /Login/SignUp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp([Bind("Username,Password,Nome,Cognome,Email,Telefono")] Utente model)
        {
            ModelState.Remove("Ordini");
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Errore nei dati inseriti";
                return View();
            }

            var user = await _db.Utente.FirstOrDefaultAsync(u => u.Username == model.Username);

            if (user != null)
            {
                TempData["error"] = "Username già esistente";
                return View();
            }

            _db.Utente.Add(model);
            await _db.SaveChangesAsync();

            TempData["success"] = "Registrazione effettuata con successo";

            return RedirectToAction("Index");
        }
    }
}
