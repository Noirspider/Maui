using Maui.Class;
using Maui.Data;
using Maui.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Maui.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult FetchPage()
        {
            return View();
        }

        [Authorize(Roles = UserRole.ADMIN)]
        public IActionResult BackOffice()
        {
            return View();
        }

        [Authorize(Roles = UserRole.ADMIN)]
        [HttpPost]
        public async Task<IActionResult> FetchNumeroTotaleOrdiniEvasi(
            int giorno,
            int mese,
            int anno
        )
        {
            DateOnly dateFormatted = new DateOnly(anno, mese, giorno);

            var numeroOrdiniEvasi = await _db
                .Ordini.Where(o =>
                    o.IsEvaso == true && DateOnly.FromDateTime(o.DataOrdine) == dateFormatted
                )
                .CountAsync();
            return Json(numeroOrdiniEvasi);
        }

        [Authorize(Roles = UserRole.ADMIN)]
        [HttpPost]
        public async Task<IActionResult> FetchTotaleIncassoInData(int giorno, int mese, int anno)
        {
            DateOnly dateFormatted = new DateOnly(anno, mese, giorno);

            var listaOrdiniEvasi = await _db
                .Ordini.Include(o => o.ProdottiAcquistati)
                .ThenInclude(pa => pa.Prodotto)
                .Where(o =>
                    o.IsEvaso == true && DateOnly.FromDateTime(o.DataOrdine) == dateFormatted
                )
                .ToListAsync();

            decimal totaleIncasso = 0;
            foreach (var ordine in listaOrdiniEvasi)
            {
                foreach (var prod in ordine.ProdottiAcquistati)
                {
                    totaleIncasso += prod.Prodotto.PrezzoProdotto;
                }
            }

            return Json(totaleIncasso);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                }
            );
        }
    }
}
