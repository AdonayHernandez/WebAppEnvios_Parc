using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebAppEnvios.Data;
using WebAppEnvios.Models;

namespace WebAppEnvios.Controllers
{
    [Authorize]
    public class PaquetesController : Controller
    {
        private readonly AppDbContext _context;

        public PaquetesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Paquetes
        public async Task<IActionResult> Index()
        {
            var paquetesQuery = _context.Paquetes.Include(p => p.Envio);
            
            if (User.IsInRole("Administrador"))
            {
                return View(await paquetesQuery.ToListAsync());
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
            
            if (cliente == null)
            {
                return View(new List<Paquete>());
            }

            return View(await paquetesQuery.Where(p => p.Envio.ClienteId == cliente.ClienteId).ToListAsync());
        }

        // GET: Paquetes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paquete = await _context.Paquetes
                .Include(p => p.Envio)
                .FirstOrDefaultAsync(m => m.PaqueteId == id);
                
            if (paquete == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Administrador"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cliente == null || paquete.Envio.ClienteId != cliente.ClienteId)
                {
                    return Forbid();
                }
            }

            return View(paquete);
        }

        // GET: Paquetes/Create
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Administrador"))
            {
                ViewData["EnvioId"] = new SelectList(_context.Envios, "EnvioId", "EnvioId");
            }
            else
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cliente != null)
                {
                    ViewData["EnvioId"] = new SelectList(_context.Envios.Where(e => e.ClienteId == cliente.ClienteId), "EnvioId", "EnvioId");
                }
                else
                {
                    ViewData["EnvioId"] = new SelectList(new List<Envio>(), "EnvioId", "EnvioId");
                }
            }
            
            return View();
        }

        // POST: Paquetes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PaqueteId,EnvioId,Peso")] Paquete paquete)
        {
            var envio = await _context.Envios.FindAsync(paquete.EnvioId);
            if (envio == null)
            {
                 ModelState.AddModelError("EnvioId", "El envío seleccionado no es válido.");
            }
            else if (!User.IsInRole("Administrador"))
            {
                 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                 var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                 if (cliente == null || envio.ClienteId != cliente.ClienteId)
                 {
                     return Forbid();
                 }
            }
        
            ModelState.Remove("Envio");

            if (ModelState.IsValid)
            {
                _context.Add(paquete);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            if (User.IsInRole("Administrador"))
            {
                ViewData["EnvioId"] = new SelectList(_context.Envios, "EnvioId", "EnvioId", paquete.EnvioId);
            }
            else
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                ViewData["EnvioId"] = new SelectList(cliente != null ? _context.Envios.Where(e => e.ClienteId == cliente.ClienteId) : Enumerable.Empty<Envio>(), "EnvioId", "EnvioId", paquete.EnvioId);
            }

            return View(paquete);
        }

        // GET: Paquetes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paquete = await _context.Paquetes.Include(p => p.Envio).FirstOrDefaultAsync(p => p.PaqueteId == id);
            if (paquete == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Administrador"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cliente == null || paquete.Envio.ClienteId != cliente.ClienteId)
                {
                    return Forbid();
                }
                ViewData["EnvioId"] = new SelectList(_context.Envios.Where(e => e.ClienteId == cliente.ClienteId), "EnvioId", "EnvioId", paquete.EnvioId);
            }
            else
            {
                ViewData["EnvioId"] = new SelectList(_context.Envios, "EnvioId", "EnvioId", paquete.EnvioId);
            }
            
            return View(paquete);
        }

        // POST: Paquetes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PaqueteId,EnvioId,Peso")] Paquete paquete)
        {
            if (id != paquete.PaqueteId)
            {
                return NotFound();
            }

            var envio = await _context.Envios.FindAsync(paquete.EnvioId);
            if (envio == null)
            {
                 ModelState.AddModelError("EnvioId", "El envío seleccionado no es válido.");
            }
            else if (!User.IsInRole("Administrador"))
            {
                 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                 var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                 if (cliente == null || envio.ClienteId != cliente.ClienteId)
                 {
                     return Forbid();
                 }
            }
        
            ModelState.Remove("Envio");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paquete);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaqueteExists(paquete.PaqueteId))
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

            if (User.IsInRole("Administrador"))
            {
                ViewData["EnvioId"] = new SelectList(_context.Envios, "EnvioId", "EnvioId", paquete.EnvioId);
            }
            else
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                ViewData["EnvioId"] = new SelectList(cliente != null ? _context.Envios.Where(e => e.ClienteId == cliente.ClienteId) : Enumerable.Empty<Envio>(), "EnvioId", "EnvioId", paquete.EnvioId);
            }

            return View(paquete);
        }

        // GET: Paquetes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paquete = await _context.Paquetes
                .Include(p => p.Envio)
                .FirstOrDefaultAsync(m => m.PaqueteId == id);
            if (paquete == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Administrador"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cliente == null || paquete.Envio.ClienteId != cliente.ClienteId)
                {
                    return Forbid();
                }
            }

            return View(paquete);
        }

        // POST: Paquetes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paquete = await _context.Paquetes.Include(p => p.Envio).FirstOrDefaultAsync(p => p.PaqueteId == id);
            if (paquete != null)
            {
                if (!User.IsInRole("Administrador"))
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (cliente == null || paquete.Envio.ClienteId != cliente.ClienteId)
                    {
                        return Forbid();
                    }
                }
                
                _context.Paquetes.Remove(paquete);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaqueteExists(int id)
        {
            return _context.Paquetes.Any(e => e.PaqueteId == id);
        }
    }
}
