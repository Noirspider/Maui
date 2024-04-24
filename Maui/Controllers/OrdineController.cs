using Maui.Class;
using Maui.Data;
using Maui.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Maui.Controllers
{
    
    public class OrdineController : Controller
    {
        private readonly ApplicationDbContext Dbcontext;

        public OrdineController(ApplicationDbContext context)
        {
            Dbcontext = context;
        }

        // GET: Ordine
        public async Task<IActionResult> Index()
        {
            try
            {
                Console.WriteLine("Inizio del metodo Index.");
                var applicationDbContext = await Dbcontext.Ordine
                    .Include(o => o.Utente)
                    .ToListAsync();

                if (!applicationDbContext.Any())
                {
                    Console.WriteLine("Non ci sono ordini nel database.");
                    // Puoi decidere cosa fare in questo caso. Forse vuoi restituire una vista diversa,
                    // o forse vuoi semplicemente passare la lista vuota alla vista e gestire il caso lì.
                }

                Console.WriteLine("Fine del metodo Index.");
                return View(applicationDbContext);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Si è verificato un errore durante l'esecuzione del metodo Index: {ex}");
                throw;
            }
        }

        public async Task<IActionResult> ToggleIsEvaso(int id)
        {
            var ordine = await Dbcontext.Ordine.FindAsync(id);

            if (ordine != null)
            {
                ordine.IsEvaso = !ordine.IsEvaso;
                Dbcontext.Update(ordine);
                await Dbcontext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Ordine/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordine = await Dbcontext
                .Ordine.Include(o => o.Utente)
                .FirstOrDefaultAsync(m => m.IdOrdine == id);
            if (ordine == null)
            {
                return NotFound();
            }

            return View(ordine);
        }

        // GET: Ordine/Create
        public IActionResult Create()
        {
            ViewData["IdUtente"] = new SelectList(Dbcontext.Utente, "IdUtente", "Username");
            return View();
        }

        // POST: Ordine/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("IdUtente,IndirizzoDiConsegna,DataOrdine,IsEvaso,Nota")] Ordine ordine
        )
        {
            ModelState.Remove("Cliente");
            ModelState.Remove("ProdottoAcquistato");
            if (ModelState.IsValid)
            {
                Dbcontext.Add(ordine);
                await Dbcontext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdUtente"] = new SelectList(
                Dbcontext.Utente,
                "IdUtente",
                "Password",
                ordine.IdUtente
            );
            return View(ordine);
        }

        // GET: Ordine/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordine = await Dbcontext.Ordine.FindAsync(id);
            if (ordine == null)
            {
                return NotFound();
            }
            ViewData["IdUtente"] = new SelectList(
                Dbcontext.Utente,
                "IdUtente",
                "Password",
                ordine.IdUtente
            );
            return View(ordine);
        }

        // POST: Ordine/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("IdOrdine,IdUtente,IndirizzoDiConsegna,DataOrdine,IsEvaso,Nota")] Ordine ordine
        )
        {
            if (id != ordine.IdOrdine)
            {
                return NotFound();
            }
            ModelState.Remove("Utente");
            ModelState.Remove("ProdottoAcquistato");

            if (ModelState.IsValid)
            {
                try
                {
                    Dbcontext.Update(ordine);
                    await Dbcontext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdineExists(ordine.IdOrdine))
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
            ViewData["IdUtente"] = new SelectList(
                Dbcontext.Utente,
                "IdUtente",
                "Username",
                ordine.IdUtente
            );
            return View(ordine);
        }

        // GET: Ordine/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordine = await Dbcontext
                .Ordine.Include(o => o.Utente)
                .FirstOrDefaultAsync(m => m.IdOrdine == id);
            if (ordine == null)
            {
                return NotFound();
            }

            return View(ordine);
        }

        // POST: Ordine/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ordine = await Dbcontext.Ordine.FindAsync(id);
            if (ordine != null)
            {
                Dbcontext.Ordine.Remove(ordine);
            }

            await Dbcontext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrdineExists(int id)
        {
            return Dbcontext.Ordine.Any(e => e.IdOrdine == id);
        }
    }
}
