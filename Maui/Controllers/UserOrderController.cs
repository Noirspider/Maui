using Maui.Data;
using Maui.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Security.Claims;




namespace Maui.Controllers
{
    // Definizione della classe UserOrderController
    [Authorize]
    // Definizione della classe UserOrderController
    public class UserOrderController : Controller
    {// Dichiarazione del database
        private readonly ApplicationDbContext _db;
        // Costruttore
        public UserOrderController(ApplicationDbContext db)
        {
            // Inizializza il database
            _db = db;
        }








        /////////////////////////////////////////////////////////////////

// Metodo per visualizzare i dettagli di un ordine
        public async Task<IActionResult> Details(int? id)
        {
            // Se l'ID è nullo, restituisci un errore
            if (id == null)
            {
                return NotFound();
            }
            // Ottieni l'ordine dal database
            var product = await _db.Prodotto
                .FirstOrDefaultAsync(m => m.IdProdotto == id);
            if (product == null)
            {
                return NotFound();
            }
            // Restituisci la vista con il prodotto
            return View(product);
        }




        /// ////////////////////////////

        // Metodo per visualizzare la pagina principale
        public IActionResult Index()
        {
            // Ottieni il carrello dalla sessione
            //var carrello = HttpContext.Session.GetString("Carrello");
            //// Se il carrello non è vuoto
            //if (!string.IsNullOrEmpty(carrello))
            //{
            //    // Deserializza il carrello
            //    var carrelloList = JsonConvert.DeserializeObject<List<CartItem>>(carrello);
            //    // Calcola il totale del carrello
            //    return View(carrelloList);
            //}
            //// Se il carrello è vuoto, restituisci una lista vuota
            //return View(new List<CartItem>());
            return View();
        }






        /////////////////////////////////////////////////////////////////








        // Metodo per ottenere la lista dei prodotti
        public async Task<IActionResult> FetchListaProdotti()
        {
            // Ottieni la lista dei prodotti dal database
            var listaProdotti = await _db
                // Seleziona la tabella dei prodotti
                .Prodotto
                // Seleziona i campi necessari
                .Select(p => new
                {
                    p.IdProdotto,
                    p.ImgProdotto,
                    p.NomeProdotto,
                    p.PrezzoProdotto,
                    p.Stile,
                    p.Birrificio
                })
                // Ottieni la lista dei prodotti dal database
                .ToListAsync();
            return Json(listaProdotti);
        }




        /////////////////////////////////////////////////////////////////






       





        /////////////////////////////////////////////////////////////////









        // Metodo per creare un nuovo ordine
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RiepilogoOrdine(
      [Bind("IdUtente,IndirizzoDiConsegna,DataOrdine")] Ordine ordine
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
            ModelState.Remove("Nota");
            ModelState.Remove("ProdottoAcquistato");

            // Se il modello non è valido, restituisci un errore
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    System.Diagnostics.Debug.WriteLine(error.ErrorMessage);
                }
            }

