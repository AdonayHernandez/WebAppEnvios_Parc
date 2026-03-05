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
    public class DestinatariosController : Controller
    {
        private readonly AppDbContext _context;

        public DestinatariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Destinatarios
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Administrador"))
            {
                return View(await _context.Destinatarios.Include(d => d.Cliente).ToListAsync());
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cliente == null)
            {
                return View(new List<Destinatario>());
            }

            return View(await _context.Destinatarios.Where(d => d.ClienteId == cliente.ClienteId).ToListAsync());
        }

        // GET: Destinatarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var destinatario = await _context.Destinatarios
                .Include(d => d.Cliente)
                .FirstOrDefaultAsync(m => m.DestinatarioId == id);
                
            if (destinatario == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Administrador"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cliente == null || destinatario.ClienteId != cliente.ClienteId)
                {
                    return Forbid();
                }
            }

            return View(destinatario);
        }

        // GET: Destinatarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Destinatarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DestinatarioId,Nombre,Telefono,Direccion,Ciudad,Pais")] Destinatario destinatario)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
            
            if (cliente == null)
            {
                cliente = new Cliente
                {
                    UserId = userId,
                    Nombre = User.Identity.Name ?? "Usuario Anónimo",
                    Email = User.Identity.Name ?? "sin_correo",
                    Direccion = "No registrada",
                    Telefono = "No registrado"
                };
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
            }

            destinatario.ClienteId = cliente.ClienteId;
            
            ModelState.Remove("Cliente");
            ModelState.Remove("Envios");

            if (ModelState.IsValid)
            {
                _context.Add(destinatario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(destinatario);
        }

        // GET: Destinatarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var destinatario = await _context.Destinatarios.FindAsync(id);
            if (destinatario == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Administrador"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cliente == null || destinatario.ClienteId != cliente.ClienteId)
                {
                    return Forbid();
                }
            }

            return View(destinatario);
        }

        // POST: Destinatarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DestinatarioId,Nombre,Telefono,Direccion,Ciudad,Pais,ClienteId")] Destinatario destinatario)
        {
            if (id != destinatario.DestinatarioId)
            {
                return NotFound();
            }

            if (!User.IsInRole("Administrador"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cliente == null || destinatario.ClienteId != cliente.ClienteId)
                {
                    return Forbid();
                }
            }

            ModelState.Remove("Cliente");
            ModelState.Remove("Envios");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(destinatario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DestinatarioExists(destinatario.DestinatarioId))
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
            return View(destinatario);
        }

        // GET: Destinatarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var destinatario = await _context.Destinatarios
                .Include(d => d.Cliente)
                .FirstOrDefaultAsync(m => m.DestinatarioId == id);
            if (destinatario == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Administrador"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cliente == null || destinatario.ClienteId != cliente.ClienteId)
                {
                    return Forbid();
                }
            }

            return View(destinatario);
        }

        // POST: Destinatarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var destinatario = await _context.Destinatarios.FindAsync(id);
            if (destinatario != null)
            {
                if (!User.IsInRole("Administrador"))
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (cliente == null || destinatario.ClienteId != cliente.ClienteId)
                    {
                        return Forbid();
                    }
                }

                _context.Destinatarios.Remove(destinatario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DestinatarioExists(int id)
        {
            return _context.Destinatarios.Any(e => e.DestinatarioId == id);
        }
    }
}
