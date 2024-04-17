using Maui.Data;
using Maui.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Maui.Controllers
{
    public class ProdottoesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProdottoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Prodottoes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Prodotto.ToListAsync());
        }

        // GET: Prodottoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prodotto = await _context.Prodotto
                .FirstOrDefaultAsync(m => m.IdProdotto == id);
            if (prodotto == null)
            {
                return NotFound();
            }

            return View(prodotto);
        }

        // GET: Prodottoes/Create
        public IActionResult Create()
        {
            return View();

        }

        // POST: Prodottoes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProdotto,NomeProdotto,ImgProdotto,PrezzoProdotto,QuantitaProdotto,Stile,Descrizione,Volume,Gradazione,Birrificio,Nazione")] Prodotto prodotto)
        {
            ModelState.Remove("ProdottoAcquistato");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(prodotto);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Aggiungi il messaggio di errore al ModelState
                    ModelState.AddModelError(string.Empty, "Si è verificato un errore durante la creazione del prodotto: " + ex.Message);
                }
            }

            // Se siamo arrivati qui, qualcosa è andato storto, mostra di nuovo la vista
            return View(prodotto);
        }

        // GET: Prodottoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prodotto = await _context.Prodotto.FindAsync(id);
            if (prodotto == null)
            {
                return NotFound();
            }
            return View(prodotto);
        }

        // POST: Prodottoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProdotto,NomeProdotto,ImgProdotto,PrezzoProdotto,QuantitaProdotto,Stile,Descrizione,Volume,Gradazione,Birrificio,Nazione")] Prodotto prodotto)
        {
            ModelState.Remove("ProdottoAcquistato");
            if (id != prodotto.IdProdotto)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(prodotto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProdottoExists(prodotto.IdProdotto))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(prodotto);
        }

        // GET: Prodottoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prodotto = await _context.Prodotto
                .FirstOrDefaultAsync(m => m.IdProdotto == id);
            if (prodotto == null)
            {
                return NotFound();
            }

            return View(prodotto);
        }

        // POST: Prodottoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prodotto = await _context.Prodotto.FindAsync(id);
            if (prodotto != null)
            {
                _context.Prodotto.Remove(prodotto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProdottoExists(int id)
        {
            return _context.Prodotto.Any(e => e.IdProdotto == id);
        }
    }
}
