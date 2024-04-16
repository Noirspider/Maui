using Maui.Data;
using Maui.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace Maui.Controllers
{
    [Authorize]
    public class UserOrderController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UserOrderController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> UserOrderHistory()
        {
            int id = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var ordini = await _db
                .Ordini.Include(o => o.Utente)
                .Include(o => o.ProdottiAcquistati)
                .ThenInclude(p => p.Prodotto)
                .Where(o => o.IdUtente == id)
                .ToListAsync();

            return View(ordini);
        }

        public IActionResult Index()
        {
            var carrello = HttpContext.Session.GetString("Carrello");
            if (!string.IsNullOrEmpty(carrello))
            {
                var carrelloList = JsonConvert.DeserializeObject<List<CartItem>>(carrello);
                return View(carrelloList);
            }

            return View(new List<CartItem>());
        }

        public async Task<IActionResult> FetchListaProdotti()
        {
            var listaProdotti = await _db
                .Prodotto
                .Select(p => new
                {
                    p.IdProdotto,
                    p.ImgProdotto,
                    p.NomeProdotto,
                    p.PrezzoProdotto,
                    p.Stile,
                    p.Birrificio
                })
                .ToListAsync();
            return Json(listaProdotti);
        }

        public async Task<IActionResult> FetchAddToCartSession(int id, int quantity)
        {
            var prodotto = await _db
                .Prodotto
                .Select(p => new
                {
                    p.IdProdotto,
                    p.ImgProdotto,
                    p.NomeProdotto,
                    PrezzoProdotto = (decimal)p.PrezzoProdotto,
                    p.Stile,
                    p.Birrificio,
                    p.QuantitaProdotto
                })
                .FirstOrDefaultAsync(p => p.IdProdotto == id);

            if (prodotto == null)
            {
                return NotFound();
            }

            var carrello = HttpContext.Session.GetString("Carrello");
            var carrelloList = string.IsNullOrEmpty(carrello) ? new List<CartItem>() : JsonConvert.DeserializeObject<List<CartItem>>(carrello) ?? new List<CartItem>();
            var existingItem = carrelloList.FirstOrDefault(i => i.IdProdotto == id);
            if (existingItem != null)
            {
                if (existingItem.Quantita + quantity > prodotto.QuantitaProdotto)
                {
                    return BadRequest("Non ci sono abbastanza birre in magazzino.");
                }
                existingItem.Quantita += quantity;
            }
            else
            {
                if (prodotto.QuantitaProdotto < 1)
                {
                    return BadRequest("Non ci sono abbastanza birre in magazzino.");
                }
                CartItem cartItem = new CartItem
                {
                    IdProdotto = prodotto.IdProdotto,
                    ImgProdotto = prodotto.ImgProdotto,
                    NomeProdotto = prodotto.NomeProdotto,
                    PrezzoProdotto = prodotto.PrezzoProdotto,
                    Stile = prodotto.Stile,
                    Birrificio = prodotto.Birrificio,
                    Quantita = quantity
                };

                carrelloList.Add(cartItem);

            }
            string jsonCart = JsonConvert.SerializeObject(carrelloList);
            HttpContext.Session.SetString("Carrello", jsonCart);

            System.Diagnostics.Debug.WriteLine(HttpContext.Session.GetString("Carrello"));

            return Ok();
        }

        public IActionResult FetchRemoveFromCartSession(int id)
        {
            var carrello = HttpContext.Session.GetString("Carrello");
            if (string.IsNullOrEmpty(carrello))
            {
                return NotFound();
            }

            var carrelloList = JsonConvert.DeserializeObject<List<CartItem>>(carrello);
            var existingItem = carrelloList.FirstOrDefault(i => i.IdProdotto == id);
            if (existingItem != null)
            {
                if (existingItem.Quantita > 1)
                {
                    existingItem.Quantita--;
                }
                else
                {
                    carrelloList.Remove(existingItem);
                }
            }
            string jsonCart = JsonConvert.SerializeObject(carrelloList);
            HttpContext.Session.SetString("Carrello", jsonCart);

            return Ok();
        }

        public IActionResult RiepilogoOrdine()
        {
            var carrello = HttpContext.Session.GetString("Carrello");
            if (!string.IsNullOrEmpty(carrello))
            {
                var carrelloList = JsonConvert.DeserializeObject<List<CartItem>>(carrello);
                ViewBag.Carrello = carrelloList;
                return View();
            }

            return View("index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RiepilogoOrdine(
            [Bind("IdUtente,IndirizzoDiConsegna,DataOrdine,Nota")] Ordine ordine
        )
        {
            ModelState.Remove("Utente");
            ModelState.Remove("ProdottiAcquistati");

            if (ModelState.IsValid)
            {
                _db.Ordini.Add(ordine);
                await _db.SaveChangesAsync();

                List<CartItem> carrello = JsonConvert.DeserializeObject<List<CartItem>>(
                    HttpContext.Session.GetString("Carrello")
                );
                foreach (var item in carrello)
                {
                    ProdottoAcquistato prodottoAcquistato = new ProdottoAcquistato
                    {
                        IdOrdine = ordine.IdOrdine,
                        IdProdotto = item.IdProdotto,
                        Quantita = item.Quantita
                    };
                    _db.ProdottiAcquistati.Add(prodottoAcquistato);
                }

                TempData["Success"] = "Ordine creato con successo";
                await _db.SaveChangesAsync();

                HttpContext.Session.Remove("Carrello");

                return RedirectToAction(
                    "DettagliOrdine",
                    "UserOrder",
                    new { id = ordine.IdOrdine }
                );
            }
            TempData["Error"] = "Errore durante la creazione dell'ordine";
            return View(ordine);
        }

        public async Task<IActionResult> DettagliOrdine(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordine = await _db
                .Ordini.Include(o => o.Utente)
                .Include(o => o.ProdottiAcquistati)
                .ThenInclude(p => p.Prodotto)
                .FirstOrDefaultAsync(m => m.IdOrdine == id);

            if (ordine == null)
            {
                return NotFound();
            }

            var totale = 0.0m;

            foreach (var prodotto in ordine.ProdottiAcquistati)
            {
                totale += prodotto.Prodotto.PrezzoProdotto * prodotto.Quantita;
            }

            ordine.PrezzoTotale = totale;

            await _db.SaveChangesAsync();

            return View(ordine);
        }
        public IActionResult Cart()
        {
            var carrello = HttpContext.Session.GetString("Carrello");
            if (!string.IsNullOrEmpty(carrello))
            {
                var carrelloList = JsonConvert.DeserializeObject<List<CartItem>>(carrello);
                return View(carrelloList);
            }

            return View(new List<CartItem>());
        }
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int idProdotto, int quantity)
        {
            var prodotto = await _db
                .Prodotto
                .FirstOrDefaultAsync(p => p.IdProdotto == idProdotto);

            if (prodotto == null)
            {
                return NotFound();
            }

            if (quantity > prodotto.QuantitaProdotto)
            {
                return BadRequest("Non ci sono abbastanza birre in magazzino.");
            }

            var carrello = HttpContext.Session.GetString("Carrello");
            if (!string.IsNullOrEmpty(carrello))
            {
                var carrelloList = JsonConvert.DeserializeObject<List<CartItem>>(carrello);
                var item = carrelloList.FirstOrDefault(i => i.IdProdotto == idProdotto);
                if (item != null)
                {
                    item.Quantita = quantity;
                    HttpContext.Session.SetString("Carrello", JsonConvert.SerializeObject(carrelloList));
                }
            }

            return Ok();
        }
        [HttpPost]
        public IActionResult RemoveFromCart(int idProdotto)
        {
            var carrello = HttpContext.Session.GetString("Carrello");
            if (!string.IsNullOrEmpty(carrello))
            {
                var carrelloList = JsonConvert.DeserializeObject<List<CartItem>>(carrello);
                var item = carrelloList.FirstOrDefault(i => i.IdProdotto == idProdotto);
                if (item != null)
                {
                    carrelloList.Remove(item);
                    HttpContext.Session.SetString("Carrello", JsonConvert.SerializeObject(carrelloList));
                }
            }

            return Ok();
        }

        public async Task<IActionResult> Checkout()
        {
            var carrello = HttpContext.Session.GetString("Carrello");
            if (string.IsNullOrEmpty(carrello))
            {
                return RedirectToAction("Index");
            }

            var carrelloList = JsonConvert.DeserializeObject<List<CartItem>>(carrello);
            var totale = 0.0m;
            foreach (var item in carrelloList)
            {
                totale += item.PrezzoProdotto * item.Quantita;
            }

            ViewBag.Totale = totale;

            return View();
        }
        public async Task<IActionResult> Details(int id)
        {
            var prodotto = await _db.Prodotto.FirstOrDefaultAsync(p => p.IdProdotto == id);
            if (prodotto == null)
            {
                return NotFound();
            }

            return View(prodotto);
        }


    }
}
