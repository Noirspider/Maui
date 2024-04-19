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

        // Metodo per visualizzare la cronologia degli ordini dell'utente
        public async Task<IActionResult> UserOrderHistory()
        {
            int id = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var ordine = await _db
                .Ordine.Include(o => o.Utente)
                .Include(o => o.ProdottoAcquistato)
                .ThenInclude(p => p.Prodotto)
                .Where(o => o.IdUtente == id)
                .ToListAsync();

            return View(ordine);
        }

        // Metodo per visualizzare la pagina principale
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

        // Metodo per ottenere la lista dei prodotti
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

        // Metodo per aggiungere un prodotto al carrello
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
                    QuantitaProdotto = prodotto.QuantitaProdotto,
                    Quantita = quantity
                };

                carrelloList.Add(cartItem);

            }
            string jsonCart = JsonConvert.SerializeObject(carrelloList);
            HttpContext.Session.SetString("Carrello", jsonCart);

            System.Diagnostics.Debug.WriteLine(HttpContext.Session.GetString("Carrello"));

            return Ok();
        }

        // Metodo per rimuovere un prodotto dal carrello
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

        // Metodo per creare un nuovo ordine
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RiepilogoOrdine(
           [Bind("IdUtente,IndirizzoDiConsegna,DataOrdine,Nota")] Ordine ordine
       )
        {
            // Ottieni l'IdUtente dall'utente attualmente autenticato
            ordine.IdUtente = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            // Ottieni l'ID dell'utente
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Ottieni il totale dal cookie, vincolato all'ID dell'utente
            Request.Cookies.TryGetValue("Totale_" + userId, out string totaleString);
            decimal totale = decimal.Parse(totaleString ?? "0.0");
            ModelState.Remove("Utente");
            ModelState.Remove("ProdottiAcquistati");

            if (ModelState.IsValid)
            {
                _db.Ordine.Add(ordine);
                await _db.SaveChangesAsync();

                List<CartItem> carrello = JsonConvert.DeserializeObject<List<CartItem>>(
                    HttpContext.Session.GetString("Carrello") ?? "[]"
                );
                foreach (var item in carrello)
                {
                    ProdottoAcquistato prodottoAcquistato = new ProdottoAcquistato
                    {
                        IdOrdine = ordine.IdOrdine,
                        IdProdotto = item.IdProdotto,
                        Quantita = item.Quantita,
                    };
                    _db.ProdottoAcquistato.Add(prodottoAcquistato);
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

        // Metodo per visualizzare i dettagli di un ordine
        public async Task<IActionResult> DettagliOrdine(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordine = await _db
                .Ordine.Include(o => o.Utente)
                .Include(o => o.ProdottoAcquistato)
                .ThenInclude(p => p.Prodotto)
                .FirstOrDefaultAsync(m => m.IdOrdine == id);

            if (ordine == null)
            {
                return NotFound();
            }

            var totale = 0.0m;

            foreach (var prodotto in ordine.ProdottoAcquistato)
            {
                totale += prodotto.Prodotto.PrezzoProdotto * prodotto.Quantita;
            }

            ordine.PrezzoTotale = totale;

            await _db.SaveChangesAsync();

            return View(ordine);
        }

        // Metodo per visualizzare il carrello
        public IActionResult Cart()
        {
            var carrello = HttpContext.Session.GetString("Carrello");
            if (!string.IsNullOrEmpty(carrello))
            {
                // Deserializza il carrello
                var carrelloList = JsonConvert.DeserializeObject<List<CartItem>>(carrello);
                
                // Calcola il totale del carrello
                var totale = 0.0m;
                foreach (var item in carrelloList)
                {
                    totale += item.PrezzoProdotto * item.Quantita;
                }

                // Ottieni l'ID dell'utente
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Imposta un cookie con il totale, vincolato all'ID dell'utente
                Response.Cookies.Append("Totale_" + userId, totale.ToString());

                // Imposta un cookie con il totale
                Response.Cookies.Append("Totale", totale.ToString());
                return View(carrelloList);
            }

            return View(new List<CartItem>());
        }

        // Metodo per aggiornare la quantità di un prodotto nel carrello
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

        // Metodo per rimuovere un prodotto dal carrello
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

        // Metodo per effettuare il checkout
        public IActionResult Checkout()
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

        // Metodo per visualizzare i dettagli di un prodotto
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