            // Imposta il totale dell'ordine
            // Imposta il totale dell'ordine
            if (ModelState.IsValid)         // verifica ex composizione
            {
                // Crea un nuovo ordine
                _db.Ordine  .Add(ordine);
                await _db.SaveChangesAsync();

                // Deserializza il carrello
                List<CartItem> carrello = JsonConvert.DeserializeObject<List<CartItem>>(
                    HttpContext.Session.GetString("Carrello") ?? "[]"
                );

                // Se il carrello non è vuoto
                if (carrello.Count > 0)
                {
                    // Per ogni prodotto nel carrello
                    foreach (var item in carrello)
                    {
                        // Ottieni il prodotto dal database
                        ProdottoAcquistato prodottoAcquistato = new ProdottoAcquistato
                        {
                            IdOrdine = ordine.IdOrdine,
                            IdProdotto = item.IdProdotto,
                            Quantita = item.Quantita,
                        };
                        _db.ProdottoAcquistato.Add(prodottoAcquistato);
                    }
                    // Aggiorna il totale dell'ordine
                    TempData["Success"] = "Ordine creato con successo";
                    await _db.SaveChangesAsync();
                    // Rimuovi il carrello dalla sessione
                    HttpContext.Session.Remove("Carrello");
                    // Rimuovi il cookie con il totale
                  // return RedirectToAction("DettagliOrdine", "UserOrder", new { id = ordine.IdOrdine });
                    return RedirectToAction("OrdineCompletato", "UserOrder");
                }
                else
                {
                    // Gestisci il caso in cui il carrello è vuoto
                    TempData["Error"] = "Il carrello è vuoto";
                }
            }
            return View(ordine);
        }




        /// //////////////////////////////////////////////

        public IActionResult OrdineCompletato()
        {
            // Restituisce la vista OrdineCompletato
            return View();
        }





        /////////////////////////////////////////////////////////////////







        // Metodo per visualizzare i dettagli di un ordine
        [HttpGet]
        public async Task<IActionResult> DettagliOrdine(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            // Ottieni l'ordine dal database
            var ordine = await _db                           // qui c'è l'errore 
               // Seleziona la tabella degli ordini
                .Ordine.Include(o => o.Utente)
                // Seleziona i campi necessari
                .Include(o => o.ProdottoAcquistato)
                .ThenInclude(p => p.Prodotto)
                .FirstOrDefaultAsync(m => m.IdOrdine == id);
            // Se l'ordine non esiste restituisci un errore
            if (ordine == null)
            {
                return NotFound();
            }

            var totale = 0.0m;

            foreach (var prodotto in ordine.ProdottoAcquistato)
            {
                if (prodotto.Prodotto != null && prodotto.Prodotto.PrezzoProdotto != null)
                {
                    totale += prodotto.Prodotto.PrezzoProdotto * prodotto.Quantita;
                }
            }
            // Aggiorna il totale dell'ordine
            ordine.PrezzoTotale = totale;
            // Salva le modifiche al database
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {   
                Console.WriteLine(ex.Message);
            }
                
                // Restituisci la vista con l'ordine
            return View(ordine);
        }



        /////////////////////////////////////////////////////////////////

        [HttpGet]
        public async Task<IActionResult> UserOrderHistory()
        {
            try
            {
                var id = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                    var utente = await _db.Utente.FindAsync(id);
                    if (utente == null)
                    {
                        Console.WriteLine("L'utente non esiste nel database.");
                        return View("Error");
                    }

                    var ordini = await _db.Utente.Include(i=>i.Ordine).Where(o => o.IdUtente == id).ToListAsync();
                    if (ordini == null)
                    {
                        Console.WriteLine("Non ci sono ordini per questo utente.");
                        return View("Error");
                    }

                    var ordiniConDettagli = await _db.Ordine
                        .Include(o => o.Utente)
                        .Include(o => o.ProdottoAcquistato)
                        .ThenInclude(p => p.Prodotto)
                        .Where(o => o.IdUtente == id)
                        .ToListAsync();

                    return View(ordiniConDettagli);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ultimo errore:" + ex.Message);
                return View("Error");
            }
        }





        //CARRELLO 



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
                // Per ogni prodotto nel carrello
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
            // Ottieni il prodotto dal database
            var prodotto = await _db
                .Prodotto
                .FirstOrDefaultAsync(p => p.IdProdotto == idProdotto);
            // Se il prodotto non esiste, restituisci un errore
            if (prodotto == null)
            {
                return NotFound();
            }
            // Se la quantità richiesta è maggiore della quantità disponibile
            if (quantity > prodotto.QuantitaProdotto)
            {
                return BadRequest("Non ci sono abbastanza birre in magazzino.");
            }
            // Ottieni il carrello dalla sessione
            var carrello = HttpContext.Session.GetString("Carrello");
            // Se il carrello è vuoto, restituisci un errore
            if (!string.IsNullOrEmpty(carrello))
            {
                //  Deserializza il carrello
                var carrelloList = JsonConvert.DeserializeObject<List<CartItem>>(carrello);
                // Ottieni il prodotto dal carrello
                var item = carrelloList.FirstOrDefault(i => i.IdProdotto == idProdotto);
                // Se il prodotto è nel carrello
                if (item != null)
                {
                    // Aggiorna la quantità del prodotto
                    item.Quantita = quantity;
                    // Serializza il carrello
                    HttpContext.Session.SetString("Carrello", JsonConvert.SerializeObject(carrelloList));
                }
            }
            // Restituisci un messaggio di successo
            return Ok();
        }



        // Metodo per aggiungere un prodotto al carrello
        public async Task<IActionResult> FetchAddToCartSession(int id, int quantity)
        {
            // Ottieni il prodotto dal database
            var prodotto = await _db
                // Seleziona la tabella dei prodotti
                .Prodotto
                // Seleziona i campi necessari
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
                // Ottieni il prodotto dal database
                .FirstOrDefaultAsync(p => p.IdProdotto == id);
            // Se il prodotto non esiste, restituisci un errore
            if (prodotto == null)
            {
                // Se il prodotto non esiste, restituisci un errore
                return NotFound();
            }
            // Ottieni il carrello dalla sessione
            var carrello = HttpContext.Session.GetString("Carrello");
            // Deserializza il carrello
            var carrelloList = string.IsNullOrEmpty(carrello) ? new List<CartItem>() : JsonConvert.DeserializeObject<List<CartItem>>(carrello) ?? new List<CartItem>();
            // Ottieni il prodotto dal carrello
            var existingItem = carrelloList.FirstOrDefault(i => i.IdProdotto == id);
            // Se il prodotto è già nel carrello
            if (existingItem != null)
            {
                // Se la quantità richiesta è maggiore della quantità disponibile
                if (existingItem.Quantita + quantity > prodotto.QuantitaProdotto)
                {
                    return BadRequest("Non ci sono abbastanza birre in magazzino.");
                }
                existingItem.Quantita += quantity;
            }
            else
            {
                // Se la quantità richiesta è maggiore della quantità disponibile
                if (prodotto.QuantitaProdotto < 1)
                {
                    return BadRequest("Non ci sono abbastanza birre in magazzino.");
                }
                // Aggiungi il prodotto al carrello
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
            // Serializza il carrello
            string jsonCart = JsonConvert.SerializeObject(carrelloList);
            HttpContext.Session.SetString("Carrello", jsonCart);

            System.Diagnostics.Debug.WriteLine(HttpContext.Session.GetString("Carrello"));
            // Restituisci un messaggio di successo
            return Ok();
        }







        // Metodo per rimuovere un prodotto dal carrello
        public IActionResult FetchRemoveFromCartSession(int id)
        {
            // Ottieni il carrello dalla sessione
            var carrello = HttpContext.Session.GetString("Carrello");
            // Se il carrello è vuoto, restituisci un errore
            if (string.IsNullOrEmpty(carrello))
            {
                return NotFound();
            }
            // Deserializza il carrello
            var carrelloList = JsonConvert.DeserializeObject<List<CartItem>>(carrello);
            var existingItem = carrelloList.FirstOrDefault(i => i.IdProdotto == id);
            if (existingItem != null)
            {
                // Se la quantità è maggiore di 1, decrementa la quantità
                if (existingItem.Quantita > 1)
                {
                    existingItem.Quantita--;
                }
                else
                {
                    carrelloList.Remove(existingItem);
                }
            }
            // Serializza il carrello
            string jsonCart = JsonConvert.SerializeObject(carrelloList);
            HttpContext.Session.SetString("Carrello", jsonCart);

            return Ok();
        }







        // Metodo per rimuovere un prodotto dal carrello
        [HttpPost]
        public IActionResult RemoveFromCart(int idProdotto)
        {
            // Ottieni il carrello dalla sessione
            var carrello = HttpContext.Session.GetString("Carrello");
            // Se il carrello è vuoto, restituisci un errore
            if (!string.IsNullOrEmpty(carrello))
            {
                // Deserializza il carrello
                var carrelloList = JsonConvert.DeserializeObject<List<CartItem>>(carrello);
                // Ottieni il prodotto dal carrello
                var item = carrelloList.FirstOrDefault(i => i.IdProdotto == idProdotto);
                // Se il prodotto è nel carrello
                if (item != null)
                {
                    // Rimuovi il prodotto dal carrello
                    carrelloList.Remove(item);
                    // Serializza il carrello
                    HttpContext.Session.SetString("Carrello", JsonConvert.SerializeObject(carrelloList));
                }
            }

            return Ok();
        }
    }
}
