using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Maui.Data;
using Maui.Models;

namespace Maui.Controllers
{
    public class OrdinesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdinesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ordines
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Ordine.Include(o => o.Utente);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Ordines/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordine = await _context.Ordine
                .Include(o => o.Utente)
                .FirstOrDefaultAsync(m => m.IdOrdine == id);
            if (ordine == null)
            {
                return NotFound();
            }

            return View(ordine);
        }

        // GET: Ordines/Create
        public IActionResult Create()
        {
            ViewData["IdUtente"] = new SelectList(_context.Utente, "IdUtente", "Password");
            return View();
        }

        // POST: Ordines/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdOrdine,IdUtente,IndirizzoDiConsegna,DataOrdine,IsEvaso,Nota")] Ordine ordine)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ordine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdUtente"] = new SelectList(_context.Utente, "IdUtente", "Password", ordine.IdUtente);
            return View(ordine);
        }

        // GET: Ordines/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordine = await _context.Ordine.FindAsync(id);
            if (ordine == null)
            {
                return NotFound();
            }
            ViewData["IdUtente"] = new SelectList(_context.Utente, "IdUtente", "Password", ordine.IdUtente);
            return View(ordine);
        }

        // POST: Ordines/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdOrdine,IdUtente,IndirizzoDiConsegna,DataOrdine,IsEvaso,Nota")] Ordine ordine)
        {
            if (id != ordine.IdOrdine)
            {
                return NotFound();
            }

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
            ViewData["IdUtente"] = new SelectList(_context.Utente, "IdUtente", "Password", ordine.IdUtente);
            return View(ordine);
        }

        // GET: Ordines/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordine = await _context.Ordine
                .Include(o => o.Utente)
                .FirstOrDefaultAsync(m => m.IdOrdine == id);
            if (ordine == null)
            {
                return NotFound();
            }

            return View(ordine);
        }

        // POST: Ordines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ordine = await _context.Ordine.FindAsync(id);
            if (ordine != null)
            {
                _context.Ordine.Remove(ordine);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrdineExists(int id)
        {
            return _context.Ordine.Any(e => e.IdOrdine == id);
        }
    }
}
