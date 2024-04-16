using Maui.Class;
using Maui.Data;
using Maui.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Maui.Controllers
{
    [Authorize(Roles = UserRole.ADMIN)]
    public class OrdineController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdineController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ordine
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Ordini.Include(o => o.Utente);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> ToggleIsEvaso(int id)
        {
            var ordine = await _context.Ordini.FindAsync(id);

            if (ordine != null)
            {
                ordine.IsEvaso = !ordine.IsEvaso;
                _context.Update(ordine);
                await _context.SaveChangesAsync();
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

            var ordine = await _context
                .Ordini.Include(o => o.Utente)
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
            ViewData["IdUtente"] = new SelectList(_context.Utente, "IdUtente", "Username");
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
            ModelState.Remove("ProdottiAcquistati");
            if (ModelState.IsValid)
            {
                _context.Add(ordine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdUtente"] = new SelectList(
                _context.Utente,
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

            var ordine = await _context.Ordini.FindAsync(id);
            if (ordine == null)
            {
                return NotFound();
            }
            ViewData["IdUtente"] = new SelectList(
                _context.Utente,
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
            ModelState.Remove("ProdottiAcquistati");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ordine);
                    await _context.SaveChangesAsync();
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
                _context.Utente,
                "IdUtente",
                "Username",
                ordine.IdUtente
            );
            return View();
        }

        // GET: Ordine/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordine = await _context
                .Ordini.Include(o => o.Utente)
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
            var ordine = await _context.Ordini.FindAsync(id);
            if (ordine != null)
            {
                _context.Ordini.Remove(ordine);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrdineExists(int id)
        {
            return _context.Ordini.Any(e => e.IdOrdine == id);
        }
    }
}
